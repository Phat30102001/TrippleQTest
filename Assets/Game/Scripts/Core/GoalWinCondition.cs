using System;
using UnityEngine;

namespace PeopleFlow.Core
{
    /// <summary>
    /// Checks for level win condition by counting completed goals.
    /// </summary>
    public class GoalWinCondition : MonoBehaviour
    {
       [SerializeField] private int _goalsRemaining;
        private bool _isSatisfied;

        public void SetData(int totalGoals)
        {
            _goalsRemaining = Mathf.Max(0, totalGoals);
            _isSatisfied = (_goalsRemaining == 0);
            
        }

        private void Start()
        {
            EventBus.OnGoalCompleted += HandleGoalCompleted;
        }

        private void OnDestroy()
        {
            EventBus.OnGoalCompleted -= HandleGoalCompleted;
        }

        private void HandleGoalCompleted()
        {
            if (_isSatisfied) return;

            _goalsRemaining--;
            if (_goalsRemaining <= 0)
            {
                _goalsRemaining = 0;
                _isSatisfied = true;
                EventBus.RaiseAllGoalsCleared();
            }
        }
    }
}
