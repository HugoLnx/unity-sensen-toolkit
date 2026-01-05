using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SensenToolkit
{
    public class InputToolkitService : APermanentSingleton<InputToolkitService>
    {
        private const string DEFAULT_KEYBOARD_AND_MOUSE_GROUP = "KeyboardAndMouse";
        private const string DEFAULT_GAMEPAD_GROUP = "Gamepad";
        public static string BindingGroupKeyboardAndMouse
            => s_bindingGroupKeyboardAndMouse ??= ResolveGroupKeyboardAndMouse();
        public static string BindingGroupGamepad
            => s_bindingGroupGamepad ??= ResolveGroupGamepad();
        private static string s_bindingGroupKeyboardAndMouse = null;
        private static string s_bindingGroupGamepad = null;

        [SerializeField] private string _keyboardAndMouseBindingGroup = DEFAULT_KEYBOARD_AND_MOUSE_GROUP;
        [SerializeField] private string _gamepadBindingGroup = DEFAULT_GAMEPAD_GROUP;
        private HashSet<string> _defaultBindingGroups;

        public string ConfigKeyboardAndMouseGroup => _keyboardAndMouseBindingGroup;
        public string ConfigGamepadGroup => _gamepadBindingGroup;
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

        private static string ResolveGroupGamepad()
        {
            InputToolkitService instance = HasInstance ? Instance : null;
            if (instance == null || string.IsNullOrWhiteSpace(instance._gamepadBindingGroup))
            {
                return DEFAULT_GAMEPAD_GROUP;
            }
            return instance._gamepadBindingGroup;
        }

        private static string ResolveGroupKeyboardAndMouse()
        {
            InputToolkitService instance = HasInstance ? Instance : null;
            if (instance == null || string.IsNullOrWhiteSpace(instance.ConfigKeyboardAndMouseGroup))
            {
                return DEFAULT_KEYBOARD_AND_MOUSE_GROUP;
            }
            return instance.ConfigKeyboardAndMouseGroup;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset1()
        {
            s_bindingGroupKeyboardAndMouse = null;
            s_bindingGroupGamepad = null;
        }
    }
}
