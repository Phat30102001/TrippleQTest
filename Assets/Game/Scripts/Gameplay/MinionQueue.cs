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
        private Color _cachedDisplayColor;

        public void Initialize(MinionQueueData data, Conveyor conveyor, ColorPalette palette)
        {
            _cacheRowData=data.rows;
            _conveyor = conveyor;
            _palette = palette;

            // Clean up existing
            while (_rowGroups.Count > 0)
            {
                var row = _rowGroups.Dequeue();
                if (row != null && PoolManager.Instance != null)
                    PoolManager.Instance.Deposit(row);

            }
            ResetCacheData();
            
            // Build new minion queue
            if (data.rows == null) return;
            PrebuildMinionRow(data);
            
            Debug.Log($"MinionQueue initialized with {_rowGroups.Count} rows");
        }

        private void PrebuildMinionRow(MinionQueueData data)
        {
            for (int i = 0; i < data.rows.Count; i++)
            {
                var rowData = data.rows[i];
                for (int j = 0; j < rowData.rowAmount; j++)
                {
                    if (_rowGroups.Count >= maxPreloadCount)
                    {
                        UpdateCurrentMinionColorGroup();
                        return;
                    }

                    var rowGo = BuildMinionRow();
                    rowGo.BuildMinion(minionsPerRow);
                    rowGo.SetData(rowData.color, _palette.GetColor(rowData.color),minionsPerRow);

                    _rowGroups.Enqueue(rowGo);
                    _loadedRowCount++;
                }

                UpdateCurrentMinionColorGroup();
                if (i < data.rows.Count - 1)
                {
                    _currentMinionColorGroupIndex++;
                }
            }
        }

        private void ResetCacheData()
        {
            _currentMinionColorGroupIndex = 0;
            _currentMinionColorGroupSum = 0;
        }

        public MinionColor GetMinionColor(int row)
        {
            if (_currentMinionColorGroupSum >= row)
                return _currentMinionColorGroup;
            else
            {
                // get next group color
                _currentMinionColorGroupIndex++;
                UpdateCurrentMinionColorGroup();
                return _currentMinionColorGroup;
            }
        }

        private void UpdateCurrentMinionColorGroup()
        {

            _currentMinionColorGroupSum += _cacheRowData[_currentMinionColorGroupIndex].rowAmount;
            _currentMinionColorGroup = _cacheRowData[_currentMinionColorGroupIndex].color;
            _cachedDisplayColor = _palette.GetColor(_currentMinionColorGroup);
        }

        private bool CheckCanAddMoreRow()
        {
            bool isReachMinloadCount = _rowGroups.Count < minloadCount;
            bool isLoadAllRow = _currentMinionColorGroupIndex >= _cacheRowData.Count - 1 &&
                                _loadedRowCount >= _currentMinionColorGroupSum;
            return isReachMinloadCount && !isLoadAllRow;
        }

        private MinionRowAgent BuildMinionRow()
        {
            // Spawn row parent at origin, then move to local position
            // var rowGo = PoolManager.Instance.Get(rowPrefab, transform.position, transform.rotation, transform);
            var rowGo = PoolManager.Instance.TryWithraw();
            // rowGo.name = $"Queued_Row_{rowData.color}";
            if (rowGo == null)
            {
                rowGo = Instantiate(rowAgent, transform);
                Debug.Log("Create new row");
            }
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
            MinionColor color = GetMinionColor(_loadedRowCount);
            row.SetData(color, _cachedDisplayColor,minionsPerRow);
        }

        private void AddMoreRow()
        {
           var row= BuildMinionRow();
           row.BuildMinion(minionsPerRow);
           SetDataForMinionRow(row);
           _rowGroups.Enqueue(row);
           _loadedRowCount++;
        }

        public bool TryPushRow()
        {
            if (IsEmpty || !_conveyor) return false;

            var row = _rowGroups.Dequeue();
            _conveyor.AddRowFromQueue(row);

            UpdateQueueVisuals();
            // OnRowsChanged?.Invoke(_rowGroups.Count);
            if (CheckCanAddMoreRow())
            {
                Debug.Log("Add more row");
                AddMoreRow();
            }
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
