using System;
using MyBox;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace SensenToolkit
{
    public class LocalizationTrigger : MonoBehaviour
    {
        [SerializeField, AutoProperty(AutoPropertyMode.Parent)] private PanelFadable _parentPanel;
        private bool IsCritical => _parentPanel == null || _parentPanel.IsVisible;
        public event Action<Locale> OnLocalizationTriggered = delegate { };

        private void OnEnable()
        {
            TriggerWhenCritical();
            LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
            if (_parentPanel != null)
            {
                _parentPanel.OnPrepareToShow += OnPrepareToShow;
            }
        }

        private void OnDisable()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
            if (_parentPanel != null)
            {
                _parentPanel.OnPrepareToShow -= OnPrepareToShow;
            }
        }

        public void Subscribe(Action<Locale> action)
        {
            OnLocalizationTriggered += action;
            TriggerWhenCritical();
        }

        public void Unsubscribe(Action<Locale> action)
        {
            OnLocalizationTriggered -= action;
        }

        private void OnSelectedLocaleChanged(Locale locale)
        {
            TriggerWhenCritical();
        }

        private void OnPrepareToShow(PanelFadable _)
        {
            TriggerLocalization();
        }

        private void TriggerWhenCritical()
        {
            if (!IsCritical) return;
            TriggerLocalization();
        }

        private void TriggerLocalization()
        {
            OnLocalizationTriggered.Invoke(LocalizationSettings.SelectedLocale);
        }
    }
}
