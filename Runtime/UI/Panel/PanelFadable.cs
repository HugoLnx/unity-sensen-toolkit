#if DOTWEEN
using System;
using DG.Tweening;
using EasyButtons;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace SensenToolkit
{
    [RequireComponent(typeof(CanvasGroup))]
    public class PanelFadable : MonoBehaviour
    {
        // For something more dramatic?
        private const float SLOW_SHOW_DURATION = 0.85f;
        private const float SLOW_HIDE_DURATION = 0.65f;

        // For screens (like pause menu)
        private const float NORMAL_SHOW_DURATION = 0.5f;
        private const float NORMAL_HIDE_DURATION = 0.35f;

        // For internal panels (like settings panels)
        private const float FAST_SHOW_DURATION = 0.25f;
        private const float FAST_HIDE_DURATION = 0.15f;

        [field: SerializeField] public bool EnableBackSfx { get; private set; } = true;
        [SerializeField] private bool _autoPushToStack = false;
        [SerializeField] private bool _hideOnAwake = true;
        [SerializeField] private PanelFadableSpeed _speedType = PanelFadableSpeed.Normal;
        [SerializeField, ConditionalField(nameof(_speedType), compareValues: PanelFadableSpeed.Custom)]
        private float _showDurationCustom = NORMAL_SHOW_DURATION;
        [SerializeField, ConditionalField(nameof(_speedType), compareValues: PanelFadableSpeed.Custom)]
        private float _hideDurationCustom = NORMAL_HIDE_DURATION;

        [Tooltip("CanvasGroup will not be interactable when this is hidden.")]
        [SerializeField] private bool _controlGroupInteractivity = true;
        [SerializeField, AutoProperty] private CanvasGroup _canvasGroup;
        [SerializeField, AutoProperty(AutoPropertyMode.Scene)]
        private PanelsService _panelsService;

        private CanvasGroup _dominantCanvasGroup;
        public CanvasGroup DominantCanvasGroup => _dominantCanvasGroup == null
            ? _dominantCanvasGroup = GetDominantCanvasGroup()
            : _dominantCanvasGroup;

        public bool IsDominant => _canvasGroup == DominantCanvasGroup;

        private Tween _tween;
        public bool IsVisible { get; private set; }
        public bool IsFullyVisible => IsVisible && _canvasGroup.alpha >= 1f;
        public CanvasGroup CanvasGroup => _canvasGroup;

        public event System.Action<PanelFadable> OnPrepareToShow = delegate { };
        public event System.Action<PanelFadable> OnPrepareToHide = delegate { };
        public event System.Action<PanelFadable> OnShown = delegate { };
        public event System.Action<PanelFadable> OnHidden = delegate { };

        private void Awake()
        {
            if (_hideOnAwake) SetVisibilityTo(false);
        }

        private void OnDisable()
        {
            Tweenx.KillAndNullify(ref _tween);
            SetVisibilityTo(false);
        }

        public Tween Show(float? duration = null)
        {
            if (IsVisible) return _tween;
            PrepareToShow();

            duration ??= ChooseShowDuration();
            float diffToShow = 1f - _canvasGroup.alpha;
            duration *= diffToShow;
            _tween = Tweenx.FromTo(
                action: (v) => _canvasGroup.alpha = v,
                duration: duration.Value,
                startValue: _canvasGroup.alpha,
                endValue: 1f
            )
            .SetUpdate(isIndependentUpdate: true)
            .SetEase(Ease.InOutSine)
            .OnComplete(() => FinishShow());

            return _tween;
        }

        public Tween Hide(float? duration = null)
        {
            if (!IsVisible) return _tween;
            PrepareToHide();

            duration ??= ChooseHideDuration();
            float diffToHide = _canvasGroup.alpha;
            duration *= diffToHide;
            _tween = Tweenx.FromTo(
                action: (v) => _canvasGroup.alpha = v,
                duration: duration.Value,
                startValue: _canvasGroup.alpha,
                endValue: 0f
            )
            .SetUpdate(isIndependentUpdate: true)
            .SetEase(Ease.InOutSine)
            .OnComplete(() => FinishHide());

            return _tween;
        }

        [Button]
        public void IntantShow()
        {
            PrepareToShow();
            FinishShow();
        }

        [Button]
        public void InstantHide()
        {
            PrepareToHide();
            FinishHide();
        }

        private void SetVisibilityTo(bool turnOn)
        {
            IsVisible = turnOn;
            if (!turnOn)
            {
                _canvasGroup.alpha = 0f;
            }
            if (_controlGroupInteractivity && _canvasGroup != null)
            {
                SwitchInteractivityTo(turnOn);
            }
        }

        private void PrepareToShow()
        {
            Tweenx.KillAndNullify(ref _tween);
            SetVisibilityTo(true);
            SwitchInteractivityTo(false);
            OnPrepareToShow.Invoke(this);
        }

        private void FinishShow()
        {
            SwitchInteractivityTo(true);
            _canvasGroup.alpha = 1f;
            if (_autoPushToStack && _panelsService != null)
            {
                _panelsService.PushTop(this);
            }
            OnShown.Invoke(this);
        }

        private void PrepareToHide()
        {
            Tweenx.KillAndNullify(ref _tween);
            SwitchInteractivityTo(false);
            OnPrepareToHide.Invoke(this);
        }

        private void FinishHide()
        {
            SetVisibilityTo(false);
            OnHidden.Invoke(this);
        }

        private void SwitchInteractivityTo(bool turnOn)
        {
            _canvasGroup.interactable = turnOn;
            _canvasGroup.blocksRaycasts = turnOn;
        }

        private CanvasGroup GetDominantCanvasGroup()
        {
            if (_canvasGroup.ignoreParentGroups) return _canvasGroup;
            if (_canvasGroup.transform.parent.TryGetComponentInParent(out CanvasGroup parentCanvasGroup))
            {
                return parentCanvasGroup;
            }
            else
            {
                return _canvasGroup;
            }
        }

        private float ChooseHideDuration()
        {
            return _speedType switch
            {
                PanelFadableSpeed.Slow => SLOW_HIDE_DURATION,
                PanelFadableSpeed.Normal => NORMAL_HIDE_DURATION,
                PanelFadableSpeed.Fast => FAST_HIDE_DURATION,
                PanelFadableSpeed.Custom => _hideDurationCustom,
                _ => throw new ArgumentOutOfRangeException(nameof(_speedType), _speedType, "Invalid speed type for hiding panel.")
            };
        }

        private float ChooseShowDuration()
        {
            return _speedType switch
            {
                PanelFadableSpeed.Slow => SLOW_SHOW_DURATION,
                PanelFadableSpeed.Normal => NORMAL_SHOW_DURATION,
                PanelFadableSpeed.Fast => FAST_SHOW_DURATION,
                PanelFadableSpeed.Custom => _showDurationCustom,
                _ => throw new ArgumentOutOfRangeException(nameof(_speedType), _speedType, "Invalid speed type for showing panel.")
            };
        }
    }
}
#endif
