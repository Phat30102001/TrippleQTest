using System;
using PeopleFlow.Core;
using PeopleFlow.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PeopleFlow.UI
{
    public class GameplayUI : UIBase
    {
        private TimerSystem _timerSystem;

        [Header("Top Bar")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI levelLabelText;
        [SerializeField] private Button manualRestartButton;

        [Header("Capacity Indicator")]
        [SerializeField] private Slider capacityIndicator;
        [SerializeField] private TextMeshProUGUI capacityValueText;
        private const int MinimumCapacityValue = 1;

        public override void Init(params object[] dependencies)
        {
            foreach (var dep in dependencies)
            {
                if (dep is TimerSystem ts) _timerSystem = ts;
            }

            if (manualRestartButton != null) 
            {
                manualRestartButton.onClick.RemoveAllListeners();
                manualRestartButton.onClick.AddListener(OnRetryRequested);
            }
        }

        public override void Show()
        {
            gameObject.SetActive(true);
            if (_timerSystem != null) _timerSystem.OnTick += HandleTimerTick;
            EventBus.OnConveyorCapacityChanged += HandleCapacityChanged;
        }

        public override void Hide()
        {
            if (_timerSystem != null) _timerSystem.OnTick -= HandleTimerTick;
            EventBus.OnConveyorCapacityChanged -= HandleCapacityChanged;
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_timerSystem != null) _timerSystem.OnTick -= HandleTimerTick;
            EventBus.OnConveyorCapacityChanged -= HandleCapacityChanged;
        }

        public override void SetData(params object[] data)
        {
            if (data != null && data.Length > 0 && data[0] is string levelName)
            {
                levelLabelText.text = levelName;
            }
        }
        private void OnRetryRequested()
        {
            EventBus.RaiseRequestRetry();
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
    }
}
