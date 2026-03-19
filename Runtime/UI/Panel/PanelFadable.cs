#if DOTWEEN
using System;
using System.Diagnostics;
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

        private const bool DEBUG_ALL = false;
        [SerializeField] private bool _debug = false;
        [field: SerializeField] public bool EnableBackSfx { get; private set; } = true;
        [SerializeField] private bool _autoPushToStack = false;
        [SerializeField] private bool _hideOnAwake = true;
        [Tooltip("If all dominant panels are hidden, the root canvas can be disabled with CanvasAutoDisable component.")]
        [SerializeField] private bool _forceDominant = false;
        [SerializeField] private PanelFadableSpeed _speedType = PanelFadableSpeed.Normal;
        [SerializeField, ConditionalField(nameof(_speedType), compareValues: PanelFadableSpeed.Custom)]
        private float _showDurationCustom = NORMAL_SHOW_DURATION;
        [SerializeField, ConditionalField(nameof(_speedType), compareValues: PanelFadableSpeed.Custom)]
        private float _hideDurationCustom = NORMAL_HIDE_DURATION;

        [Tooltip("CanvasGroup will not be interactable when this is hidden.")]
        [SerializeField] private bool _controlGroupInteractivity = true;
        [SerializeField, AutoProperty] private CanvasGroup _canvasGroup;
        [SerializeField, AutoProperty(AutoPropertyMode.Parent, allowEmpty: true, predicateMethodName: nameof(IsNotMyself))]
        private PanelFadable _parentPanel;

        [SerializeField, AutoProperty(AutoPropertyMode.Scene)]
        private PanelsService _panelsService;

        private Logx _logger;
        private Logx Logger => _logger ??= Logx.GetLogger(nameof(PanelFadable), true);

        private CanvasGroup _dominantCanvasGroup;
        public CanvasGroup DominantCanvasGroup => _dominantCanvasGroup == null
            ? _dominantCanvasGroup = GetDominantCanvasGroup()
            : _dominantCanvasGroup;

        public bool IsDominant => _canvasGroup == DominantCanvasGroup;

        private Tween _tween;
        public bool IsVisible { get; private set; }
        public bool IsFullyVisible => IsVisible && _canvasGroup.alpha >= 1f;
        public bool IsFullyInvisible => !IsVisible || _canvasGroup.alpha <= 0f;
        public CanvasGroup CanvasGroup => _canvasGroup;

        public event System.Action<PanelFadable> OnPrepareToShow = delegate { };
        public event System.Action<PanelFadable> OnPrepareToHide = delegate { };
        public event System.Action<PanelFadable> OnShown = delegate { };
        public event System.Action<PanelFadable> OnHidden = delegate { };

        private void Awake()
        {
            _panelsService = PanelsService.GetInstanceIfExists();
            if (_hideOnAwake) SetVisibilityTo(false);
            if (_parentPanel != null)
            {
                if (_parentPanel == this)
                {
                    throw new InvalidOperationException("PanelFadable cannot be its own parent.");
                }
                _parentPanel.OnPrepareToShow += ParentPanel_OnPrepareToShow;
                _parentPanel.OnShown += ParentPanel_OnShown;
                _parentPanel.OnPrepareToHide += ParentPanel_OnPrepareToHide;
                _parentPanel.OnHidden += ParentPanel_OnHidden;
            }
        }

        private void OnDisable()
        {
            Tweenx.KillAndNullify(ref _tween);
            if (_parentPanel != null)
            {
                _parentPanel.OnPrepareToShow -= ParentPanel_OnPrepareToShow;
                _parentPanel.OnShown -= ParentPanel_OnShown;
                _parentPanel.OnPrepareToHide -= ParentPanel_OnPrepareToHide;
                _parentPanel.OnHidden -= ParentPanel_OnHidden;
            }
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
            LogInfo($"{nameof(PrepareToShow)} called.");
            Tweenx.KillAndNullify(ref _tween);
            SetVisibilityTo(true);
            SwitchInteractivityTo(false);
            if (_autoPushToStack && _panelsService != null)
            {
                _panelsService.PushTop(this);
            }
            if (_parentPanel == null || _parentPanel.IsVisible)
            {
                LogInfo($"Invoke:{nameof(OnPrepareToShow)}");
                OnPrepareToShow.Invoke(this);
            }
        }

        private void FinishShow()
        {
            LogInfo($"{nameof(FinishShow)} called.");
            SwitchInteractivityTo(true);
            _canvasGroup.alpha = 1f;
            if (_parentPanel == null || _parentPanel.IsVisible)
            {
                LogInfo($"Invoke:{nameof(OnShown)}");
                OnShown.Invoke(this);
            }
        }

        private void PrepareToHide()
        {
            LogInfo($"{nameof(PrepareToHide)} called.");
            Tweenx.KillAndNullify(ref _tween);
            SwitchInteractivityTo(false);
            if (_parentPanel == null || _parentPanel.IsVisible)
            {
                LogInfo($"Invoke:{nameof(OnPrepareToHide)}");
                OnPrepareToHide.Invoke(this);
            }
        }

        private void FinishHide()
        {
            LogInfo($"{nameof(FinishHide)} called.");
            SetVisibilityTo(false);
            if (_parentPanel == null || _parentPanel.IsVisible)
            {
                LogInfo($"Invoke:{nameof(OnHidden)}");
                OnHidden.Invoke(this);
            }
        }

        private void SwitchInteractivityTo(bool turnOn)
        {
            _canvasGroup.interactable = turnOn;
            _canvasGroup.blocksRaycasts = turnOn;
        }

        private CanvasGroup GetDominantCanvasGroup()
        {
            if (_forceDominant || _canvasGroup.ignoreParentGroups) return _canvasGroup;
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

        private void ParentPanel_OnPrepareToShow(PanelFadable fadable)
        {
            if (IsFullyVisible)
            {
                LogInfo($"Invoke:{nameof(OnPrepareToShow)} (through parent)");
                OnPrepareToShow.Invoke(this);
            }
        }

        private void ParentPanel_OnShown(PanelFadable fadable)
        {
            if (IsFullyVisible)
            {
                LogInfo($"Invoke:{nameof(OnShown)} (through parent)");
                OnShown.Invoke(this);
            }
        }

        private void ParentPanel_OnPrepareToHide(PanelFadable fadable)
        {
            if (IsFullyVisible)
            {
                LogInfo($"Invoke:{nameof(OnPrepareToHide)} (through parent)");
                OnPrepareToHide.Invoke(this);
            }
        }

        private void ParentPanel_OnHidden(PanelFadable fadable)
        {
            if (IsFullyVisible)
            {
                LogInfo($"Invoke:{nameof(OnHidden)} (through parent)");
                OnHidden.Invoke(this);
            }
        }

        private bool IsNotMyself(UnityEngine.Object obj) => obj != null && this != obj as PanelFadable;

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private void LogInfo(string msg)
        {
            bool isDebugging = DEBUG_ALL || _debug;
            if (!isDebugging) return;
            Logger.Info(msg, category: this.NameWithParent());
        }
    }
}
#endif
