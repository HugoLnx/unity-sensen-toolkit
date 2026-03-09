using System;
using System.Collections.Generic;
using MyBox;
using SensenToolkit.Internal;
using UnityEngine;

namespace SensenToolkit
{
    public abstract class AValueSO<Tvalue, Tso> : AValueSOBase, IScriptableCallbackSubscriber_OnAppAwake_Internal
        where Tso : AValueSO<Tvalue, Tso>
    {
        [SerializeField, MustBeAssigned] private string _name = null;
        [Tooltip("Use it if you don't want it to be stored across the sessions. (eg. for settings)")]
        [SerializeField] private bool _resetToDefaultOnEnable = true;
        [Tooltip("Value to reset.")]
        [SerializeField] private Tvalue _defaultValue;
        [Tooltip("Forces to use constant value instead of RawValue")]
        [SerializeField] private bool _useConstant = false;
        [SerializeField, ConditionalField(nameof(_useConstant))]
        private Tvalue _constantValue;
        [SerializeField, ReadOnly] protected Tvalue RawValue;
        private (bool IsSet, Tvalue Value) _prevValue;
        private (bool IsSet, Tvalue Value) _runtimeDefaultValue = (false, default);

        public Tvalue Value { get => GetValue(); set => SetValue(value); }
        public Tvalue DefaultValue => ResolveDefaultValue();
        public override bool IsUsingDefaultValue => EqualityComparer<Tvalue>.Default.Equals(Value, DefaultValue);

        public override object ValueAsObject => Value;
        public override string Name => _name;
        private static bool IsRuntime => AppCore.IsRuntime;

        private const bool ACTIVATE_LOGGER = false;
        [NonSerialized] private Logx _logger;
        private Logx Logger => _logger ??= Logx.GetLogger(typeof(Tso).Name, ACTIVATE_LOGGER);

        public delegate void ExtraValueChangedHandler(Tso source, Tvalue newValue, Tvalue oldValue);
        public event ExtraValueChangedHandler OnValueChangedExtra = delegate { };
        public event Action<Tso> OnValueChanged = delegate { };

        protected void OnAppBoot()
        {
            Logger.Info("OnAppBoot called.");
            _prevValue = (false, default);
            _runtimeDefaultValue = (false, default);

            if (_resetToDefaultOnEnable)
            {
                RawValue = DefaultValue;
            }
            OnValueChangedExtra = delegate { }; // Unsubscribe all listeners
            OnValueChanged = delegate { }; // Unsubscribe all listeners

            AppCore.OnAppBootingEnd += OnAppBootingEnd;
#if UNITY_EDITOR
            AppCore.OnAppQuittingEnd += OnAppQuittingEnd;
#endif
        }

        private void OnAppBootingEnd()
        {
            Logger.Info("OnAppBootingEnd called.");
            TryChange();
        }

#if UNITY_EDITOR
        private void OnAppQuittingEnd()
        {
            Initialize();
        }

        protected void OnEnable()
        {
            TryInitializeInRuntime();
        }

        private void OnValidate()
        {
            TryInitializeInRuntime();
        }

        private void TryInitializeInRuntime()
        {
            if (IsRuntime) return;
            Initialize();
        }

        private void Initialize()
        {
            if (!Env.IsEditor) return;

            Logger.Info("Initializing in editor.");
            RawValue = _defaultValue;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif

        public void AddSyncListener(Action<Tso> listener)
        {
            listener(this as Tso);
            OnValueChanged += listener;
        }

        public void RemoveSyncListener(Action<Tso> listener)
        {
            OnValueChanged -= listener;
        }

        public void ChangeDefault(Tvalue newDefault)
        {
            Logger.Info("ChangeDefault called.");
            Tvalue oldDefault = DefaultValue;
            _runtimeDefaultValue = (true, newDefault);

            bool isUsingOldDefault = EqualityComparer<Tvalue>.Default.Equals(RawValue, oldDefault);
            if (isUsingOldDefault) SetValue(DefaultValue);
        }

        public override void ResetToDefault()
        {
            Logger.Info("ResetToDefault called.");
            SetValue(DefaultValue);
        }

        private Tvalue GetValue()
        {
            return _useConstant ? _constantValue : RawValue;
        }

        private void SetValue(Tvalue setValue)
        {
            bool ignore = !IsRuntime;
            Logger.Info($"SetValue called. {"(IGNORED)".If(ignore)}");
            if (ignore) return;
            RawValue = setValue;
            TryChange();
        }

        private void TryChange()
        {
            bool ignore = !IsRuntime;
            Logger.Info($"TryChange called. {"(IGNORED)".If(ignore)}");
            if (ignore) return;
            (bool hasPrevValue, Tvalue oldValue) = _prevValue;
            Tvalue newValue = Value;

            bool hasChanged = !hasPrevValue || !EqualityComparer<Tvalue>.Default.Equals(oldValue, newValue);
            if (hasChanged)
            {
                _prevValue = (true, newValue);
                EmitValueChanged(oldValue);
            }
        }

        private void EmitValueChanged(Tvalue oldValue = default)
        {
            Logger.Info("EmitValueChanged called.");
            Tvalue val = Value;
            OnValueChangedExtra.Invoke(this as Tso, val, oldValue);
            OnValueChanged.Invoke(this as Tso);
        }

        private Tvalue ResolveDefaultValue()
            => _runtimeDefaultValue.IsSet ? _runtimeDefaultValue.Value : _defaultValue;

        public void ScriptableCallback_OnAppAwake_Internal() => OnAppBoot();
    }
}
