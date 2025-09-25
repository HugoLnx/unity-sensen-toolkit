using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace SensenToolkit
{
    [System.Serializable]
    public class LocalizedFontLocaleConfig
    {
        public Locale Locale;
        public TMP_FontAsset Font;

        public float FontResizeBy = 1f;

        [SerializeField] private bool _overrideBold;
        [SerializeField, ConditionalField(nameof(_overrideBold))] private bool _bold;

        [SerializeField] private bool _overrideCharacterSpacing;
        [SerializeField, ConditionalField(nameof(_overrideCharacterSpacing))]
        private float _characterSpacing;

        [SerializeField] private bool _overrideWordSpacing;
        [SerializeField, ConditionalField(nameof(_overrideWordSpacing))]
        private float _wordSpacing = 0f;

        [SerializeField] private bool _overrideLineSpacing;
        [SerializeField, ConditionalField(nameof(_overrideLineSpacing))]
        private float _lineSpacing = 0f;

        public bool? Bold => _overrideBold ? _bold : null;
        public float? CharacterSpacing => _overrideCharacterSpacing ? _characterSpacing : null;
        public float? LineSpacing => _overrideLineSpacing ? _lineSpacing : null;
        public float? WordSpacing => _overrideWordSpacing ? _wordSpacing : null;
    }
}
