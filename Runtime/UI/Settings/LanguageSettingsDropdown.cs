#if SENSEN_UI_HEAT
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace SensenToolkit
{
    public class LanguageSettingsDropdown : ADynamicSettingsDropdown
    {
        [Tooltip("If enabled, only the locales in the list will be shown. Otherwise, all available locales will be shown.")]
        [SerializeField] private bool _filterLocales = false;
        [SerializeField] private List<Locale> _onlyLocales;
        private IEnumerable<Locale> Locales => _filterLocales
            ? LocalizationSettings.AvailableLocales.Locales
            : _onlyLocales;

        public override IEnumerable<DynamicDropdownItemData> GetDropdownItems()
        {
            if (!LocalizationSettings.HasSettings) yield break;
            foreach (Locale locale in Locales)
            {
                yield return new DynamicDropdownItemData()
                {
                    Name = locale.LocaleName,
                    Key = locale.Identifier.Code
                };
            }
        }
    }
}
#endif
