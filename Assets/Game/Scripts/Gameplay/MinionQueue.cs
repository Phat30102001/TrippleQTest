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
        [SerializeField] private MinionRowAgent rowAgent;

        private readonly Queue<MinionRowAgent> _rowGroups = new Queue<MinionRowAgent>();
        private Conveyor _conveyor;
        [SerializeField] private MinionAgent minionPrefab;
        private ColorPalette _palette;
        private List<MinionRowData> _unsetRowData = new List<MinionRowData>();
        [Header("Minion preload config")]
        // this value will load more minions from pool manager
        // if queue have 20 or less row, load more minions from pool manager, unntil it reach maxPreloadCount
        [SerializeField] private int minloadCount = 10;
        
        // this value for load when start game 
        // level data have 100 row, maxPreloadCount is 50 => only instantiate 50 minions
        [SerializeField] private int maxPreloadCount = 20;
        private int _loadedRowCount = 0;
        
        // public event Action<int> OnRowsChanged;

        public int RemainingRowCount => _rowGroups.Count;
        public bool IsEmpty => _rowGroups.Count == 0;
        [Header("Cache data")]
        private List<MinionRowData> _cacheRowData = new List<MinionRowData>();
        private MinionColor _currentMinionColorGroup;
        private int _currentMinionColorGroupSum = 0;
        private int _currentMinionColorGroupIndex = 0;
        private int _highestMinionRowSpawned = 0;
        private Color _cachedDisplayColor;

        public void Initialize(MinionQueueData data, Conveyor conveyor, ColorPalette palette)
        {
            _cacheRowData.AddRange(data.rows);
            _conveyor = conveyor;
            _palette = palette;

            // Clean up existing
            while (_rowGroups.Count > 0)
            {
                var row = _rowGroups.Dequeue();
                if (row != null && PoolManager.Instance != null)
                    // PoolManager.Instance.Return(rowPrefab, row.gameObject);
                    PoolManager.Instance.Deposit(row);

            }

            if (data.rows == null) return;

            int rowIndex = 0;
            // foreach (var rowData in data.rows)
            for (int i = 0; i < data.rows.Count; i++)
            {
                var rowData = data.rows[i];
                for (int j = 0; j < rowData.rowAmount; j++)
                {
                    if (_rowGroups.Count >= maxPreloadCount)
                    {
                        // Debug.Log("Preload count reached max");
                        break;
                    }

                    var rowGo = BuildMinionRow();
                    rowGo.BuildMinion(minionsPerRow);
                    rowGo.SetData(rowData.color, _palette.GetColor(rowData.color),minionsPerRow);

                    // if (minionsPerRow > rowGo.Minions.Count)
                    // {
                    //     for (int k = 0; k < minionsPerRow; k++)
                    //     {
                    //         var minionGo = Instantiate(minionPrefab, rowGo.transform);
                    //         rowGo.AddMinion(minionGo);
                    //     }
                    // }
                    //
                    // var minionList = rowGo.Minions;
                    // for (int k = 0; k < minionsPerRow; k++)
                    // {
                    //     float hOffset = (k - (minionsPerRow - 1) * 0.5f) * minionHorizontalOffset;
                    //     Vector3 minionLocalPos = new Vector3(hOffset, 0, 0);
                    //     minionList[k].transform.position = rowGo.transform.TransformPoint(minionLocalPos);
                    //     minionList[k].transform.rotation = rowGo.transform.rotation;
                    //
                    //     minionList[k].PrefabOrigin = minionPrefab.gameObject;
                    //     minionList[k].SetData(null, 0, 0, rowData.color, _palette.GetColor(rowData.color));
                    //
                    //     // Ensure minion is correctly positioned locally within the row
                    //     minionList[k].transform.localPosition = minionLocalPos;
                    // }


                    _rowGroups.Enqueue(rowGo);
                    _loadedRowCount++;
                    rowIndex++;
                    _highestMinionRowSpawned++;
                }

                if (i < data.rows.Count - 1)
                    UpdateCurrentMinionColorGroup();
            }

            // OnRowsChanged?.Invoke(_rowGroups.Count);
        }

        public MinionColor GetMinionColor(int row)
        {
            if (_currentMinionColorGroupSum >= row)
                return _currentMinionColorGroup;
            else
            {
                // get next group color
                UpdateCurrentMinionColorGroup();
                return _currentMinionColorGroup;
            }
        }

        private void UpdateCurrentMinionColorGroup()
        {
            _currentMinionColorGroupIndex++;
            _currentMinionColorGroupSum += _cacheRowData[_currentMinionColorGroupIndex].rowAmount;
            _currentMinionColorGroup = _cacheRowData[_currentMinionColorGroupIndex].color;
            _cachedDisplayColor = _palette.GetColor(_currentMinionColorGroup);
        }

        private bool CheckReachMinLoadCount()
        {
            return _rowGroups.Count >= minloadCount;
        }

        private MinionRowAgent BuildMinionRow()
        {
            // Spawn row parent at origin, then move to local position
            // var rowGo = PoolManager.Instance.Get(rowPrefab, transform.position, transform.rotation, transform);
            var rowGo = PoolManager.Instance.TryWithraw();
            // rowGo.name = $"Queued_Row_{rowData.color}";
            if (rowGo == null)
                rowGo = Instantiate(rowAgent, transform);
            rowGo.transform.SetParent(transform);
            // Force exact local position to avoid scale/rotation distortions
            rowGo.transform.localPosition = new Vector3(0, groundYOffset, -_rowGroups.Count * minionSpacing);
            rowGo.transform.localRotation = Quaternion.identity;
            // if (!rowGo.TryGetComponent(out MinionRowAgent rowAgent))
            //     rowAgent = rowGo.AddComponent<MinionRowAgent>();

            rowGo.RowPrefabOrigin = rowAgent.gameObject;
            return rowGo;
        }

        private void SetDataForMinionRow(MinionRowAgent row)
        {
            MinionColor color = GetMinionColor(_highestMinionRowSpawned);
            row.SetData(color, _cachedDisplayColor,minionsPerRow);
        }

        public bool TryPushRow()
        {
            if (IsEmpty || !_conveyor) return false;

            var row = _rowGroups.Dequeue();
            _conveyor.AddRowFromQueue(row);

            UpdateQueueVisuals();
            // OnRowsChanged?.Invoke(_rowGroups.Count);
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
