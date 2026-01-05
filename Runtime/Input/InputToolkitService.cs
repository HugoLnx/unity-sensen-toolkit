using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SensenToolkit
{
    public class InputToolkitService : APermanentSingleton<InputToolkitService>
    {
        public const string DEFAULT_KEYBOARD_AND_MOUSE_GROUP = "KeyboardAndMouse";
        public const string DEFAULT_GAMEPAD_GROUP = "Gamepad";
        [SerializeField] private string _keyboardAndMouseBindingGroup = DEFAULT_KEYBOARD_AND_MOUSE_GROUP;
        [SerializeField] private string _gamepadBindingGroup = DEFAULT_GAMEPAD_GROUP;
        private HashSet<string> _defaultBindingGroups;

        public string BindingGroupKeyboardAndMouse => _keyboardAndMouseBindingGroup;
        public string BindingGroupGamepad => _gamepadBindingGroup;
        public HashSet<string> BindingGroups => _defaultBindingGroups ??= new HashSet<string>
        {
            _keyboardAndMouseBindingGroup,
            _gamepadBindingGroup
        };

        private IInputActionCollection2 _actions;
        private IInputActionCollection2 _originalActions;

        public IInputActionCollection2 Actions => _actions;
        public IInputActionCollection2 OriginalActions => _originalActions;
        private event Action<IInputActionCollection2> OnActionsChanged;

        public void BindActionCollection(Action<IInputActionCollection2> setActions)
        {
            if (_actions != null)
            {
                setActions(_actions);
            }
            OnActionsChanged += setActions;
        }

        public void UnbindActionCollection(Action<IInputActionCollection2> setActions)
        {
            OnActionsChanged -= setActions;
        }

        public void SetActions(IInputActionCollection2 actions, IInputActionCollection2 originalActions = null)
        {
            IInputActionCollection2 oldActions = _actions;
            _actions = actions;
            _originalActions = originalActions;
            if (oldActions != actions)
            {
                OnActionsChanged?.Invoke(_actions);
            }
        }
    }
}
