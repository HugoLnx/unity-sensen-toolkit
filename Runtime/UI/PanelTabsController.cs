#if DOTWEEN
using System;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace SensenToolkit
{
    [System.Serializable]
    public struct PanelTabConfig
    {
        public RadioButton TabButton;
        public PanelFadable Panel;
    }

    public class PanelTabsController : MonoBehaviour
    {
        [SerializeField] private PanelTabConfig[] _tabs;
        [SerializeField, AutoProperty(AutoPropertyMode.Children)]
        private RadioButtonGroup _tabGroup;

        private void OnEnable()
        {
            _tabGroup.OnStateChanged += OnTabStateChanged;
        }

        private void OnDisable()
        {
            _tabGroup.OnStateChanged -= OnTabStateChanged;
        }

        private void OnTabStateChanged()
        {
            RadioButton activeBtn = _tabGroup.ActiveButton;
            foreach (PanelTabConfig cfg in _tabs)
            {
                if (cfg.TabButton == activeBtn) cfg.Panel.Show();
                else cfg.Panel.Hide();
            }
        }
    }
}
#endif
