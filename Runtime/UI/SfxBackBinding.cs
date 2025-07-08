using MyBox;
using UnityEngine;

namespace SensenToolkit
{
    public class SfxBackBinding : MonoBehaviour
    {
        [SerializeField, MustBeAssigned] private AudioProfile _sfxBack;
        [SerializeField, MustBeAssigned] private AudioProfile _sfxBackError;

        [SerializeField, AutoProperty(AutoPropertyMode.Scene)]
        private PanelsService _panelsService;
        private SfxService _sfxService;

        private void Awake()
        {
            _sfxService = SfxService.Instance;
        }

        private void OnEnable()
        {
            _panelsService.OnBack += OnBack;
        }

        private void OnDisable()
        {
            _panelsService.OnBack -= OnBack;
        }

        private void OnBack(PanelFadable previousTopPanel, PanelFadable topPanel)
        {
            if (previousTopPanel != null && !previousTopPanel.EnableBackSfx) return;
            AudioProfile sfx = previousTopPanel == null ? _sfxBackError : _sfxBack;
            _sfxService.Play(sfx);
        }
    }
}
