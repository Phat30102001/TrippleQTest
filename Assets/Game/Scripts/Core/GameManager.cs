using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using PeopleFlow.Data;
namespace PeopleFlow.Core
{


    /// <summary>
    /// Manages the high-level game state and transitions.
    /// </summary>
    public class GameManager : MonoBehaviour
    {



        [SerializeField] private LevelLoader levelLoader;
        [SerializeField] private LevelListSO levelList;
        [SerializeField] private int startLevelIndex = 0;
        private int _currentLevelIndex;
        public event Action<GameSessionState, object[]> OnGameStateChanged;

        public GameSessionState CurrentState { get; private set; } = GameSessionState.Loading;
        public string FailureReason { get; private set; } = GameFailReason.None;

        private void OnEnable()
        {
            EventBus.OnConveyorOverflow += HandleOverflow;
            EventBus.OnLevelTimeOut += HandleTimeOut;
            EventBus.OnAllGoalsCleared += HandleAllCleared;
            EventBus.OnRequestRetry += HandleRetryRequest;
            EventBus.OnMovingToNextLevel += HandleMovingToNextLevel;
        }

        private void OnDisable()
        {
            EventBus.OnConveyorOverflow -= HandleOverflow;
            EventBus.OnLevelTimeOut -= HandleTimeOut;
            EventBus.OnAllGoalsCleared -= HandleAllCleared;
            EventBus.OnRequestRetry -= HandleRetryRequest;
            EventBus.OnMovingToNextLevel -= HandleMovingToNextLevel;
        }

        private void HandleRetryRequest() => RestartSession();

        public void Start()
        {
            _currentLevelIndex= startLevelIndex;
            HandleTransitionToGameplay();
        }
        private void HandleMovingToNextLevel()
        {
            _currentLevelIndex++;
            HandleTransitionToGameplay();
        }

        private void HandleTransitionToGameplay()
        {
            var levelData= levelList.GetLevel(_currentLevelIndex);
            LoadingTransition((callback => { levelLoader.SetData(_currentLevelIndex,levelData,callback); }), ((callback) =>
                {
                    CurrentState = GameSessionState.Playing;
                    FailureReason = GameFailReason.None;
                    OnGameStateChanged?.Invoke(CurrentState, new object[] { levelLoader.GetActiveLevel() });
                    callback?.Invoke();
                }),
                null);
        }


        private void LoadingTransition(Action<Action> onInitComplete, Action<Action> onPreLoad, Action onFinished)
        {
            CurrentState = GameSessionState.Loading;
            // OnGameStateChanged?.Invoke(CurrentState, new object[]
            // {

            OnGameStateChanged?.Invoke(CurrentState, new object[]
            {
                onInitComplete,
                onPreLoad,
                onFinished
            });
        }
        // public void StartSession(float timeLimit, int goalCount)
        // {
        //     CurrentState = GameSessionState.Playing;
        //     levelLoader.Init();
        //     FailureReason = GameFailReason.None;
        //     
        //     if (winCondition != null) winCondition.Initialize(goalCount);
        //     if (timerSystem != null) timerSystem.Begin(timeLimit);
        //     if (inputReader != null) inputReader.SetEnabled(true);
        //     
        //     OnGameStateChanged?.Invoke(CurrentState, null);
        // }

        private void HandleOverflow() => FailSession(GameFailReason.ConveyorOverflow);
        private void HandleTimeOut() => FailSession(GameFailReason.TimeExpired);
        private void HandleAllCleared() => WinSession();

        public void WinSession()
        {
            if (CurrentState != GameSessionState.Playing) return;

            CurrentState = GameSessionState.Won;
            EndSession();
            OnGameStateChanged?.Invoke(CurrentState, null);
        }

        public void FailSession(string loseReason)
        {
            if (CurrentState != GameSessionState.Playing) return;

            CurrentState = GameSessionState.Failed;
            FailureReason = loseReason;
            EndSession();

            OnGameStateChanged?.Invoke(CurrentState, new[] { loseReason });
        }

        private void EndSession()
        {
            levelLoader.EndLevel();
        }

        public void RestartSession()
        {
            // EventBus.Reset();
            // Scene currentScene = SceneManager.GetActiveScene();
            // SceneManager.LoadScene(currentScene.name);
            LoadingTransition(
                callback =>
                {
                    EventBus.RaisePauseGame(true);
                    levelLoader.ResetLevel(callback);
                }, 
                callback =>
                {
                    CurrentState = GameSessionState.Playing;
                    FailureReason = GameFailReason.None;
                    OnGameStateChanged?.Invoke(CurrentState, new object[] { levelLoader.GetActiveLevel() });
                    callback?.Invoke();
                }, () =>
                {
                    EventBus.RaisePauseGame(false);
                });

        }
    }
}
