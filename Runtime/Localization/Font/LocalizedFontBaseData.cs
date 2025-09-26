using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace SensenToolkit
{
    [System.Serializable]
    public class LocalizedFontBaseData
    {
        public float FontSize;

        [SerializeField] private bool _overrideBold;
        [SerializeField, FormerlySerializedAs("Bold")]
        [ConditionalField(nameof(_overrideBold))]
        private bool _bold;

        [SerializeField] private bool _overrideCharacterSpacing;
        [SerializeField, FormerlySerializedAs("CharacterSpacing")]
        [ConditionalField(nameof(_overrideCharacterSpacing))]
        private float _characterSpacing;

        [SerializeField] private bool _overrideWordSpacing;
        [SerializeField, FormerlySerializedAs("WordSpacing")]
        [ConditionalField(nameof(_overrideWordSpacing))]
        private float _wordSpacing;

        [SerializeField] private bool _overrideLineSpacing;
        [SerializeField, FormerlySerializedAs("LineSpacing")]
        [ConditionalField(nameof(_overrideLineSpacing))]
        private float _lineSpacing;

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
    }
}
