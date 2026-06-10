using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PeopleFlow.Core
{
    public enum GameSessionState { Loading, Playing, Won, Failed }
    public enum GameFailReason { None, TimeExpired, ConveyorOverflow }

    /// <summary>
    /// Manages the high-level game state and transitions.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private TimerSystem timerSystem;
        [SerializeField] private GoalWinCondition winCondition;
        [SerializeField] private InputReader inputReader;

        public event Action<GameSessionState, GameFailReason> OnGameStateChanged;

        public GameSessionState CurrentState { get; private set; } = GameSessionState.Loading;
        public GameFailReason FailureReason { get; private set; } = GameFailReason.None;

        private void OnEnable()
        {
            EventBus.OnConveyorOverflow += HandleOverflow;
            EventBus.OnLevelTimeOut += HandleTimeOut;
            EventBus.OnAllGoalsCleared += HandleAllCleared;
        }

        private void OnDisable()
        {
            EventBus.OnConveyorOverflow -= HandleOverflow;
            EventBus.OnLevelTimeOut -= HandleTimeOut;
            EventBus.OnAllGoalsCleared -= HandleAllCleared;
        }

        public void StartSession(float timeLimit, int goalCount)
        {
            CurrentState = GameSessionState.Playing;
            FailureReason = GameFailReason.None;
            
            if (winCondition != null) winCondition.Initialize(goalCount);
            if (timerSystem != null) timerSystem.Begin(timeLimit);
            if (inputReader != null) inputReader.SetEnabled(true);
            
            OnGameStateChanged?.Invoke(CurrentState, FailureReason);
        }

        private void HandleOverflow() => FailSession(GameFailReason.ConveyorOverflow);
        private void HandleTimeOut() => FailSession(GameFailReason.TimeExpired);
        private void HandleAllCleared() => WinSession();

        public void WinSession()
        {
            if (CurrentState != GameSessionState.Playing) return;
            
            CurrentState = GameSessionState.Won;
            EndSession();
            OnGameStateChanged?.Invoke(CurrentState, FailureReason);
        }

        public void FailSession(GameFailReason reason)
        {
            if (CurrentState != GameSessionState.Playing) return;
            
            CurrentState = GameSessionState.Failed;
            FailureReason = reason;
            EndSession();
            OnGameStateChanged?.Invoke(CurrentState, FailureReason);
        }

        private void EndSession()
        {
            if (timerSystem != null) timerSystem.Stop();
            if (inputReader != null) inputReader.SetEnabled(false);
        }

        public void RestartSession()
        {
            // EventBus.Reset();
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }
    }
}
