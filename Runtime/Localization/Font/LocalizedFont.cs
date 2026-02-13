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

        private Dictionary<string, LocalizedFontDataToApply> _finalConfigs;
        private Dictionary<string, LocalizedFontDataToApply> FinalConfigs
            => _finalConfigs != null && _finalConfigs.Count > 0 ? _finalConfigs : RefreshFinalConfigs();


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
            if (_text.enableAutoSizing)
            {
                _baseConfig.FontAutoSize = new FontAutoSizeConfig(true, _text.fontSizeMin, _text.fontSizeMax);
            }
            else
            {
                _baseConfig.FontAutoSize = new FontAutoSizeConfig(false, 5f, 30f);
            }
            if (_text.fontStyle.HasFlag(FontStyles.Bold))
            {
                _baseConfig.Bold = true;
            }
            if (!Mathf.Approximately(_text.characterSpacing, 0f))
            {
                _baseConfig.CharacterSpacing = _text.characterSpacing;
            }
            if (!Mathf.Approximately(_text.wordSpacing, 0f))
            {
                _baseConfig.WordSpacing = _text.wordSpacing;
            }
            if (!Mathf.Approximately(_text.lineSpacing, 0f))
            {
                _baseConfig.LineSpacing = _text.lineSpacing;
            }
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
            if (!FinalConfigs.TryGetValue(AsKey(locale), out LocalizedFontDataToApply config))
            {
                Debug.LogWarning($"[AttachLocalizedFont:{name}] No config for locale {locale}");
                return;
            }
            _text.font = config.Font;
            _text.fontSize = config.FontSize;
            _text.enableAutoSizing = config.FontAutoSize.Enabled;
            _text.fontSizeMin = config.FontAutoSize.MinSize;
            _text.fontSizeMax = config.FontAutoSize.MaxSize;
            if (config.Bold.Value) _text.fontStyle |= FontStyles.Bold;
            else _text.fontStyle &= ~FontStyles.Bold;
            _text.characterSpacing = config.CharacterSpacing.Value;
            _text.wordSpacing = config.WordSpacing.Value;
            _text.lineSpacing = config.LineSpacing.Value;
            _text.margin = new Vector4(
                _text.margin.x,
                config.MarginTop ?? _text.margin.y,
                _text.margin.z,
                config.MarginBottom ?? _text.margin.w
            );
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
                if (LocStringx.IsPresent(_testLocalizedString))
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

                if (LocStringx.IsPresent(localizedString))
                {
                    localizedString.LocaleOverride = locale;
                    str = localizedString.GetLocalizedString();
                    localizedString.LocaleOverride = null;
                }
            }

            if (string.IsNullOrEmpty(str)) return;
            _text.text = str;
        }

        private Dictionary<string, LocalizedFontDataToApply> RefreshFinalConfigs()
        {
            Dictionary<string, LocalizedFontLocaleOverrides> allLocaleOverrides = BuildLocaleOverridesDict();
            Dictionary<TMP_FontAsset, LocalizedFontLocaleOverrides> allFontOverrides = BuildFontOverridesDict();

            _finalConfigs = new Dictionary<string, LocalizedFontDataToApply>();
            foreach (LocalizedFontLocaleConfig font in _font.Fonts)
            {
                foreach (Locale locale in font.Locales)
                {
                    if (locale == null) continue;
                    string localeKey = AsKey(locale);
                    if (_finalConfigs.ContainsKey(localeKey))
                    {
                        Debug.LogWarning($"[AttachLocalizedFont:{name}] Locale {locale} is included multiple times.");
                        continue;
                    }
                    allLocaleOverrides.TryGetValue(localeKey, out LocalizedFontLocaleOverrides localeOverrides);
                    allFontOverrides.TryGetValue(font.Font, out LocalizedFontLocaleOverrides fontOverrides);
                    _finalConfigs[localeKey] = new LocalizedFontDataToApply
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
                        FontAutoSize = GetFirstNonNull(
                            localeOverrides?.FontAutoSize,
                            fontOverrides?.FontAutoSize,
                            new FontAutoSizeConfig(
                                _baseConfig.FontAutoSize.Enabled,
                                _baseConfig.FontAutoSize.MinSize * font.FontResizeBy,
                                _baseConfig.FontAutoSize.MaxSize * font.FontResizeBy
                            )
                        ),
                        Bold = GetFirstNonNull(
                            localeOverrides?.Bold,
                            fontOverrides?.Bold,
                            _baseConfig.Bold,
                            font.Bold,
                            false
                        ),
                        CharacterSpacing = GetFirstNonNull(
                            localeOverrides?.CharacterSpacing,
                            fontOverrides?.CharacterSpacing,
                            _baseConfig.CharacterSpacing,
                            font.CharacterSpacing,
                            0
                        ),
                        WordSpacing = GetFirstNonNull(
                            localeOverrides?.WordSpacing,
                            fontOverrides?.WordSpacing,
                            _baseConfig.WordSpacing,
                            font.WordSpacing,
                            0
                        ),
                        LineSpacing = GetFirstNonNull(
                            localeOverrides?.LineSpacing,
                            fontOverrides?.LineSpacing,
                            _baseConfig.LineSpacing,
                            font.LineSpacing,
                            0
                        ),
                        MarginTop = GetFirstNonNull(
                            localeOverrides?.MarginTop,
                            fontOverrides?.MarginTop,
                            _baseConfig.MarginTop,
                            _text.margin.y
                        ),
                        MarginBottom = GetFirstNonNull(
                            localeOverrides?.MarginBottom,
                            fontOverrides?.MarginBottom,
                            _baseConfig.MarginBottom,
                            _text.margin.w
                        )
                    };
                }
            }

            if (_finalConfigs.Count == 0)
            {
                Debug.LogWarning($"[AttachLocalizedFont:{name}] No fonts in the font set {_font}");
            }

            return _finalConfigs;
        }

        private Dictionary<string, LocalizedFontLocaleOverrides> BuildLocaleOverridesDict()
        {
            Dictionary<string, LocalizedFontLocaleOverrides> overrides = new();
            IEnumerable<LocalizedFontLocaleOverrides> allOverrides = FlatEach(
                _overrides,
                _overridesSO == null ? null : _overridesSO.Overrides
            );
            foreach (LocalizedFontLocaleOverrides ovr in allOverrides)
            {
                foreach (Locale locale in ovr.FilterLocales)
                {
                    if (locale == null) continue;
                    string localeKey = AsKey(locale);
                    if (overrides.ContainsKey(localeKey))
                    {
                        overrides[localeKey] = overrides[localeKey].Override(ovr);
                    }
                    else
                    {
                        overrides[localeKey] = ovr;
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

        private string AsKey(Locale locale) => locale == null ? "<null>" : locale.Identifier.Code;
    }
}
