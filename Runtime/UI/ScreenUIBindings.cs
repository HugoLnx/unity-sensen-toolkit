#if DOTWEEN
using DG.Tweening;
using MyBox;
using SensenToolkit;
using UnityEngine;

namespace Bumashuta
{
    [RequireComponent(typeof(PanelFadable))]
    public class ScreenUIBindings : MonoBehaviour
    {

        [Tooltip("Freeze gameplay while this screen is shown.")]
        [SerializeField] private bool _freeze = true;

        [Tooltip("Pause is blocked while this screen is shown.")]
        [SerializeField] private bool _blockPause = true;

        [SerializeField, AutoProperty] private PanelFadable _panel;
        [SerializeField, AutoProperty(AutoPropertyMode.Scene, allowEmpty: true)]
        private FreezeService _freezeService;
        [SerializeField, AutoProperty(AutoPropertyMode.Scene, allowEmpty: true)]
        private PauseService _pauseService;

        private void OnEnable()
        {
            _panel.OnPrepareToShow += OnPanelPrepareToShow;
            _panel.OnHidden += OnPanelHidden;
            UnlockGameplay();
        }

        private void OnDisable()
        {
            _panel.OnPrepareToShow -= OnPanelPrepareToShow;
            _panel.OnHidden -= OnPanelHidden;
            UnlockGameplay();
        }

        private void LockGameplay()
        {
            BlockPause();
            Freeze();
        }

        private void UnlockGameplay()
        {
            Unfreeze();
            UnblockPause();
        }

        private void OnPanelHidden(PanelFadable _) => UnlockGameplay();
        private void OnPanelPrepareToShow(PanelFadable _) => LockGameplay();

        private void BlockPause()
        {
            if (!_blockPause || _pauseService == null) return;
            _pauseService.BlockPause(this);
        }

        private void UnblockPause()
        {
            if (!_blockPause || _pauseService == null) return;
            _pauseService.UnblockPause(this);
        }

        private void Freeze()
        {
            if (!_freeze || _freezeService == null) return;
            _freezeService.Freeze(this);
        }

        private void Unfreeze()
        {
            if (!_freeze || _freezeService == null) return;
            _freezeService.Unfreeze(this);
        }
    }
}
#endif
