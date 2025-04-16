#if DOTWEEN
using DG.Tweening;
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
        [SerializeField, AutoProperty(AutoPropertyMode.Children)]
        private CanvasGroup _canvasGroup;
        private Tween _tween;

        private void Awake()
        {
            if (_hideOnAwake) _canvasGroup.alpha = 0f;
        }

        public void Appear(float duration = FADE_IN_DEFAULT_DURATION)
        {
            Tweenx.KillAndNullify(ref _tween);
            _tween = Tweenx.FromTo(
                action: (v) => _canvasGroup.alpha = v,
                duration: duration,
                startValue: _canvasGroup.alpha,
                endValue: 1f
            )
            .SetEase(Ease.InOutSine);
        }

        public void Disappear(float duration = FADE_OUT_DEFAULT_DURATION)
        {
            Tweenx.KillAndNullify(ref _tween);
            _tween = Tweenx.FromTo(
                action: (v) => _canvasGroup.alpha = v,
                duration: duration,
                startValue: _canvasGroup.alpha,
                endValue: 0f
            )
            .SetEase(Ease.InOutSine);
        }
    }
}
#endif
