using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace SensenToolkit
{

    [CreateAssetMenu(fileName = "LocalizedFont", menuName = "Sensen/Localization/LocalizedFontSO", order = 1)]
    public class LocalizedFontSO : ScriptableObject
    {
        [field: SerializeField] public LocalizedFontLocaleConfig[] Fonts { get; private set; } = new LocalizedFontLocaleConfig[0];

        private void OnValidate()
        {
            HashSet<Locale> missingLocales = new(LocalizationSettings.AvailableLocales.Locales);
            HashSet<Locale> includedLocales = new();
            foreach (LocalizedFontLocaleConfig font in Fonts)
            {
                foreach (Locale locale in font.Locales)
                {
                    if (locale == null) continue;

                    if (includedLocales.Contains(locale))
                    {
                        Debug.LogError($"[LocalizedFontSO:{name}] Locale {locale} is included multiple times.");
                    }

                    missingLocales.Remove(locale);
                    includedLocales.Add(locale);
                }
            }

            if (missingLocales.Count > 0)
            {
                Debug.LogError($"[LocalizedFontSO:{name}] Missing locales: {string.Join(", ", missingLocales)}");
            }
        }
    }
}
