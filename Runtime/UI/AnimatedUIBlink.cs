using System;
using DG.Tweening;
using EasyButtons;
using MyBox;
using UnityEngine;

namespace SensenToolkit
{
    public class AnimatedUIBlink : MonoBehaviour
    {
        [SerializeField] private float _blinkDuration = 1.25f;
        [SerializeField, MinMaxRange(0f, 1f)] private RangedFloat _alphaRange = new(0.05f, 1f);
        [SerializeField] private Ease _easeFadeIn = Ease.InOutSine;
        [SerializeField] private Ease _easeFadeOut = Ease.InSine;
        [SerializeField] private bool _blinkOnStart = true;

        [Header("References")]
        [SerializeField, AutoProperty] private CanvasGroup _canvasGroup;
        private Tween _tween;

        private void Start()
        {
            if (_blinkOnStart) EnsureBlink();
        }

        private void OnDisable()
        {
            Tweenx.KillAndNullify(ref _tween);
        }

        [Button(Mode = ButtonMode.EnabledInPlayMode)]
        public void EnsureBlink()
        {
            if (_tween != null && _tween.IsActive()) return;
            RestartBlink();
        }

        [Button(Mode = ButtonMode.EnabledInPlayMode)]
        public void RestartBlink()
        {
            Tweenx.KillAndNullify(ref _tween);

            _tween = DOTween.Sequence()
            .Append(Tweenx.FromTo(
                action: (a) => _canvasGroup.alpha = a,
                duration: _blinkDuration / 2f,
                startValue: _alphaRange.Min,
                endValue: _alphaRange.Max
            ).SetEase(_easeFadeIn))
            .Append(Tweenx.FromTo(
                action: (a) => _canvasGroup.alpha = a,
                duration: _blinkDuration / 2f,
                startValue: _alphaRange.Max,
                endValue: _alphaRange.Min
            ).SetEase(_easeFadeOut))
            .SetLoops(-1, LoopType.Restart)
            .Play();
        }

        [Button(Mode = ButtonMode.EnabledInPlayMode)]
        public void StopBlink()
        {
            Tweenx.KillAndNullify(ref _tween);
            _canvasGroup.alpha = 1f;
        }
    }
}
