#if DOTWEEN
using System;
using DG.Tweening;
using MyBox;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SensenToolkit
{
    public class SettingsRowUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Config")]
        [SerializeField] private float _highlightTransitionDuration = 0.2f;
        [SerializeField] private float _maxColorAlpha = 0.02f;

        [Header("References")]
        [SerializeField, AutoProperty] private Image _background;

        private RectTransform Rect => transform as RectTransform;
        private float _highlightIntensity;
        private Tween _tween;

        private void Awake()
        {
            SetBackgroundHighlight(0f);
        }

        public void OnPointerEnter(PointerEventData data)
        {
            SmoothEnforceHighlightIntensityTo(1f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SmoothEnforceHighlightIntensityTo(0f);
        }


        private void SmoothEnforceHighlightIntensityTo(float intensity)
        {
            Tweenx.KillAndNullify(ref _tween);
            float changeLength = Mathf.Abs(_highlightIntensity - intensity);
            if (Mathf.Approximately(changeLength, 0f))
            {
                SetBackgroundHighlight(intensity);
                return;
            }
            float duration = _highlightTransitionDuration / changeLength;
            _tween = DOTween.To(
                getter: () => _highlightIntensity,
                setter: SetBackgroundHighlight,
                endValue: intensity,
                duration: duration
            ).SetEase(Ease.OutCubic).SetTarget(Rect);
        }

        private void SetBackgroundHighlight(float intensity)
        {
            _highlightIntensity = intensity;
            RefreshHighlight();
        }

        private void RefreshHighlight()
        {
            _background.color = _background.color.WithAlpha(_highlightIntensity * _maxColorAlpha);
        }
    }
}
#endif
