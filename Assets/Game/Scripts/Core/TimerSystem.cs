using System;
using UnityEngine;

namespace PeopleFlow.Core
{
    /// <summary>
    /// Manages the countdown timer for the game session.
    /// </summary>
    public class TimerSystem : MonoBehaviour
    {
        public event Action<float> OnTick;

        private float _timeRemaining;
        private bool _isActive;

        public float TimeRemaining => _timeRemaining;

        public void Begin(float durationSeconds)
        {
            _timeRemaining = Mathf.Max(0f, durationSeconds);
            _isActive = (_timeRemaining > 0f);
            OnTick?.Invoke(_timeRemaining);
        }

        public void Stop() => _isActive = false;

        private void Update()
        {
            if (!_isActive) return;

            _timeRemaining -= Time.deltaTime;
            if (_timeRemaining <= 0f)
            {
                _timeRemaining = 0f;
                _isActive = false;
                OnTick?.Invoke(_timeRemaining);
                EventBus.RaiseLevelTimeOut();
                return;
            }
            
            OnTick?.Invoke(_timeRemaining);
        }
    }
}
