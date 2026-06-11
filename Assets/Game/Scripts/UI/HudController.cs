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



        private void Start()
        {
            // Initialize UIManager with dependencies
            if (UIManager.Instance != null)
            {
                UIManager.Instance.InitializePanels(timerSystem, gameManager);

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

        private void HandleGameStateChanged(GameSessionState state, params object[] data)
        {
            switch (state)
            {
                case GameSessionState.Playing:
                    UIManager.Instance.ShowUI<GameplayUI>(data);
                    break;
                case GameSessionState.Failed:
                    UIManager.Instance.ShowUI<LoseUI>(data);
                    break;
                case GameSessionState.Won:
                    UIManager.Instance.ShowUI<WinUI>();
                    break;
                case GameSessionState.Loading:
                    UIManager.Instance.ShowUI<LoadingUI>(data);
                    break;
            }
        }
    }
}