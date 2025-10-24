using System;
using System.Collections.Generic;
using EasyButtons;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace SensenToolkit
{
    public class LocalizationService : APermanentSingleton<LocalizationService>
    {
        [SerializeField] private LocalizationExtraDataSO _localeExtraData;
        [SerializeField] private Locale _defaultLocale;
        [SerializeField] private Locale _enLocale;
        private Dictionary<string, LocaleExtraData> _localesExtraDataDict;

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

        public LocaleExtraData GetLocaleExtraData(Locale locale)
        {
            Dictionary<string, LocaleExtraData> dataMap = GetOrBuildLocaleExtraDataDictionary();
            dataMap.TryGetValue(locale.Identifier.Code, out LocaleExtraData localeData);
            return localeData;
        }

        private Dictionary<string, LocaleExtraData> GetOrBuildLocaleExtraDataDictionary()
        {
            if (_localesExtraDataDict != null) return _localesExtraDataDict;
            _localesExtraDataDict = new Dictionary<string, LocaleExtraData>();
            foreach (LocaleExtraData localeData in _localeExtraData.LocalesData)
            {
                _localesExtraDataDict[localeData.Locale.Identifier.Code] = localeData;
            }
            return _localesExtraDataDict;
        }
    }
}
