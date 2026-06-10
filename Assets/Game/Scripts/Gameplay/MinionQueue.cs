using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PeopleFlow.Data;
using PeopleFlow.Core;

namespace PeopleFlow.Gameplay
{
    /// <summary>
    /// Represents a queue of minions waiting to be pushed onto the conveyor belt.
    /// Minions are pre-spawned and kept in a visual 'tube'.
    /// </summary>
    public class MinionQueue : MonoBehaviour
    {
        [Header("Positioning Settings")]
        [SerializeField] private float minionSpacing = 1.2f;
        [SerializeField] private float minionHorizontalOffset = 0.8f;
        [SerializeField] private float groundYOffset = 0.75f;
        [SerializeField] private float moveForwardDuration = 0.3f;
        [SerializeField] private int minionsPerRow = 4;
        
        [Header("Row Prefab")]
        [SerializeField] private GameObject rowPrefab;

        private readonly Queue<MinionRowAgent> _rowGroups = new Queue<MinionRowAgent>();
        private Conveyor _conveyor;
        private GameObject _minionPrefab;
        private ColorPalette _palette;

        public event Action<int> OnRowsChanged;

        public int RemainingRowCount => _rowGroups.Count;
        public bool IsEmpty => _rowGroups.Count == 0;

        public void Initialize(MinionQueueData data, Conveyor conveyor, GameObject minionPrefab, ColorPalette palette)
        {
            _conveyor = conveyor;
            _minionPrefab = minionPrefab;
            _palette = palette;

            // Clean up existing
            while (_rowGroups.Count > 0)
            {
                var row = _rowGroups.Dequeue();
                if (row != null && PoolManager.Instance != null)
                    PoolManager.Instance.Return(rowPrefab, row.gameObject);
            }

            if (data.rows == null) return;

            int rowIndex = 0;
            foreach (var rowData in data.rows)
            {
                for (int i = 0; i < rowData.count; i++)
                {
                    // Spawn row parent at origin, then move to local position
                    var rowGo = PoolManager.Instance.Get(rowPrefab, transform.position, transform.rotation, transform);
                    rowGo.name = $"Queued_Row_{rowData.color}";
                    
                    // Force exact local position to avoid scale/rotation distortions
                    rowGo.transform.localPosition = new Vector3(0, groundYOffset, -rowIndex * minionSpacing);
                    
                    if (!rowGo.TryGetComponent(out MinionRowAgent rowAgent))
                        rowAgent = rowGo.AddComponent<MinionRowAgent>();
                        
                    rowAgent.RowPrefabOrigin = rowPrefab;
                    rowAgent.Initialize(rowData.color);

                    for (int j = 0; j < minionsPerRow; j++)
                    {
                        float hOffset = (j - (minionsPerRow - 1) * 0.5f) * minionHorizontalOffset;
                        Vector3 minionLocalPos = new Vector3(hOffset, 0, 0);
                        
                        var minionGo = PoolManager.Instance.Get(_minionPrefab, rowGo.transform.TransformPoint(minionLocalPos), rowGo.transform.rotation, rowGo.transform);
                        if (!minionGo.TryGetComponent(out MinionAgent agent))
                            agent = minionGo.AddComponent<MinionAgent>();
                            
                        agent.PrefabOrigin = _minionPrefab;
                        agent.Initialize(null, 0, 0, rowData.color, _palette.GetColor(rowData.color));
                        
                        rowAgent.AddMinion(agent);
                        // Ensure minion is correctly positioned locally within the row
                        minionGo.transform.localPosition = minionLocalPos;
                    }
                    
                    _rowGroups.Enqueue(rowAgent);
                    rowIndex++;
                }
            }
            
            OnRowsChanged?.Invoke(_rowGroups.Count);
        }

        public bool TryPushRow()
        {
            if (IsEmpty || !_conveyor) return false;

            var row = _rowGroups.Dequeue();
            _conveyor.AddRowFromQueue(row);

            UpdateQueueVisuals();
            OnRowsChanged?.Invoke(_rowGroups.Count);
            return true;
        }

        private void UpdateQueueVisuals()
        {
            int rowIndex = 0;
            foreach (var row in _rowGroups)
            {
                Vector3 targetLocalPos = new Vector3(0, groundYOffset, -rowIndex * minionSpacing);
                StartCoroutine(SmoothMoveTo(row.transform, targetLocalPos, moveForwardDuration));
                rowIndex++;
            }
        }

        private IEnumerator SmoothMoveTo(Transform target, Vector3 targetLocalPos, float duration)
        {
            Vector3 startPos = target.localPosition;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (target == null) yield break;
                elapsed += Time.deltaTime;
                target.localPosition = Vector3.Lerp(startPos, targetLocalPos, elapsed / duration);
                yield return null;
            }
            if (target != null) target.localPosition = targetLocalPos;
        }
    }
}
