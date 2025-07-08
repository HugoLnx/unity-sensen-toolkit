using MyBox;
using UnityEngine;

namespace SensenToolkit
{
    public class SfxPauseBinding : MonoBehaviour
    {
        [SerializeField, MustBeAssigned] private AudioProfile _sfxPause;
        [SerializeField, MustBeAssigned] private AudioProfile _sfxUnpause;
        [SerializeField, AutoProperty] private PauseService _pauseService;
        private SfxService _sfxService;

        private void Awake()
        {
            _sfxService = SfxService.Instance;
        }

        private void OnEnable()
        {
            _pauseService.OnChanged += OnPauseStateChanged;
        }

        private void OnDisable()
        {
            _pauseService.OnChanged -= OnPauseStateChanged;
        }

        private void OnPauseStateChanged(bool isPaused)
        {
            if (isPaused) _sfxService.Play(_sfxPause);
            else _sfxService.Play(_sfxUnpause);
        }
    }
}
