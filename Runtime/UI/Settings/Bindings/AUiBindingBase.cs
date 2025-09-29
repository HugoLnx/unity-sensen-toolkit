using System;
using System.Collections;
using MyBox;
using UnityEngine;

namespace SensenToolkit
{
    public abstract class AUiBindingBase<TvalueSO, TrawValue, TuiValue> : AUiBindingBaseAbstract
    where TvalueSO : AValueSO<TrawValue, TvalueSO>
    {
        [SerializeField, MustBeAssigned] protected TvalueSO Value;
        private bool _ignoreChanges = false;

        protected abstract void SetUiValue(TuiValue value);
        protected abstract TuiValue GetUiValue();
        protected abstract TrawValue FromUIValue(TuiValue uiValue);
        protected abstract TuiValue ToUIValue(TrawValue value);
        private WaitForEndOfFrame _waitForEndOfFrame = new();

        protected virtual bool ShouldWaitToAddListener => false;

        protected void OnEnable()
        {
            StartCoroutine(WaitToAddListener());
            BindUiChanges();
        }

        protected void OnDisable()
        {
            Value.RemoveSyncListener(OnValueChanged);
            UnbindUiChanges();
        }

        private IEnumerator WaitToAddListener()
        {
            if (ShouldWaitToAddListener)
            {
                yield return _waitForEndOfFrame;
                yield return _waitForEndOfFrame;
                yield return _waitForEndOfFrame;
            }
            Value.AddSyncListener(OnValueChanged);
        }

        private void OnValueChanged(TvalueSO _)
        {
            if (_ignoreChanges) return;
            _ignoreChanges = true;
            try
            {
                PushCurrentValueToUi();
            }
            finally
            {
                _ignoreChanges = false;
            }
        }

        public override void PushCurrentValueToUi()
        {
            SetUiValue(ToUIValue(Value.Value));
        }

        protected void OnUiValueChanged(TuiValue newUiValue)
        {
            if (_ignoreChanges) return;
            _ignoreChanges = true;
            try
            {
                Value.Value = FromUIValue(newUiValue);
            }
            finally
            {
                _ignoreChanges = false;
            }
        }
    }
}
