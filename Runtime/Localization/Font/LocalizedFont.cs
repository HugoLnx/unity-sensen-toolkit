using System.Collections.Generic;
using EasyButtons;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;

namespace SensenToolkit
{
    [RequireComponent(typeof(TMP_Text))]
    [RequireComponent(typeof(LocalizationTrigger))]
    public class LocalizedFont : MonoBehaviour
    {
        [SerializeField] private LocalizedFontSO _font;
        [SerializeField] private LocalizedFontBaseData _baseConfig;
        [SerializeField] private LocalizedFontDataOverridesSO _overridesSO;
        [SerializeField] private LocalizedFontLocaleOverrides[] _overrides;
        [SerializeField] private LocalizedString _testLocalizedString;
        [SerializeField] private string _testString;
        private Dictionary<Locale, LocalizedFontDataToApply> _finalConfigs;
        [SerializeField, AutoProperty] private TMP_Text _text;
        [SerializeField, AutoProperty] private LocalizationTrigger _localizationTrigger;

        private Dictionary<Locale, LocalizedFontDataToApply> FinalConfigs => _finalConfigs ??= RefreshFinalConfigs();

        private void OnEnable()
        {
            _localizationTrigger.Subscribe(OnLocalizationTriggered);
        }

        private void OnDisable()
        {
            _localizationTrigger.Unsubscribe(OnLocalizationTriggered);
        }

        private void OnLocalizationTriggered(Locale _)
        {
            ApplyLocale(LocalizationSettings.SelectedLocale);
        }

        [Button]
        private void PullBaseConfig()
        {
            _baseConfig.FontSize = _text.fontSize;
            _baseConfig.Bold = _text.fontStyle.HasFlag(FontStyles.Bold);
            _baseConfig.CharacterSpacing = _text.characterSpacing;
            _baseConfig.WordSpacing = _text.wordSpacing;
            _baseConfig.LineSpacing = _text.lineSpacing;
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        [Button]
        internal void ApplyLocale(Locale locale)
        {
#if UNITY_EDITOR
            RefreshFinalConfigs();
#endif
            if (!FinalConfigs.TryGetValue(locale, out LocalizedFontDataToApply config))
            {
                Debug.LogWarning($"[AttachLocalizedFont:{name}] No config for locale {locale}");
                return;
            }
            _text.font = config.Font;
            _text.fontSize = config.FontSize;
            if (config.Bold) _text.fontStyle |= FontStyles.Bold;
            else _text.fontStyle &= ~FontStyles.Bold;
            _text.characterSpacing = config.CharacterSpacing;
            _text.wordSpacing = config.WordSpacing;
            _text.lineSpacing = config.LineSpacing;
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEditor.EditorUtility.SetDirty(_text);
#endif
        }

        [Button]
        internal void ApplyLocaleAndTestText(Locale locale)
        {
            ApplyLocale(locale);

            string str = _testString;
            if (string.IsNullOrEmpty(str))
            {
                LocalizedString localizedString = null;
                if (_testLocalizedString != null && !_testLocalizedString.IsEmpty)
                {
                    localizedString = _testLocalizedString;
                }
                else if (TryGetComponent(out LocalizedText locText))
                {
                    localizedString = locText.LocalizedString;
                }
                else if (TryGetComponent(out LocalizeStringEvent locStringEvent))
                {
                    localizedString = locStringEvent.StringReference;
                }

                if (localizedString != null && !localizedString.IsEmpty)
                {
                    localizedString.LocaleOverride = locale;
                    str = localizedString.GetLocalizedString();
                    localizedString.LocaleOverride = null;
                }
            }

            if (string.IsNullOrEmpty(str)) return;
            _text.text = str;
        }

        private Dictionary<Locale, LocalizedFontDataToApply> RefreshFinalConfigs()
        {
            Dictionary<Locale, LocalizedFontLocaleOverrides> overrides = BuildOverridesDict();

            _finalConfigs = new Dictionary<Locale, LocalizedFontDataToApply>();
            foreach (LocalizedFontLocaleConfig font in _font.Fonts)
            {
                if (font.Locale == null) continue;
                overrides.TryGetValue(font.Locale, out LocalizedFontLocaleOverrides localeOverrides);
                _finalConfigs[font.Locale] = new LocalizedFontDataToApply
                {
                    Font = localeOverrides?.Font == null ? font.Font : localeOverrides.Font,
                    FontSize = localeOverrides?.FontSize ?? (_baseConfig.FontSize * font.FontResizeBy),
                    Bold = localeOverrides?.Bold ?? font.Bold ?? _baseConfig.Bold,
                    CharacterSpacing = localeOverrides?.CharacterSpacing ?? font.CharacterSpacing ?? _baseConfig.CharacterSpacing,
                    WordSpacing = localeOverrides?.WordSpacing ?? font.WordSpacing ?? _baseConfig.WordSpacing,
                    LineSpacing = localeOverrides?.LineSpacing ?? font.LineSpacing ?? _baseConfig.LineSpacing
                };
            }

            return _finalConfigs;
        }

        private Dictionary<Locale, LocalizedFontLocaleOverrides> BuildOverridesDict()
        {
            Dictionary<Locale, LocalizedFontLocaleOverrides> overrides = new();
            if (_overridesSO != null)
            {
                foreach (LocalizedFontLocaleOverrides ovr in _overridesSO.Overrides)
                {
                    if (ovr.Locale == null) continue;
                    overrides[ovr.Locale] = ovr;
                }
            }

            foreach (LocalizedFontLocaleOverrides ovr in _overrides)
            {
                if (ovr.Locale == null) continue;
                if (overrides.ContainsKey(ovr.Locale))
                {
                    overrides[ovr.Locale] = overrides[ovr.Locale].Override(ovr);
                }
                else
                {
                    overrides[ovr.Locale] = ovr;
                }
            }

            return overrides;
        }
    }
}
