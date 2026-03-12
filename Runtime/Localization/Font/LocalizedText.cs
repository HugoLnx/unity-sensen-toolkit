using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace SensenToolkit
{
    [RequireComponent(typeof(TMP_Text))]
    [RequireComponent(typeof(LocalizationTrigger))]
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField, MustBeAssigned] private LocalizedString _string;
        [Tooltip("Custom format for the localized string.eg: \"{0} coins\"")]
        [SerializeField] private string _customFormat;
        [SerializeField, AutoProperty] private TMP_Text _text;
        [SerializeField, AutoProperty] private LocalizationTrigger _localizationTrigger;

        public LocalizedString LocalizedString
        {
            get => _string;
            set => _string = value;
        }

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
            if (LocStringx.IsEmptyOrNull(_string)) return;
            string localized = _string.GetLocalizedString();
            _text.text = string.IsNullOrEmpty(_customFormat)
                ? localized
                : string.Format(_customFormat, localized);
        }
    }
}
