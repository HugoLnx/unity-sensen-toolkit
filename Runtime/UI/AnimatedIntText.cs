using DG.Tweening;
using MyBox;
using SensenToolkit;
using TMPro;
using UnityEngine;

namespace SensenToolkit
{
    public class AnimatedIntText : MonoBehaviour
    {
        [SerializeField] private float _animationDuration = 1f;
        [SerializeField, AutoProperty] private TMP_Text _text;
        [field: SerializeField] public string TextFormat { get; set; } = "";
        private int _lastDrawnValue;
        private Tween _tween;

        public int LastDrawnValue => _lastDrawnValue;

        private void OnDisable()
        {
            Tweenx.KillAndNullify(ref _tween);
        }

        public void Set(int value, bool animate = true)
        {
            if (animate)
            {
                AnimateScoreChangeTo(value);
            }
            else
            {
                Tweenx.KillAndNullify(ref _tween);
                UpdateText(value);
            }
        }

        private void AnimateScoreChangeTo(int value)
        {
            Tweenx.KillAndNullify(ref _tween);

            _tween = Tweenx.FromTo(
                action: (v) => UpdateText((int)v),
                startValue: _lastDrawnValue,
                endValue: value,
                duration: _animationDuration
            )
            .SetEase(Ease.OutSine)
            .OnComplete(() => UpdateText(value));
        }

        private void UpdateText(int value)
        {
            _lastDrawnValue = value;
            _text.text = string.IsNullOrWhiteSpace(TextFormat)
                ? $"{value}"
                : string.Format(TextFormat, value);
        }
    }
}
