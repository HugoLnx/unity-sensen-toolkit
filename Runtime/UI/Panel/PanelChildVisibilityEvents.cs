using System;
using MyBox;
using UnityEngine;

namespace SensenToolkit
{
    public class PanelChildVisibilityEvents : MonoBehaviour
    {
        [SerializeField, AutoProperty(AutoPropertyMode.Parent, allowEmpty: true)]
        private PanelFadable _parentPanel;

        public event Action<bool> OnVisibilityChanged = delegate { };
        public event Action OnShow = delegate { };
        public event Action OnHidden = delegate { };
        public bool IsVisible => _parentPanel == null || _parentPanel.IsVisible;

        private void Awake()
        {
            if (_parentPanel != null)
            {
                _parentPanel.OnPrepareToShow += ParentPanel_OnPrepareToShow;
                _parentPanel.OnHidden += ParentPanel_OnHidden;
            }
        }

        private void OnDestroy()
        {
            if (_parentPanel != null)
            {
                _parentPanel.OnPrepareToShow -= ParentPanel_OnPrepareToShow;
                _parentPanel.OnHidden -= ParentPanel_OnHidden;
            }
        }

        private void OnEnable()
        {
            TriggerVisibilityIfVisible();
            var bootBlackout = AppBootBlackoutService.GetInstanceIfExists();
            if (bootBlackout != null)
            {
                bootBlackout.TryAddListenerBlackoutOver(TriggerVisibilityIfVisible);
            }
        }

        private void OnDisable()
        {
            TriggerVisibilityChange(visible: false);
        }

        private void ParentPanel_OnPrepareToShow(PanelFadable fadable)
        {
            TriggerVisibilityChange(visible: true);
        }

        private void ParentPanel_OnHidden(PanelFadable fadable)
        {
            TriggerVisibilityChange(visible: false);
        }

        private void TriggerVisibilityChange(bool visible)
        {
            OnVisibilityChanged.Invoke(visible);
            if (visible) OnShow.Invoke();
            else OnHidden.Invoke();
        }

        private void TriggerVisibilityIfVisible()
        {
            if (_parentPanel == null || _parentPanel.IsVisible) TriggerVisibilityChange(visible: true);
        }
    }
}
