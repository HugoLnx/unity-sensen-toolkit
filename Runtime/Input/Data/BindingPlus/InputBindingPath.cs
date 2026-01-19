using System;
using System.Linq;
using System.Text.RegularExpressions;
using SensenToolkit.InputRebinding.Internal;
using UnityEngine;

namespace SensenToolkit.InputRebinding.Data
{
    public class InputBindingPath
    {
        private static readonly Regex s_deviceNamePattern = new(@"^(\<[^>]+\>)", RegexOptions.Compiled);
        /*
            <Mouse>/leftButton => "<Mouse>", "leftButton", null
            <Gamepad>/leftStick/up => "<Gamepad>", "leftStick", "up"
        */
        public string Device { get; private set; }
        public string Control { get; private set; }
        public string ControlPart { get; private set; }
        private string _fullPath;
        public string AsString => _fullPath ??= BuildStringJoined();
        public bool IsVector2CompositePart
            => InputConstants.Vector2CompositeControls.Any(MatchesControlPart);

        public InputBindingPath(string device, string control, string controlPart = null)
        {
            Device = device;
            Control = control;
            ControlPart = controlPart;
        }

        public static InputBindingPath FromFullPath(string path)
        {
            string device = ExtractDevice(path);
            string[] pathParts = path[(device.Length + 1)..].Split('/', StringSplitOptions.RemoveEmptyEntries);
            return new(
                device: device,
                control: pathParts.Length >= 1 ? pathParts[0] : null,
                controlPart: pathParts.Length >= 2 ? string.Join('/', pathParts[1..^0]) : null
            );
        }

        public InputBindingPath Clone() => new(Device, Control, ControlPart);

        public void SetDevice(string device)
        {
            Device = device;
            _fullPath = null;
        }

        public void SetControl(string control)
        {
            Control = control;
            _fullPath = null;
        }

        public InputBindingPath SetControlPart(string controlPart)
        {
            ControlPart = controlPart;
            _fullPath = null;

            return this;
        }

        public bool MatchesDevice(string device) => device == this.Device
                || this.Device?.Equals(device ?? "", StringComparison.OrdinalIgnoreCase) == true;
        public bool MatchesControl(string control) => control == this.Control
            || this.Control?.Equals(control ?? "", StringComparison.OrdinalIgnoreCase) == true;
        public bool MatchesControlPart(string controlPart) => controlPart == this.ControlPart
            || this.ControlPart?.Equals(controlPart ?? "", StringComparison.OrdinalIgnoreCase) == true;

        public static string ExtractDevice(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            Match match = s_deviceNamePattern.Match(path);
            if (!match.Success)
            {
                Debug.LogWarning($"[InputBindingPath] ExtractDevice: No device found in path '{path}'");
                return null;
            }

            return match.Groups[1].Value;
        }

        private string BuildStringJoined()
        {
            if (string.IsNullOrEmpty(Control)) return Device;
            return string.IsNullOrEmpty(ControlPart)
                ? $"{Device}/{Control}"
                : $"{Device}/{Control}/{ControlPart}";
        }
    }
}
