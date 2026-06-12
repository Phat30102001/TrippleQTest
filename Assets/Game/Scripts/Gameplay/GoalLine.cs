using System;
using System.Collections.Generic;
using UnityEngine;
using PeopleFlow.Core;
using PeopleFlow.Data;
using TMPro;

namespace PeopleFlow.Gameplay
{
    /// <summary>
    /// Manages an ordered queue of goal gates. Promotes the next gate when one is completed.
    /// Spawns a barrier when the entire line is cleared.
    /// </summary>
    public class GoalLine : MonoBehaviour
    {
        [SerializeField] private GameObject barrierPrefab;
        [Header("Gate Instances")]
        [SerializeField] private GoalGate currentGate;
        [SerializeField] private GoalGate nextGate;

        [Header("Goal count")]
        [SerializeField] private TextMeshPro countLabel;
        
        // public event Action<GoalLine> OnLineCleared;
        // public event Action<int> OnRemainingGatesChanged;

        private IList<GoalGateData> _gatesData;
        private int _currentIndex;
        private bool _isLocked;
        
        private Conveyor _conveyor;
        private ColorPalette _palette;

        public int RemainingCount => _gatesData != null ? _gatesData.Count - _currentIndex : 0;
        public bool IsCleared => _gatesData == null || _currentIndex >= _gatesData.Count;

        public void SetBarrierPrefab(GameObject prefab)
        {
            barrierPrefab = prefab;
        }

        public void Build(IList<GoalGateData> gatesData, Conveyor conveyor, 
                          ColorPalette palette, bool startLocked = false)
        {
            _gatesData = gatesData;
            _conveyor = conveyor;
            _palette = palette;
            _isLocked = startLocked;
            _currentIndex = 0;

            if (currentGate != null)
            {
                currentGate.OnCompleted -= HandleGateCompleted;
                currentGate.OnCompleted += HandleGateCompleted;
            }

            RefreshGates();
        }

        private void RefreshGates()
        {
            countLabel.text = RemainingCount.ToString();
            if (IsCleared)
            {
                if (currentGate != null) currentGate.gameObject.SetActive(false);
                if (nextGate != null) nextGate.gameObject.SetActive(false);
                
                SpawnBarrier();
                // OnLineCleared?.Invoke(this);
                return;
            }

            // Setup current gate
            SetupGate(currentGate, _gatesData[_currentIndex], true);

            // Setup next gate (preview)
            if (_currentIndex + 1 < _gatesData.Count)
            {
                SetupGate(nextGate, _gatesData[_currentIndex + 1], false);
                nextGate.gameObject.SetActive(true);
            }
            else
            {
                if (nextGate != null) nextGate.gameObject.SetActive(false);
            }

            // OnRemainingGatesChanged?.Invoke(RemainingCount);
        }

        private void SetupGate(GoalGate gate, GoalGateData data, bool activate)
        {
            if (gate == null) return;
            
            gate.gameObject.SetActive(true);
            gate.Initialize(data, _conveyor, _palette);
            
            if (gate.Visual != null)
                gate.Visual.Bind(gate, _palette);

            if (activate && !_isLocked)
                gate.Activate();
        }

        public void Unlock()
        {
            if (!_isLocked) return;
            _isLocked = false;
            if (!IsCleared && currentGate != null)
            {
                currentGate.Activate();
            }
        }

        private void HandleGateCompleted(GoalGate gate)
        {
            EventBus.RaiseGoalCompleted();
            
            _currentIndex++;
            RefreshGates();
        }

        private void SpawnBarrier()
        {
            if (barrierPrefab == null) return;
            var barrier = Instantiate(barrierPrefab, transform.position, transform.rotation, transform);
            barrier.name = "Clear_Barrier";
        }
    }
}
