using UnityEngine;
using UnityEngine.UI;
using PeopleFlow.Core;
using TMPro;

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
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI levelLabelText;
        [SerializeField] private Button manualRestartButton;

        [Header("Capacity Indicator")]
        [SerializeField] private Slider capacityIndicator;
        [SerializeField] private TextMeshProUGUI capacityValueText;
        private const int MinimumCapacityValue = 1;

        [Header("Overlay Panels")]
        [SerializeField] private GameObject winOverlay;
        [SerializeField] private GameObject failOverlay;
        [SerializeField] private TextMeshProUGUI failCauseText;
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

        // private void HandleCapacityChanged(int current, int capacity)
        // {
        //     // Debug.Log("Capacity changed: " + current + " / " + capacity);
        //     if (capacityIndicator != null)
        //     {
        //         capacityIndicator.maxValue = Mathf.Max(1, capacity);
        //         capacityIndicator.value = current;
        //
        //         // Visual feedback: Green to Red as it fills up
        //         if (capacityIndicator.fillRect != null)
        //         {
        //             var fillImage = capacityIndicator.fillRect.GetComponent<UnityEngine.UI.Image>();
        //             if (fillImage != null)
        //             {
        //                 float ratio = (float)current / capacityIndicator.maxValue;
        //                 fillImage.color = Color.Lerp(Color.green, Color.red, ratio);
        //             }
        //         }
        //     }
        //     if (capacityValueText != null)
        //         capacityValueText.text = current + " / " + capacity;
        // }
        private void HandleCapacityChanged(int current, int capacity)
        {
            // Debug.Log("Capacity changed: " + current + " / " + capacity);

            UpdateCapacityIndicator(current, capacity);
            UpdateCapacityValueText(current, capacity);
        }

        private void UpdateCapacityIndicator(int current, int capacity)
        {
            if (capacityIndicator == null)
                return;

            float maxCapacityValue = Mathf.Max(MinimumCapacityValue, capacity);
            capacityIndicator.maxValue = maxCapacityValue;
            capacityIndicator.value = current;

            UpdateCapacityFillColor(current, maxCapacityValue);
        }

        private void UpdateCapacityFillColor(int current, float maxCapacityValue)
        {
            if (capacityIndicator.fillRect == null)
                return;

            Image fillImage = capacityIndicator.fillRect.GetComponent<Image>();
            if (fillImage == null)
                return;

            float fillRatio = current / maxCapacityValue;
            fillImage.color = Color.Lerp(Color.green, Color.red, fillRatio);
        }

        private void UpdateCapacityValueText(int current, int capacity)
        {
            if (capacityValueText == null)
                return;

            capacityValueText.text = $"{current} / {capacity}";
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
