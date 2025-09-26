using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Serialization;

namespace SensenToolkit
{
    [System.Serializable]
    public class LocalizedFontLocaleOverrides
    {
        private readonly List<Locale> _localeEmptyList = new();
        private readonly List<TMP_FontAsset> _fontEmptyList = new();
        [SerializeField]
        private List<Locale> _filterLocales = new();
        [SerializeField] private LocalizedFontOverrideFilterSO _filterSO;


        [SerializeField] private bool _overrideFont;
        [SerializeField, ConditionalField(nameof(_overrideFont))] private TMP_FontAsset _font;

        [SerializeField] private bool _overrideFontSize;
        [SerializeField, ConditionalField(nameof(_overrideFontSize))] private float _fontSize = 14f;

        [SerializeField] private bool _overrideBold;
        [SerializeField, ConditionalField(nameof(_overrideBold))] private bool _bold;

        [SerializeField] private bool _overrideCharacterSpacing;
        [SerializeField, ConditionalField(nameof(_overrideCharacterSpacing))] private float _characterSpacing;

        [SerializeField] private bool _overrideWordSpacing;
        [SerializeField, ConditionalField(nameof(_overrideWordSpacing))] private float _wordSpacing;

        [SerializeField] private bool _overrideLineSpacing;
        [SerializeField, ConditionalField(nameof(_overrideLineSpacing))] private float _lineSpacing;

        public List<Locale> FilterLocales
        {
            get
            {
                if (_filterLocales.Count > 0) return _filterLocales;
                return _filterSO == null ? _localeEmptyList : _filterSO.Locales;
            }
        }
        public List<TMP_FontAsset> FilterFonts => _filterSO == null ? _fontEmptyList : _filterSO.Fonts;

        public TMP_FontAsset Font
        {
            get => _overrideFont ? _font : null;
            set
            {
                _overrideFont = value != null;
                _font = value;
            }
        }

        public float? FontSize
        {
            get => _overrideFontSize ? _fontSize : null;
            set
            {
                _overrideFontSize = value != null;
                _fontSize = value ?? 14f;
            }
        }

        public bool? Bold
        {
            get => _overrideBold ? _bold : null;
            set
            {
                _overrideBold = value != null;
                _bold = value ?? false;
            }
        }

        public float? CharacterSpacing
        {
            get => _overrideCharacterSpacing ? _characterSpacing : null;
            set
            {
                _overrideCharacterSpacing = value != null;
                _characterSpacing = value ?? 0f;
            }
        }

        public float? WordSpacing
        {
            get => _overrideWordSpacing ? _wordSpacing : null;
            set
            {
                _overrideWordSpacing = value != null;
                _wordSpacing = value ?? 0f;
            }
        }

        public float? LineSpacing
        {
            get => _overrideLineSpacing ? _lineSpacing : null;
            set
            {
                _overrideLineSpacing = value != null;
                _lineSpacing = value ?? 0f;
            }
        }

        public LocalizedFontLocaleOverrides Clone()
        {
            return new LocalizedFontLocaleOverrides
            {
                _filterLocales = new List<Locale>(this._filterLocales),
                _filterSO = this._filterSO,
                Font = this.Font,
                FontSize = this.FontSize,
                Bold = this.Bold,
                CharacterSpacing = this.CharacterSpacing,
                WordSpacing = this.WordSpacing,
                LineSpacing = this.LineSpacing
            };
        }

        public LocalizedFontLocaleOverrides Override(LocalizedFontLocaleOverrides other)
        {
            return new LocalizedFontLocaleOverrides
            {
                _filterLocales = new List<Locale>(this._filterLocales),
                _filterSO = this._filterSO,
                Font = this.Font != null ? this.Font : other.Font,
                FontSize = this.FontSize ?? other.FontSize,
                Bold = this.Bold ?? other.Bold,
                CharacterSpacing = this.CharacterSpacing ?? other.CharacterSpacing,
                WordSpacing = this.WordSpacing ?? other.WordSpacing,
                LineSpacing = this.LineSpacing ?? other.LineSpacing
            };
        }
    }
}
