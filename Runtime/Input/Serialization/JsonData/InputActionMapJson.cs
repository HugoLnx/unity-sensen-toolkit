using UnityEngine.InputSystem;
using UnityEngine;
using System.Reflection;

namespace SensenToolkit.InputRebinding.Internal
{
    [System.Serializable]
    public class InputActionMapJson
    {
        private static readonly FieldInfo s_fieldId = typeof(InputActionMap).GetField("m_Id", BindingFlags.NonPublic | BindingFlags.Instance);

        [SerializeField] public string Id;
        [SerializeField] public string Name;
        public InputActionMap ActionMap => _actionMap ??= ToInputActionMap();
        private InputActionMap _actionMap;

        public static InputActionMapJson From(InputActionMap actionMap)
        {
            return new InputActionMapJson
            {
                Id = actionMap.id.ToString(),
                Name = actionMap.name,
            };
        }

        public InputActionMap ToInputActionMap()
        {
            InputActionMap actionMap = new(Name);
            s_fieldId.SetValue(actionMap, Id);
            return actionMap;
        }
    }
}
