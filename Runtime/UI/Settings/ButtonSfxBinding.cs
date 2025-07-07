using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace SensenToolkit
{
    public class ButtonSfxBinding : MonoBehaviour
    {
        [SerializeField, AutoProperty(AutoPropertyMode.Children)]
        private Button _button;
        [SerializeField, MustBeAssigned] private AudioProfile _sfx;
        [Tooltip("Sfx to play when the button is disabled. If null, no sound will be played when the button is disabled.")]
        [SerializeField] private AudioProfile _sfxWhenDisabled;
        private SfxService _sfxService;

        private void Awake()
        {
            _sfxService = SfxService.Instance;
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnButtonClicked);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            if (!_button.interactable)
            {
                if (_sfxWhenDisabled != null) _sfxService.Play(_sfxWhenDisabled);
                return;
            }
            _sfxService.Play(_sfx);
        }
    }
}
