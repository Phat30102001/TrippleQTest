using System.Collections.Generic;
using UnityEngine;
using PeopleFlow.Core;
using PeopleFlow.Data;

namespace PeopleFlow.Gameplay
{
    /// <summary>
    /// Manages the conveyor belt system, handling minion spawning and capacity limits.
    /// </summary>
    [RequireComponent(typeof(ConveyorPath))]
    public class Conveyor : MonoBehaviour
    {
        private ConveyorPath _path;
        private ColorPalette _palette;
        private GameObject _minionPrefab;

        private int _maxCapacityRows;
        private int _minionsPerRow;
        private float _rowSpacing;
        private float _movementSpeed;

        private int _currentOccupiedRows;
        private readonly List<MinionRowAgent> _activeRows = new List<MinionRowAgent>();
        private readonly List<MinionAgent> _activeMinions = new List<MinionAgent>();

        public ConveyorPath Path => _path;
        public Conveyor NextConveyor { get; set; }

        public int CurrentOccupiedRows => _activeRows.Count;
        public int MaxCapacityRows => _maxCapacityRows;
        public bool IsFull => _activeRows.Count >= _maxCapacityRows;
        public IReadOnlyList<MinionAgent> ActiveMinions => _activeMinions;

        public void Init(GameObject minionPrefab, ColorPalette palette)
        {
            _minionPrefab = minionPrefab;
            _palette = palette;
            if (_path == null) _path = GetComponent<ConveyorPath>();
            
            _maxCapacityRows = 8;
            _minionsPerRow = 4;
            _rowSpacing = 1.5f; // Increased for rows
            _movementSpeed = 4.5f;

            _activeRows.Clear();
            _activeMinions.Clear();
            EventBus.RaiseConveyorCapacityChanged(0, _maxCapacityRows);
        }

        public void AddRowFromQueue(MinionRowAgent row)
        {
            if (IsFull) return;

            float spawnDistance = -_activeRows.Count * _rowSpacing;
            row.AnimateEntry(this, spawnDistance, _movementSpeed);
            AddRowDirectly(row);
        }

        public void AddRowDirectly(MinionRowAgent row)
        {
            if (!_activeRows.Contains(row))
            {
                _activeRows.Add(row);
                foreach (var minion in row.Minions)
                {
                    if (!_activeMinions.Contains(minion))
                        _activeMinions.Add(minion);
                }
                UpdateRowCount();
            }
        }

        public void RemoveRow(MinionRowAgent row)
        {
            if (_activeRows.Remove(row))
            {
                foreach (var minion in row.Minions)
                {
                    _activeMinions.Remove(minion);
                }
                UpdateRowCount();
            }
        }

        public void RemoveAgent(MinionAgent agent)
        {
            _activeMinions.Remove(agent);
            // Row management is handled by MinionRowAgent when its last child is removed
            UpdateRowCount();
        }

        private void UpdateRowCount()
        {
            int count = _activeRows.Count;
            if (count != _currentOccupiedRows)
            {
                _currentOccupiedRows = count;
                EventBus.RaiseConveyorCapacityChanged(_currentOccupiedRows, _maxCapacityRows);
            }
        }

        public bool TryAddRow(MinionColor color)
        {
            // Keeping for compatibility or manual spawns if needed
            return false; 
        }
    }
}
