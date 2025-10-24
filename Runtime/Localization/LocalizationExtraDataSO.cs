using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

namespace SensenToolkit
{
    [CreateAssetMenu(fileName = "LocalizationExtraDataSO", menuName = "Sensen/Localization/Extra Data")]
    public class LocalizationExtraDataSO : ScriptableObject
    {
        public LocaleExtraData[] LocalesData = new LocaleExtraData[]
        {
            new()
            {
                Locale = null,
                EnglishName = "English",
                NativeName = "English"
            },
            new()
            {
                Locale = null,
                EnglishName = "Chinese (Simplified)",
                NativeName = "简体中文"
            },
            new()
            {
                Locale = null,
                EnglishName = "Chinese (Traditional)",
                NativeName = "繁体中文"
            },
            new()
            {
                Locale = null,
                EnglishName = "French",
                NativeName = "Français"
            },
            new()
            {
                Locale = null,
                EnglishName = "German",
                NativeName = "Deutsch"
            },
            new()
            {
                Locale = null,
                EnglishName = "Italian",
                NativeName = "Italiano"
            },
            new()
            {
                Locale = null,
                EnglishName = "Japanese",
                NativeName = "日本語"
            },
            new()
            {
                Locale = null,
                EnglishName = "Korean",
                NativeName = "한국어"
            },
            new()
            {
                Locale = null,
                EnglishName = "Polish",
                NativeName = "Polski"
            },
            new()
            {
                Locale = null,
                EnglishName = "Portuguese (Brazilian)",
                NativeName = "Português Brasileiro"
            },
            new()
            {
                Locale = null,
                EnglishName = "Portuguese (European)",
                NativeName = "Português de Portugal"
            },
            new()
            {
                Locale = null,
                EnglishName = "Russian",
                NativeName = "Русский"
            },
            new()
            {
                Locale = null,
                EnglishName = "Spanish (LATAM)",
                NativeName = "Español (LATAM)"
            },
            new()
            {
                Locale = null,
                EnglishName = "Spanish (Spain)",
                NativeName = "Español (España)"
            },
            new()
            {
                Locale = null,
                EnglishName = "Thai",
                NativeName = "ภาษาไทย"
            },
            new()
            {
                Locale = null,
                EnglishName = "Turkish",
                NativeName = "Türkçe"
            },
            new()
            {
                Locale = null,
                EnglishName = "Ukrainian",
                NativeName = "Українська"
            }
        };

        private void OnValidate()
        {
            HashSet<string> allCodesConfigured = new();
            foreach (LocaleExtraData localeData in LocalesData)
            {
                allCodesConfigured.Add(localeData.Locale.Identifier.Code);
            }

            List<Locale> allLocales = UnityEngine.Localization.Settings.LocalizationSettings.AvailableLocales.Locales;
            HashSet<string> localesMissing = new();
            foreach (Locale locale in allLocales)
            {
                if (!allCodesConfigured.Contains(locale.Identifier.Code))
                {
                    localesMissing.Add(locale.Identifier.Code);
                }
            }
            if (localesMissing.Count > 0)
            {
                Debug.LogWarning($"[LocalizationExtraDataSO] The following locales are missing extra data configuration: {string.Join(", ", localesMissing)}");
            }
        }
    }
}
