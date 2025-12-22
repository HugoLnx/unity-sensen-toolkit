using UnityEngine.InputSystem;
using SensenToolkit;
using System;
using UnityEngine;
using System.Reflection;

namespace SensenToolkit.InputRebindingSerialization
{
    [System.Serializable]
    internal class InputActionJson
    {
        private static readonly FieldInfo s_fieldName = typeof(InputAction).GetField("m_Name", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo s_fieldType = typeof(InputAction).GetField("m_Type", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo s_fieldExpectedControlType = typeof(InputAction).GetField("m_ExpectedControlType", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo s_fieldId = typeof(InputAction).GetField("m_Id", BindingFlags.NonPublic | BindingFlags.Instance);

        [SerializeField] public string ActionMapIdRaw;
        [SerializeField] public string Id;
        [SerializeField] public string Name;
        [SerializeField] public InputActionType Type;
        [SerializeField] public string ExpectedControlType;
        [SerializeField] public bool WantsInitialStateCheck;

        private Guid? _actionMapId;
        public Guid? ActionMapId => _actionMapId ??= string.IsNullOrEmpty(ActionMapIdRaw)
            ? null
            : Guid.Parse(ActionMapIdRaw);

        public InputAction Action => _action ??= ToInputAction();


        private InputAction _action = null;

        public static InputActionJson From(InputAction action)
        {
            string mapId = null;
            Guid? mapGuid = action.actionMap?.id;
            if (mapGuid != null && mapGuid != Guid.Empty)
            {
                mapId = mapGuid.Value.ToString();
            }
            return new InputActionJson
            {
                ActionMapIdRaw = mapId,
                Name = action.name,
                Type = action.type,
                ExpectedControlType = action.expectedControlType,
                Id = action.id.ToString(),
                WantsInitialStateCheck = action.wantsInitialStateCheck,
            };
        }

        private InputAction ToInputAction()
        {
            InputAction action = new(Name, Type)
            {
                wantsInitialStateCheck = WantsInitialStateCheck,
            };
            s_fieldName.SetValue(action, Name);
            s_fieldType.SetValue(action, Type);
            s_fieldExpectedControlType.SetValue(action, ExpectedControlType);
            s_fieldId.SetValue(action, Id);
            return action;
        }
    }
}
