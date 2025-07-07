#if SENSEN_UI_HEAT
using MyBox;
using UnityEngine;
using Michsky.UI.Heat;

namespace SensenToolkit
{
    public class SfxSwitchHeatBinding : MonoBehaviour
    {
        [SerializeField, AutoProperty] private SwitchManager _switch;
        [SerializeField, MustBeAssigned] private AudioProfile _sfxOn;
        [SerializeField, MustBeAssigned] private AudioProfile _sfxOff;
        [Tooltip("Sfx to play when the switch is disabled. If null, no sound will be played when the switch is disabled.")]
        [SerializeField] private AudioProfile _sfxWhenDisabled;
        private SfxService _sfxService;

        private void Awake()
        {
            _sfxService = SfxService.Instance;
        }

        private void OnEnable()
        {
            _switch.onValueChanged.AddListener(OnSwitchValueChanged);
        }

        private void OnSwitchValueChanged(bool isOn)
        {
            if (!_switch.isInteractable)
            {
                if (_sfxWhenDisabled != null) _sfxService.Play(_sfxWhenDisabled);
                return;
            }

            _sfxService.Play(isOn ? _sfxOn : _sfxOff);
        }
    }
}
#endif
