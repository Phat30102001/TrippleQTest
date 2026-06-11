using System;
using PeopleFlow.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PeopleFlow.UI
{
    public class LoseUI : UIBase
    {
        [SerializeField] private TextMeshProUGUI failCauseText;
        [SerializeField] private Button failRetryButton;

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
            if (failRetryButton != null) 
            {
                failRetryButton.onClick.RemoveAllListeners();
                failRetryButton.onClick.AddListener(OnRetryRequested);
            }
        }

        public override void SetData(params object[] data)
        {
            if (data != null && data.Length > 0 && data[0] is string loseCause)
            {
                failCauseText.text = loseCause;
            }
        }

        private void OnRetryRequested()
        {
            EventBus.RaiseRequestRetry();
        }
    }
}
