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
            HashSet<Locale> locales = new(LocalizationSettings.AvailableLocales.Locales);
            foreach (LocalizedFontLocaleConfig font in Fonts)
            {
                if (font.Locale == null) continue;
                locales.Remove(font.Locale);
            }

            if (locales.Count > 0)
            {
                Debug.LogError($"[LocalizedFontSO:{name}] Missing locales: {string.Join(", ", locales)}");
            }
        }
    }
}
