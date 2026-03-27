using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace SensenToolkit
{
    [RequireComponent(typeof(ScrollRect))]
    [RequireComponent(typeof(PanelChildVisibilityEvents))]
    public class AutoScrollToTop : MonoBehaviour
    {
        [SerializeField, AutoProperty] private ScrollRect _scrollRect;
        [SerializeField, AutoProperty] private PanelChildVisibilityEvents _visibilityEvents;

        private void Awake()
        {
            _visibilityEvents.OnShow += OnShow;
        }

        private void OnEnable()
        {
            ScrollToTop();
        }

        private void OnShow() => ScrollToTop();

        private void ScrollToTop()
        {
            _scrollRect.verticalNormalizedPosition = 1f;
        }
    }
}
