using UnityEngine.InputSystem;

namespace SensenToolkit
{
    [System.Serializable]
    public class InputActionPlus
    {
        public InputAction Action;
        public InputActionMap ActionMap;
        public string Key => _key ??= InputUtils.GenerateActionKey(Action, ActionMap);
        private string _key = null;

        public static InputActionPlus FromInputAction(InputAction action, InputActionMap actionMapOverride = null)
        {
            return new InputActionPlus
            {
                Action = action,
                ActionMap = actionMapOverride ?? action.actionMap,
            };
        }
    }
}
