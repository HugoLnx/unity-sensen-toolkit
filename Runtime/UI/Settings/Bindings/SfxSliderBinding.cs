using MyBox;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SensenToolkit
{
    public class SfxSliderBinding : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField, AutoProperty] private Slider _slider;
        [Tooltip("How much percent the slider value must change to trigger a sound change. ")]
        [SerializeField] private float _changePercentThresholdToChange = 0.05f;
        [SerializeField, MustBeAssigned] private AudioProfile _sfxClick;
        [SerializeField, MustBeAssigned] private AudioProfile _sfxChange;
        [SerializeField, MustBeAssigned] private AudioProfile _sfxSubmit;
        [Tooltip("Sfx to play when the switch is disabled. If null, no sound will be played when the switch is disabled.")]
        [SerializeField] private AudioProfile _sfxWhenDisabled;
        public float ChangeAmountToPlay => Mathf.Abs((_slider.maxValue - _slider.minValue)) * _changePercentThresholdToChange;
        private float _missingToPlay = -1;
        private float _lastValue = float.NaN;
        private SfxService _sfxService;

        private void Awake()
        {
            _sfxService = SfxService.Instance;
            _missingToPlay = ChangeAmountToPlay;
        }

        private void OnEnable()
        {
            _slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        private void OnDisable()
        {
            _slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }

        private void OnSliderValueChanged(float val)
        {
            if (!_slider.interactable) return;

            if (float.IsNaN(_lastValue))
            {
                _lastValue = val;
                _missingToPlay = ChangeAmountToPlay;
                return;
            }

            _missingToPlay -= Mathf.Abs(val - _lastValue);
            _lastValue = val;
            if (_missingToPlay <= 0)
            {
                _sfxService.Play(_sfxChange);
                _missingToPlay += ChangeAmountToPlay;
            }
        }

        public void OnPointerDown(PointerEventData _)
        {
            if (!_slider.interactable)
            {
                if (_sfxWhenDisabled != null) _sfxService.Play(_sfxWhenDisabled);
                return;
            }

            _sfxService.Play(_sfxClick);
            _missingToPlay = ChangeAmountToPlay;
            _lastValue = _slider.value;
        }

        public void OnPointerUp(PointerEventData _)
        {
            if (!_slider.interactable) return;
            _sfxService.Play(_sfxSubmit);
        }
    }
}
