#if DOTWEEN
using MyBox;
using UnityEngine;
using UnityEngine.Serialization;

namespace SensenToolkit
{
    [RequireComponent(typeof(PanelFadable))]
    public class ScreenUIBindings : MonoBehaviour
    {

        [Tooltip("Freeze gameplay while this screen is shown.")]
        [SerializeField]
        private bool _holdFocus = true;

        [Tooltip("Pause is blocked while this screen is shown.")]
        [SerializeField] private bool _blockPause = true;

        [SerializeField, AutoProperty] private PanelFadable _panel;
        [SerializeField, AutoProperty(AutoPropertyMode.Scene, allowEmpty: true)]
        private PanelsService _panelsService;
        [SerializeField, AutoProperty(AutoPropertyMode.Scene, allowEmpty: true)]
        private PauseService _pauseService;

        private void Awake()
        {
            _panelsService = PanelsService.GetInstanceIfExists();
            _pauseService = PauseService.GetInstanceIfExists();
        }

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
            HoldFocus();
        }

        private void UnlockGameplay()
        {
            ReleaseFocus();
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

        private void HoldFocus()
        {
            if (!_holdFocus || _panelsService == null) return;
            _panelsService.AddFocusHolder(_panel);
        }

        private void ReleaseFocus()
        {
            if (!_holdFocus || _panelsService == null) return;
            _panelsService.RemoveFocusHolder(_panel);
        }
    }
}
#endif
