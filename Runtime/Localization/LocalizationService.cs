using System;
using EasyButtons;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace SensenToolkit
{
    public class LocalizationService : APermanentSingleton<LocalizationService>
    {
        [SerializeField] private Locale _defaultLocale;
        [SerializeField] private Locale _enLocale;
        public Locale CurrentLocale => LocalizationSettings.SelectedLocale;

        protected override void AwakeSingleton()
        {
            base.AwakeSingleton();
            EnforceDefaultLocale();
        }

        public void EnforceDefaultLocale() => SetLocale(_defaultLocale);
        public void EnforceEnLocale() => SetLocale(_enLocale);

        [Button]
        public void SetLocale(Locale locale)
        {
            LocalizationSettings.SelectedLocale = locale;
        }

        public void ForceLocaleWithCode(string code)
        {
            Locale locale = LocalizationSettings.AvailableLocales.GetLocale(new LocaleIdentifier(code));
            if (locale != null)
            {
                SetLocale(locale);
            }
            else
            {
                Debug.LogWarning($"[LocalizationService] Locale with code '{code}' not found. Available locales: {string.Join(", ", LocalizationSettings.AvailableLocales.Locales)}");
            }
        }
    }
}
