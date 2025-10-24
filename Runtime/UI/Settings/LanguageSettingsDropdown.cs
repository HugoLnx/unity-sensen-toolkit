#if SENSEN_UI_HEAT
using System;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace SensenToolkit
{
    public class LanguageSettingsDropdown : ADynamicSettingsDropdown
    {
        private const string ITEM_LOCALE_NATIVE_NAME_FORMAT = "<size={0:0.##}em><b>{1}</b></size>";
        [Tooltip("If enabled, only the locales in the list will be shown. Otherwise, all available locales will be shown.")]
        [SerializeField] private bool _filterLocales = false;
        [SerializeField] private List<Locale> _onlyLocales;
        private LocalizationService _localizationService;
        private LocalizationService LocService => _localizationService = _localizationService == null
            ? LocalizationService.Instance
            : _localizationService;
        private IEnumerable<Locale> Locales => _filterLocales
            ? _onlyLocales
            : LocalizationSettings.AvailableLocales.Locales;

        protected override void Awake()
        {
            base.Awake();
            _localizationService = LocalizationService.Instance;
        }

        public override IEnumerable<DynamicDropdownItemData> GetDropdownItems()
        {
            if (!LocalizationSettings.HasSettings) yield break;
            foreach (Locale locale in Locales)
            {
                yield return new DynamicDropdownItemData()
                {
                    Name = GetPrettyLocaleName(locale),
                    Key = locale.Identifier.Code
                };
            }
        }

        private string GetPrettyLocaleName(Locale locale)
        {
            LocaleExtraData extraData = LocService.GetLocaleExtraData(locale);
            string nativeName = extraData.NativeName;
            string englishName = extraData.NativeName.Equals(extraData.EnglishName) ? null : extraData.EnglishName;
            float nativeNameFontEmSize = GetItemNativeNameFontEmSize(locale);
            float englishNameFontEmSize = GetItemEnglishNameFontEmSize(locale);
            string nativeNameFormatted = string.Format(ITEM_LOCALE_NATIVE_NAME_FORMAT, nativeNameFontEmSize, nativeName)
                .Replace(",", ".");
            string englishNameSuffix = string.IsNullOrEmpty(englishName)
                ? ""
                : String.Format("<size={0}em> / {1}</size>", englishNameFontEmSize, englishName)
                    .Replace(",", ".");
            return nativeNameFormatted + englishNameSuffix;
        }

        private float GetItemNativeNameFontEmSize(Locale locale)
        {
            return locale.Identifier.Code switch
            {
                var s when s.Contains("zh")
                    || s.Contains("kr")
                    || s.Contains("ko")
                    || s.Contains("jp")
                    || s.Contains("ja") => 1.5f,

                var s when s.Contains("th") => 1.7f,

                _ => 1f
            };
        }

        private float GetItemEnglishNameFontEmSize(Locale locale)
        {
            return locale.Identifier.Code switch
            {
                var s when s.Contains("zh")
                    || s.Contains("kr")
                    || s.Contains("ko") => 0.9f,

                var s when s.Contains("th")
                    || s.Contains("jp")
                    || s.Contains("ja") => 1.15f,

                _ => 0.85f
            };
        }
    }
}
#endif
