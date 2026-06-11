using System;
using UnityEngine;
using PeopleFlow.Core;

namespace PeopleFlow.UI
{
    /// <summary>
    /// Logic Controller for UI orchestration. Monitors game state and tells UIManager what to show.
    /// </summary>
    public class HudController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private TimerSystem timerSystem;

        [Header("Settings")]
        [SerializeField] private string currentLevelName = "Level 001";

        private void Start()
        {
            // Initialize UIManager with dependencies
            if (UIManager.Instance != null)
            {
                UIManager.Instance.InitializePanels(timerSystem, gameManager);
                UIManager.Instance.ShowUI<GameplayUI>(currentLevelName);
            }
        }

        private void OnEnable()
        {
            if (gameManager != null) gameManager.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            if (gameManager != null) gameManager.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void HandleGameStateChanged(GameSessionState state, object[] param)
        {
            switch (state)
            {
                case GameSessionState.Playing:
                    UIManager.Instance.ShowUI<GameplayUI>(currentLevelName);
                    break;
                case GameSessionState.Failed:
                    UIManager.Instance.ShowUI<LoseUI>(param);
                    break;
                case GameSessionState.Won:
                    UIManager.Instance.ShowUI<WinUI>();
                    break;
            }
        }

        public void SetLevelName(string name)
        {
            currentLevelName = name;
            if (UIManager.Instance != null)
                UIManager.Instance.ShowUI<GameplayUI>(name);
        }
    }
}