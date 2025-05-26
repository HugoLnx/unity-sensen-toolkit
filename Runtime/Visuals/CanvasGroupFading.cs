#if DOTWEEN
using DG.Tweening;
using EasyButtons;
using MyBox;
using UnityEngine;

namespace SensenToolkit
{
    [RequireComponent(typeof(CanvasGroup))]
    public class CanvasGroupFading : MonoBehaviour
    {
        private const float FADE_IN_DEFAULT_DURATION = 0.85f;
        private const float FADE_OUT_DEFAULT_DURATION = 1f;
        [SerializeField] private bool _hideOnAwake = true;
        [Tooltip("Ancestor canvas will be disabled/enabled when this screen is shown/hidden.")]
        [SerializeField] private bool _controlAncestorCanvas = true;
        [SerializeField, AutoProperty(AutoPropertyMode.Children)]
        private CanvasGroup _canvasGroup;
        [SerializeField, AutoProperty(AutoPropertyMode.Parent)]
        private Canvas _ancestorCanvas;
        private Tween _tween;

        private void Awake()
        {
            if (_hideOnAwake) _canvasGroup.alpha = 0f;
        }

        public void Show(float duration = FADE_IN_DEFAULT_DURATION)
        {
            SetAncestorCanvasEnabled(true);
            Tweenx.KillAndNullify(ref _tween);
            _tween = Tweenx.FromTo(
                action: (v) => _canvasGroup.alpha = v,
                duration: duration,
                startValue: _canvasGroup.alpha,
                endValue: 1f
            )
            .SetEase(Ease.InOutSine);
        }

        public void Hide(float duration = FADE_OUT_DEFAULT_DURATION)
        {
            Tweenx.KillAndNullify(ref _tween);
            _tween = Tweenx.FromTo(
                action: (v) => _canvasGroup.alpha = v,
                duration: duration,
                startValue: _canvasGroup.alpha,
                endValue: 0f
            )
            .SetEase(Ease.InOutSine)
            .OnKill(() => InstantHide());
        }

        [Button]
        public void IntantShow()
        {
            Tweenx.KillAndNullify(ref _tween);
            _canvasGroup.alpha = 1f;
            SetAncestorCanvasEnabled(true);
        }

        [Button]
        public void InstantHide()
        {
            Tweenx.KillAndNullify(ref _tween);
            _canvasGroup.alpha = 0f;
            SetAncestorCanvasEnabled(false);
        }

        private void SetAncestorCanvasEnabled(bool turnOn)
        {
            if (!_controlAncestorCanvas || _ancestorCanvas == null) return;
            _ancestorCanvas.enabled = turnOn;
        }
    }
}
#endif
