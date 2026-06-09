using UnityEngine;
using UnityEngine.UI;
using PeopleFlow.Core;

namespace PeopleFlow.UI
{
    /// <summary>
    /// UI Controller for the in-game HUD.
    /// </summary>
    public class HudController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private TimerSystem timerSystem;

        [Header("Top Bar")]
        [SerializeField] private Text timerText;
        [SerializeField] private Text levelLabelText;
        [SerializeField] private Button manualRestartButton;

        [Header("Capacity Indicator")]
        [SerializeField] private Slider capacityIndicator;
        [SerializeField] private Text capacityValueText;

        [Header("Overlay Panels")]
        [SerializeField] private GameObject winOverlay;
        [SerializeField] private GameObject failOverlay;
        [SerializeField] private Text failCauseText;
        [SerializeField] private Button winRetryButton;
        [SerializeField] private Button failRetryButton;

        [Header("Settings")]
        [SerializeField] private string currentLevelName = "Level 001";

        private void Awake()
        {
            if (winOverlay != null) winOverlay.SetActive(false);
            if (failOverlay != null) failOverlay.SetActive(false);
            if (levelLabelText != null) levelLabelText.text = currentLevelName;

            if (manualRestartButton != null) manualRestartButton.onClick.AddListener(OnRetryRequested);
            if (winRetryButton != null) winRetryButton.onClick.AddListener(OnRetryRequested);
            if (failRetryButton != null) failRetryButton.onClick.AddListener(OnRetryRequested);
        }

        private void OnEnable()
        {
            EventBus.OnConveyorCapacityChanged += HandleCapacityChanged;
            if (timerSystem != null) timerSystem.OnTick += HandleTimerTick;
            if (gameManager != null) gameManager.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            EventBus.OnConveyorCapacityChanged -= HandleCapacityChanged;
            if (timerSystem != null) timerSystem.OnTick -= HandleTimerTick;
            if (gameManager != null) gameManager.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void HandleTimerTick(float remainingTime)
        {
            if (timerText == null) return;
            int totalSeconds = Mathf.CeilToInt(remainingTime);
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            timerText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
        }

        private void HandleCapacityChanged(int current, int capacity)
        {
            Debug.Log("Capacity changed: " + current + " / " + capacity);
            if (capacityIndicator != null)
            {
                capacityIndicator.maxValue = Mathf.Max(1, capacity);
                capacityIndicator.value = current;
            }
            if (capacityValueText != null)
                capacityValueText.text = current + " / " + capacity;
        }

        private void HandleGameStateChanged(GameSessionState state, GameFailReason reason)
        {
            if (winOverlay != null) winOverlay.SetActive(state == GameSessionState.Won);
            if (failOverlay != null) failOverlay.SetActive(state == GameSessionState.Failed);
            
            if (state == GameSessionState.Failed && failCauseText != null)
            {
                failCauseText.text = reason == GameFailReason.TimeExpired ? "TIME EXPIRED" : "CONVEYOR OVERFLOW";
            }
        }

        public void SetLevelName(string name)
        {
            currentLevelName = name;
            if (levelLabelText != null) levelLabelText.text = name;
        }

        private void OnRetryRequested()
        {
            if (gameManager != null) gameManager.RestartSession();
        }
    }
}
