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
        private LevelController _levelController;

        [SerializeField] private int _minionsPerRow = 4;
        [SerializeField] private float _rowSpacing = 0f;
        [SerializeField] private float _movementSpeed = 7f;

        [SerializeField] private List<MinionRowAgent> _activeRows = new List<MinionRowAgent>();
        private readonly List<MinionAgent> _activeMinions = new List<MinionAgent>();

        public ConveyorPath Path => _path;
        public Conveyor NextConveyor { get; set; }

        public int CurrentOccupiedRows => _activeRows.Count;
        public bool IsFull => _levelController != null && _levelController.IsGlobalCapacityFull;
        public IReadOnlyList<MinionAgent> ActiveMinions => _activeMinions;

        [SerializeField] private GameObject startIndicatorPrefab;

        public void Init(GameObject minionPrefab, ColorPalette palette, LevelController levelController)
        {
            _minionPrefab = minionPrefab;
            _palette = palette;
            _levelController = levelController;

            if (_path == null) _path = GetComponent<ConveyorPath>();

            _activeRows.Clear();
            _activeMinions.Clear();

            SpawnIndicator();
        }

        private void SpawnIndicator()
        {
            if (startIndicatorPrefab != null)
            {
                var indicatorGo = Instantiate(startIndicatorPrefab, transform);
                var indicator = indicatorGo.GetComponent<ConveyorIndicator>();
                if (indicator != null) indicator.Initialize(this);
            }
        }

        public void AddRowFromQueue(MinionRowAgent row)
        {
            // if (IsFull) return;

            // When adding from queue, we start at distance 0 or based on occupied?
            // User wanted rows to be stacked visually? 
            // Actually, if it's a global capacity, we just check if full.
            float spawnDistance = 0f;
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

                if (_levelController != null) _levelController.RegisterRowStateChanged(1);
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

                if (_levelController != null) _levelController.RegisterRowStateChanged(-1);
            }
        }

        public void RemoveAllRows()
        {
            while (_activeRows.Count > 0)
            {
                var row = _activeRows[0];
                if (_activeRows.Remove(row))
                {
                    foreach (var minion in row.Minions)
                    {
                        _activeMinions.Remove(minion);
                    }

                    PoolManager.Instance.Deposit(row);
                    row.gameObject.SetActive(false);

                    if (_levelController != null) _levelController.RegisterRowStateChanged(-1);
                }
                
            }
        }

        public void RemoveAgent(MinionAgent agent)
        {
            _activeMinions.Remove(agent);
            // MinionRowAgent handles its own destruction/removal from conveyor when empty
        }

        public void Clear()
        {
            // remove all row
            RemoveAllRows();
        }
    }
}
