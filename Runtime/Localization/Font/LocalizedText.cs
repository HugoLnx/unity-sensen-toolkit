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
            _text.text = _string.GetLocalizedString();
        }
    }
}
