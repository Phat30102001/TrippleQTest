using PeopleFlow.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PeopleFlow.UI
{
    public class WinUI : UIBase
    {
        [SerializeField] private Button winRetryButton;

        public override void Show()
        {
            gameObject.SetActive(true);
        }

        public override void Hide()
        {
            gameObject.SetActive(false);
        }

        public override void Init(params object[] dependencies)
        {
            if (winRetryButton != null)
            {
                winRetryButton.onClick.RemoveAllListeners();
                winRetryButton.onClick.AddListener(OnRetryRequested);
            }
        }

        public override void SetData(params object[] data)
        {
            // Optional: Handle win data if needed
        }

        private void OnRetryRequested()
        {
            Hide();
            EventBus.RaiseRequestRetry();
        }
    }
}