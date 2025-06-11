using System;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace SensenToolkit
{
    public abstract class AValueSO<Tvalue, Tso> : AValueSOBase
        where Tso : AValueSO<Tvalue, Tso>
    {
        [SerializeField, MustBeAssigned] private string _name = null;
        [Tooltip("Use it if you don't want it to be stored across the sessions. (eg. for settings)")]
        [SerializeField] private bool _resetToDefaultOnEnable = true;
        [Tooltip("Value to reset.")]
        [SerializeField] private Tvalue _defaultValue;
        [Tooltip("Forces default to RawValue while its not playing")]
        [SerializeField] private bool _forceDefaultWhileEditing = true;
        [Tooltip("Forces to use constant value instead of RawValue")]
        [SerializeField] private bool _useConstant = false;
        [SerializeField, ConditionalField(nameof(_useConstant))]
        private Tvalue _constantValue;
        [SerializeField, ReadOnly] protected Tvalue RawValue;
        [SerializeField, HideInInspector] private bool _wasInitialized = false;
        private Tvalue _prevValue;

        public Tvalue Value { get => GetValue(); set => SetValue(value); }
        public Tvalue DefaultValue => _defaultValue;
        public override object ValueAsObject => Value;
        public override string Name => _name;

        public delegate void ExtraValueChangedHandler(Tso source, Tvalue newValue, Tvalue oldValue);
        public event ExtraValueChangedHandler OnValueChangedExtra = delegate { };
        public event Action<Tso> OnValueChanged = delegate { };

        protected void OnEnable()
        {
            TryInitialize();
            TryResetToDefault();
        }

        protected void OnDisable()
        {
            TryResetToDefault();
            OnValueChangedExtra = delegate { }; // Unsubscribe all listeners
            OnValueChanged = delegate { }; // Unsubscribe all listeners
        }

        protected void OnValidate()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && _forceDefaultWhileEditing) SetValue(_defaultValue);
#endif
            TryInitialize();
            TryChange();
        }

        public void AddSyncListener(Action<Tso> listener)
        {
            listener(this as Tso);
            OnValueChanged += listener;
        }

        public void RemoveSyncListener(Action<Tso> listener)
        {
            OnValueChanged -= listener;
        }

        private void TryResetToDefault()
        {
            if (!_resetToDefaultOnEnable) return;
            ResetToDefault();
        }

        public override void ResetToDefault()
        {
            SetValue(_defaultValue);
        }

        private Tvalue GetValue()
        {
            return _useConstant ? _constantValue : RawValue;
        }

        private void SetValue(Tvalue setValue)
        {
            _wasInitialized = true;
            RawValue = setValue;
            TryChange();
        }

        private void TryChange()
        {
            Tvalue oldValue = _prevValue;
            Tvalue newValue = Value;

            bool hasChanged = !EqualityComparer<Tvalue>.Default.Equals(oldValue, newValue);
            if (hasChanged)
            {
                _prevValue = newValue;
                EmitValueChanged(oldValue);
            }
        }

        private void EmitValueChanged(Tvalue oldValue = default)
        {
            Tvalue val = Value;
            OnValueChangedExtra.Invoke(this as Tso, val, oldValue);
            OnValueChanged.Invoke(this as Tso);
        }

        private void TryInitialize()
        {
            if (_wasInitialized) return;
            RawValue = _defaultValue;
        }
    }
}
