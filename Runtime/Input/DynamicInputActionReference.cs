using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SensenToolkit
{
    [System.Serializable]
    public class DynamicInputActionReference
    {
        [SerializeField] private InputActionReference _actionRef;
        private IInputActionCollection2 _actionCollection;
        private InputAction _action;
        private InputAction _bindedAction;
        private bool _originalActionInitialized = false;
        private InputAction _originalAction = null;

        public InputAction Action => ResolveAction();
        public event Action<InputAction> OnBindingAdd = delegate { };
        public event Action<InputAction> OnBindingRemove = delegate { };

        public void SetActionCollection(IInputActionCollection2 actionCollection)
        {
            _actionCollection = actionCollection;
            _action = null;
            if (_bindedAction == null) return;
            EnsureBinded();
        }

        public InputAction OriginalActionClone()
        {
            EnsureOriginalAction();
            return _originalAction?.Clone();
        }

        public void EnsureBinded()
        {
            EnsureUnbinded();
            InputAction action = Action;
            if (action == null) return;

            OnBindingAdd.Invoke(action);
            _bindedAction = action;
        }

        public void EnsureUnbinded()
        {
            if (_bindedAction == null) return;

            OnBindingRemove.Invoke(_bindedAction);
            _bindedAction = null;
        }

        private InputAction ResolveAction()
        {
            if (_action != null) return _action;
            if (_actionRef == null) return null;

            EnsureOriginalAction();

            _action = _actionCollection != null
                ? _actionCollection.FindAction(_actionRef.name)
                : _actionRef.action;


            return _action;
        }

        private void EnsureOriginalAction()
        {
            if (_originalActionInitialized) return;
            _originalAction = _actionRef.action.Clone();
            _originalActionInitialized = true;
        }
    }
}
