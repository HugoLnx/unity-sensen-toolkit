using System.Collections.Generic;
using SensenToolkit.InputRebinding.Data;
using UnityEngine.InputSystem;

namespace SensenToolkit.InputRebinding.Internal
{
    public struct KeyListeningMetadata
    {
        public InputBinding NewBinding { get; internal set; }
        public bool IsKnownDevice;
        public string UnknownDeviceGroup;
        public string UnknownDeviceShortName;
    }

    public class KeyListeningMetadataProcessor
    {
        private string _keyboardAndMouseGroup;
        private string _gamepadGroup;

        public KeyListeningMetadataProcessor(string keyboardAndMouseGroup, string gamepadGroup)
        {
            _keyboardAndMouseGroup = keyboardAndMouseGroup;
            _gamepadGroup = gamepadGroup;
        }

        public KeyListeningMetadata ProcessKeyListeningResult(KeyListeningResult result)
        {
            InputAction action = result.Action;
            string newPath = result.NewPath;
            InputDevice device = result.Device;

            var newPathParts = InputBindingPath.FromFullPath(newPath);
            if (device is Joystick && newPathParts.MatchesControl("hat"))
            {
                newPathParts.SetDevice("<Joystick>");
                newPath = newPathParts.AsString;
            }
            bool isKeyboardAndMouse = device is Keyboard || device is Mouse;
            string mainGroup = isKeyboardAndMouse
                ? _keyboardAndMouseGroup
                : _gamepadGroup;
            bool isKnownStandardGamepad = InputUtils.IsKnownStandardGamepadPath(newPath);
            bool isKnownDevice = isKeyboardAndMouse || isKnownStandardGamepad;
            string unknownDeviceGroup = isKnownDevice ? null : InputDeviceUtils.GenerateId(device);
            string unknownDeviceShortName = isKnownDevice ? null : InputDeviceUtils.GenerateShortName(device);

            // bool isAlreadyBound = action.controls.Any(control => InputControlPath.Matches(newPath, control));
            List<string> groupsList = new() { mainGroup };
            if (!isKnownDevice)
            {
                groupsList.Add($"{BindingPlus.DEVICE_ID_PREFIX}{unknownDeviceGroup}");
                groupsList.Add($"{BindingPlus.DEVICE_SHORTNAME_PREFIX}{unknownDeviceShortName}");
            }
            groupsList.Add(BindingPlus.CUSTOM_BINDING_GROUP);
            string groups = InputBindingGroups.Join(groupsList);
            InputBinding newBinding = new()
            {
                path = newPath,
                groups = groups
            };


            return new KeyListeningMetadata
            {
                NewBinding = newBinding,
                IsKnownDevice = isKnownDevice,
                UnknownDeviceGroup = unknownDeviceGroup,
                UnknownDeviceShortName = unknownDeviceShortName,
            };
        }
    }
}
