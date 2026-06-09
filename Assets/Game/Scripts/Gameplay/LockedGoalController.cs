using System;
using UnityEngine;
using PeopleFlow.Core;

namespace PeopleFlow.Gameplay
{
    /// <summary>
    /// Controls a locked goal line, decrementing an unlock counter when any goal is completed.
    /// </summary>
    [RequireComponent(typeof(GoalLine))]
    public class LockedGoalController : MonoBehaviour
    {
        private GoalLine _goalLine;
        private int _unlockCounter;
        private bool _isUnlocked;

        public event Action<int> OnCounterChanged;

        public int UnlockCounter => _unlockCounter;
        public bool IsUnlocked => _isUnlocked;

        public void Initialize(int unlockCounter)
        {
            _goalLine = GetComponent<GoalLine>();
            _unlockCounter = Mathf.Max(0, unlockCounter);
            _isUnlocked = _unlockCounter <= 0;
            
            EventBus.OnGoalCompleted += HandleGlobalGoalCompleted;
            OnCounterChanged?.Invoke(_unlockCounter);
            
            if (_isUnlocked) _goalLine.Unlock();
        }

        private void OnDestroy()
        {
            EventBus.OnGoalCompleted -= HandleGlobalGoalCompleted;
        }

        private void HandleGlobalGoalCompleted()
        {
            if (_isUnlocked) return;
            
            _unlockCounter--;
            OnCounterChanged?.Invoke(_unlockCounter);
            
            if (_unlockCounter <= 0)
            {
                _unlockCounter = 0;
                _isUnlocked = true;
                _goalLine.Unlock();
            }
        }
    }
}
