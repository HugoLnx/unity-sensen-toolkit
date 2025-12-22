using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MyBox;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SensenToolkit
{
    public struct RebindingMetadata
    {
        public InputBinding NewBinding { get; internal set; }
        public bool IsKnownDevice;
        // public bool IsAlreadyBound { get; internal set; }
        public string UnknownDeviceGroup;
        public string UnknownDeviceShortName;
    }

    public class RebindingMetadataProcessor
    {
        public const string ESCAPE_KEY_PATH = "<Keyboard>/escape";
        public static readonly string[] MousePositionPaths = new[]
        {
            "<Mouse>/position",
            "<Mouse>/delta",
            "<Pointer>/position",
            "<Pointer>/delta",
        };

        public static readonly string[] KeyboardArrowPaths = new[]
        {
            "<Keyboard>/upArrow",
            "<Keyboard>/downArrow",
            "<Keyboard>/leftArrow",
            "<Keyboard>/rightArrow",
        };

        public static readonly string[] GamepadDpadPaths = new[]
        {
            "<Gamepad>/dpad",
            "<Gamepad>/dpad/up",
            "<Gamepad>/dpad/down",
            "<Gamepad>/dpad/left",
            "<Gamepad>/dpad/right",
        };

        public static readonly string[] GamepadLeftStickPaths = new[]
        {
            "<Gamepad>/leftStick",
            "<Gamepad>/leftStick/up",
            "<Gamepad>/leftStick/down",
            "<Gamepad>/leftStick/left",
            "<Gamepad>/leftStick/right",
        };

        private static readonly Regex s_blankRegex = new(@"\s+", RegexOptions.Compiled);
        private static readonly Regex s_versionRegex = new(@"\d[\.,\d_-]+", RegexOptions.Compiled);
        private static readonly Regex s_specialCharsRegex = new(@"[^\d\w]", RegexOptions.Compiled);
        private static readonly Regex s_firstWordRegex = new(@"[^a-zA-Z]*([A-Z][A-Z]+|[A-Z][a-z]+|[a-z]+)", RegexOptions.Compiled);
        private InputToolkitService _inputToolkit;
        private InputAction _originalAction;

        public RebindingMetadataProcessor(InputToolkitService inputToolkit, InputAction originalAction)
        {
            _inputToolkit = inputToolkit;
            _originalAction = originalAction;
        }

        public RebindingMetadata ProcessKeyListeningResult(KeyListeningResult result)
        {
            InputAction action = result.Action;
            string newPath = result.NewPath;
            InputDevice device = result.Device;

            var newPathParts = BindingPathComponents.FromFullPath(newPath);
            if (device is Joystick && newPathParts.MatchesControl("hat"))
            {
                newPathParts.SetDevice("<Joystick>");
                newPath = newPathParts.AsString;
            }
            bool isKeyboardAndMouse = device is Keyboard || device is Mouse;
            string mainGroup = isKeyboardAndMouse
                ? _inputToolkit.BindingGroupKeyboardAndMouse
                : _inputToolkit.BindingGroupGamepad;
            bool isKnownStandardGamepad = IsKnownStandardGamepadPath(newPath);
            bool isKnownDevice = isKeyboardAndMouse || isKnownStandardGamepad;
            string unknownDeviceGroup = isKnownDevice ? null : DeviceToGroupName(device);
            string unknownDeviceShortName = isKnownDevice ? null : DeviceShortName(device);

            // bool isAlreadyBound = action.controls.Any(control => InputControlPath.Matches(newPath, control));
            List<string> groupsList = new() { mainGroup };
            if (!isKnownDevice)
            {
                groupsList.Add($"{BindingPlus.DEVICE_ID_PREFIX}{unknownDeviceGroup}");
                groupsList.Add($"{BindingPlus.DEVICE_SHORTNAME_PREFIX}{unknownDeviceShortName}");
            }
            groupsList.Add(BindingPlus.CUSTOM_BINDING_GROUP);
            string groups = BindingGroups.Join(groupsList);
            InputBinding newBinding = new()
            {
                path = newPath,
                groups = groups
            };


            return new RebindingMetadata
            {
                NewBinding = newBinding,
                IsKnownDevice = isKnownDevice,
                UnknownDeviceGroup = unknownDeviceGroup,
                UnknownDeviceShortName = unknownDeviceShortName,
            };
        }

        private static string DeviceShortName(InputDevice device)
        {
            string manufacturer = (device.description.manufacturer ?? "").Trim();
            string product = (device.description.product ?? "").Trim();

            if (!String.IsNullOrEmpty(manufacturer)) manufacturer = s_blankRegex.Replace(manufacturer, "");
            product = s_blankRegex.Replace(product, " ");

            if (String.IsNullOrEmpty(manufacturer) && !String.IsNullOrEmpty(product))
            {
                Match firstWordMatch = s_firstWordRegex.Match(product);
                if (firstWordMatch.Success
                    && firstWordMatch.Length < (product.Length - 2)
                    && firstWordMatch.Length >= 2)
                {
                    manufacturer = firstWordMatch.Value;
                    product = product
                    .Replace(manufacturer, "", StringComparison.OrdinalIgnoreCase)
                    .Trim();
                }
            }

            product = s_blankRegex.Replace(product, "");
            if (String.IsNullOrEmpty(product)) product = device.name ?? "Unknown";

            product = product
                .Replace("generic", "Gn", StringComparison.OrdinalIgnoreCase)
                .Replace("usb", "U", StringComparison.OrdinalIgnoreCase)
                .Replace("controller", "Ct", StringComparison.OrdinalIgnoreCase)
                .Replace("gamepad", "Gd", StringComparison.OrdinalIgnoreCase)
                .Replace("joystick", "Jy", StringComparison.OrdinalIgnoreCase)
                .Replace("wired", "Wd", StringComparison.OrdinalIgnoreCase)
                .Replace("wireless", "Ws", StringComparison.OrdinalIgnoreCase)
                .Replace("android", "Ad", StringComparison.OrdinalIgnoreCase)
                .Replace("elite", "El", StringComparison.OrdinalIgnoreCase)
                .Replace("dualshock", "Du", StringComparison.OrdinalIgnoreCase);
            Match versionMatch = s_versionRegex.Match(device.name);
            string version = versionMatch.Success ? versionMatch.Value : "";
            if (!string.IsNullOrEmpty(version))
            {
                product = product
                    .Replace(version, "", StringComparison.OrdinalIgnoreCase)
                    .Trim();
                version = s_specialCharsRegex.Replace(version, "");
            }

            const int TARGET_LENGTH = 7;
            const int MAX_VERSION_LENGTH = 3;
            int manufacturerLength = Mathf.Min(3, manufacturer.Length);
            int versionLength = Mathf.Min(MAX_VERSION_LENGTH, version.Length);
            int productLength = Mathf.Min(TARGET_LENGTH - manufacturerLength, product.Length);
            string shortName = String.IsNullOrEmpty(manufacturer)
                ? ""
                : manufacturer[..manufacturerLength].Capitalize(forceLowerEnding: true);
            shortName += product[..productLength].Capitalize(forceLowerEnding: true);
            shortName += version[..versionLength].ToLowerInvariant();
            return shortName;
        }

        private static string DeviceToGroupName(InputDevice device)
        {
            string hashInput = $"{device.name}{device.displayName}{device.description.manufacturer}{device.description.product}{device.description.serial}{device.description.interfaceName}{device.description.version}";
            string fullHash = Hash128.Compute(hashInput).ToString();

            return $"{DeviceShortName(device)}{fullHash[..8]}";
        }

        private static bool IsKnownStandardGamepadPath(string path)
        {
            var pathComponents = BindingPathComponents.FromFullPath(path);
            bool isGamepadPath = pathComponents.Device.Equals("<gamepad>", StringComparison.OrdinalIgnoreCase);
            if (isGamepadPath) return true;

            bool isJoystickPath = pathComponents.Device.Equals("<joystick>", StringComparison.OrdinalIgnoreCase);
            // If is not joystick nor gamepad path, then it's not known gamepad
            if (!isJoystickPath) return false;

            // <Joystick>/Trigger has different trigger button on different joystick models
            bool isStandardizedJoystickPath = !pathComponents.Control.Equals("trigger", StringComparison.OrdinalIgnoreCase);
            return isStandardizedJoystickPath;
        }
    }
}
