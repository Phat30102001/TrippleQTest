using System;
using System.Collections.Generic;
using UnityEngine;

namespace PeopleFlow.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [SerializeField] private List<UIBase> panels;
        private Dictionary<Type, UIBase> _panelDict = new Dictionary<Type, UIBase>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            foreach (var panel in panels)
            {
                if (panel != null)
                {
                    _panelDict[panel.GetType()] = panel;
                    panel.Hide();
                }
            }
        }

        public void InitializePanels(params object[] dependencies)
        {
            foreach (var panel in _panelDict.Values)
            {
                panel.Init(dependencies);
            }
        }

        public T GetUI<T>() where T : UIBase
        {
            if (_panelDict.TryGetValue(typeof(T), out var panel))
            {
                return panel as T;
            }
            return null;
        }

        public void ShowUI<T>(params object[] data) where T : UIBase
        {
            var ui = GetUI<T>();
            if (ui != null)
            {
                ui.SetData(data);
                ui.Show();
            }
        }

        public void HideUI<T>() where T : UIBase
        {
            var ui = GetUI<T>();
            if (ui != null)
            {
                ui.Hide();
            }
        }

        public void HideAll()
        {
            foreach (var panel in _panelDict.Values)
            {
                panel.Hide();
            }
        }
    }
}