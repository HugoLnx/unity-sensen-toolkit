using System;
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
        [SerializeField, AutoProperty] private TMP_Text _text;
        [SerializeField, AutoProperty] private LocalizationTrigger _localizationTrigger;

        private Dictionary<Locale, LocalizedFontDataToApply> _finalConfigs;
        private Dictionary<Locale, LocalizedFontDataToApply> FinalConfigs => _finalConfigs ??= RefreshFinalConfigs();

        private Dictionary<Locale, LocalizedFontLocaleConfig> _localeConfigs;
        private Dictionary<Locale, LocalizedFontLocaleConfig> LocaleConfigs => _localeConfigs ??= RefreshLocaleConfigs();

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
            RefreshLocaleConfigs();
            Dictionary<Locale, LocalizedFontLocaleOverrides> allLocaleOverrides = BuildLocaleOverridesDict();
            Dictionary<TMP_FontAsset, LocalizedFontLocaleOverrides> allFontOverrides = BuildFontOverridesDict();

            _finalConfigs = new Dictionary<Locale, LocalizedFontDataToApply>();
            foreach (LocalizedFontLocaleConfig font in _font.Fonts)
            {
                if (font.Locale == null) continue;
                allLocaleOverrides.TryGetValue(font.Locale, out LocalizedFontLocaleOverrides localeOverrides);
                allFontOverrides.TryGetValue(font.Font, out LocalizedFontLocaleOverrides fontOverrides);
                _finalConfigs[font.Locale] = new LocalizedFontDataToApply
                {
                    Font = GetFirstNonNull(
                        localeOverrides?.Font,
                        fontOverrides?.Font,
                        font.Font
                    ),
                    FontSize = GetFirstNonNull(
                        localeOverrides?.FontSize,
                        fontOverrides?.FontSize,
                        _baseConfig.FontSize * font.FontResizeBy
                    ),
                    Bold = GetFirstNonNull(
                        localeOverrides?.Bold,
                        fontOverrides?.Bold,
                        _baseConfig.Bold
                    ),
                    CharacterSpacing = GetFirstNonNull(
                        localeOverrides?.CharacterSpacing,
                        fontOverrides?.CharacterSpacing,
                        _baseConfig.CharacterSpacing
                    ),
                    WordSpacing = GetFirstNonNull(
                        localeOverrides?.WordSpacing,
                        fontOverrides?.WordSpacing,
                        _baseConfig.WordSpacing
                    ),
                    LineSpacing = GetFirstNonNull(
                        localeOverrides?.LineSpacing,
                        fontOverrides?.LineSpacing,
                        _baseConfig.LineSpacing
                    ),
                };
            }

            return _finalConfigs;
        }

        private Dictionary<Locale, LocalizedFontLocaleConfig> RefreshLocaleConfigs()
        {
            _localeConfigs = new Dictionary<Locale, LocalizedFontLocaleConfig>();
            foreach (LocalizedFontLocaleConfig font in _font.Fonts)
            {
                if (font.Locale == null) continue;
                _localeConfigs[font.Locale] = font;
            }
            return _localeConfigs;
        }

        private Dictionary<Locale, LocalizedFontLocaleOverrides> BuildLocaleOverridesDict()
        {
            Dictionary<Locale, LocalizedFontLocaleOverrides> overrides = new();
            IEnumerable<LocalizedFontLocaleOverrides> allOverrides = FlatEach(
                _overrides,
                _overridesSO == null ? null : _overridesSO.Overrides
            );
            foreach (LocalizedFontLocaleOverrides ovr in allOverrides)
            {
                foreach (Locale locale in ovr.FilterLocales)
                {
                    if (locale == null) continue;
                    if (overrides.ContainsKey(locale))
                    {
                        overrides[locale] = overrides[locale].Override(ovr);
                    }
                    else
                    {
                        overrides[locale] = ovr;
                    }
                }
            }

            return overrides;
        }

        private Dictionary<TMP_FontAsset, LocalizedFontLocaleOverrides> BuildFontOverridesDict()
        {
            Dictionary<TMP_FontAsset, LocalizedFontLocaleOverrides> overrides = new();
            IEnumerable<LocalizedFontLocaleOverrides> allOverrides = FlatEach(
                _overrides,
                _overridesSO == null ? null : _overridesSO.Overrides
            );
            foreach (LocalizedFontLocaleOverrides ovr in allOverrides)
            {
                foreach (TMP_FontAsset font in ovr.FilterFonts)
                {
                    if (font == null) continue;
                    if (overrides.ContainsKey(font))
                    {
                        overrides[font] = overrides[font].Override(ovr);
                    }
                    else
                    {
                        overrides[font] = ovr;
                    }
                }
            }
            return overrides;
        }

        private IEnumerable<T> FlatEach<T>(params IEnumerable<T>[] collections)
        {
            foreach (IEnumerable<T> collection in collections)
            {
                if (collection == null) continue;
                foreach (T item in collection)
                {
                    yield return item;
                }
            }
        }

        private T GetFirstNonNull<T>(params T[] items) where T : class
        {
            foreach (T item in items)
            {
                if (item != null) return item;
            }
            return null;
        }

        private T GetFirstNonNull<T>(params T?[] items) where T : struct
        {
            foreach (T? item in items)
            {
                if (item.HasValue) return item.Value;
            }
            throw new Exception("All items are null");
        }
    }
}
