using System;
using System.Collections;
using EasyButtons;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace SensenToolkit
{
    [RequireComponent(typeof(PanelChildVisibilityEvents))]
    public class MaxWidthContainerUI : MonoBehaviour
    {
        [SerializeField, AutoProperty(AutoPropertyMode.Parent)] private CanvasScaler _canvasScaler;
        [SerializeField, AutoProperty] private PanelChildVisibilityEvents _visibilityEvents;
        [SerializeField, AutoProperty] private RectTransform _container;
        [SerializeField] private float _maxWidth = 1920f;

        private void Awake()
        {
            _visibilityEvents.OnShow += OnShow;
            var screenService = ScreenService.GetInstanceIfExists();
            if (screenService != null)
            {
                screenService.OnResolutionChanged += OnResolutionChanged;
            }
        }

        private void OnDestroy()
        {
            if (_visibilityEvents != null)
            {
                _visibilityEvents.OnShow -= OnShow;
            }
            var screenService = ScreenService.GetInstanceIfExists();
            if (screenService != null)
            {
                screenService.OnResolutionChanged -= OnResolutionChanged;
            }
        }

        private void OnShow() => StartCoroutine(DelayedRefreshMaxWidth());
        private void OnResolutionChanged(Resolution _)
        {
            if (!_visibilityEvents.IsVisible) return;
            StartCoroutine(DelayedRefreshMaxWidth());
        }

        private IEnumerator DelayedRefreshMaxWidth()
        {
            RefreshMaxWidth();
            yield return null;
            RefreshMaxWidth();
        }

        [Button]
        private void RefreshMaxWidth()
        {
            float parentRawWidth = ((RectTransform)transform.parent).rect.width;
            float adaptedMaxWidth = _maxWidth / Screen.currentResolution.width * _canvasScaler.referenceResolution.x;
            float targetWidth = Mathf.Min(parentRawWidth, adaptedMaxWidth);
            _container.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
        }
    }
}
