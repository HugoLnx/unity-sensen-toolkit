using System;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace SensenToolkit
{
    public enum LocalizedVisibilityMode
    {
        ShowOnMatch,
        HideOnMatch
    }
    public class LocalizedVisibility : MonoBehaviour
    {
        [SerializeField] private List<Locale> _targetLocales = new();
        [SerializeField] private LocalizedVisibilityMode _visibilityMode = LocalizedVisibilityMode.ShowOnMatch;
        [SerializeField, AutoProperty(AutoPropertyMode.Parent, allowEmpty: true)]
        private PanelFadable _parentPanel;

        private void Awake()
        {
            LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
            if (_parentPanel != null)
            {
                _parentPanel.OnPrepareToShow += OnPrepareToShowPanel;
            }
        }

        private void Start()
        {
            RefreshIfVisible();
        }

        private void OnEnable()
        {
            RefreshIfVisible();
        }

        private void OnSelectedLocaleChanged(Locale _) => RefreshIfVisible();
        private void OnPrepareToShowPanel(PanelFadable _) => Refresh();

        private void RefreshIfVisible()
        {
            if (_parentPanel != null && !_parentPanel.IsVisible) return;
            Refresh();
        }

        private void Refresh()
        {
            if (this == null || this.gameObject == null) return;
            Locale locale = LocalizationSettings.SelectedLocale;
            bool isMatch = _targetLocales.Any(l => l.Identifier.Code == locale.Identifier.Code);
            bool shouldShow = _visibilityMode == LocalizedVisibilityMode.ShowOnMatch
                ? isMatch
                : !isMatch;
            gameObject.SetActive(shouldShow);
        }
    }
}
