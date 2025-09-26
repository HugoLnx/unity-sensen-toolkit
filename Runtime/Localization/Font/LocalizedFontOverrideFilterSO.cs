using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace SensenToolkit
{
    [CreateAssetMenu(fileName = "OverrideFilter", menuName = "Sensen/Localization/OverrideFilter")]
    public class LocalizedFontOverrideFilterSO : ScriptableObject
    {
        private readonly List<Locale> _localeEmptyList = new();
        private readonly List<TMP_FontAsset> _fontEmptyList = new();
        [SerializeField] private List<Locale> _locales = new();
        [SerializeField] private List<TMP_FontAsset> _fonts = new();
        [SerializeField] private bool _isEnabled = true;

        public List<Locale> Locales => _isEnabled ? _locales : _localeEmptyList;
        public List<TMP_FontAsset> Fonts => _isEnabled ? _fonts : _fontEmptyList;
    }
}
