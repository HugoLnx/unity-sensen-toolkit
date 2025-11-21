using System;
using System.Linq;

namespace SensenToolkit
{
    public class BindingPathComponents
    {
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
            => RebindingMetadataProcessor.Vector2CompositeOrder.Any(MatchesControlPart);

        public BindingPathComponents(string device, string control, string controlPart = null)
        {
            Device = device;
            Control = control;
            ControlPart = controlPart;
        }

        public static BindingPathComponents FromFullPath(string path)
        {
            string[] pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            return new(
                device: pathParts.Length >= 1 ? pathParts[0] : null,
                control: pathParts.Length >= 2 ? pathParts[1] : null,
                controlPart: pathParts.Length >= 3 ? string.Join('/', pathParts[2..^0]) : null
            );
        }

        public BindingPathComponents Clone() => new(Device, Control, ControlPart);

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

        public void SetControlPart(string controlPart)
        {
            ControlPart = controlPart;
            _fullPath = null;
        }

        public bool MatchesDevice(string device) => device == this.Device
                || this.Device?.Equals(device ?? "", StringComparison.OrdinalIgnoreCase) == true;
        public bool MatchesControl(string control) => control == this.Control
            || this.Control?.Equals(control ?? "", StringComparison.OrdinalIgnoreCase) == true;
        public bool MatchesControlPart(string controlPart) => controlPart == this.ControlPart
            || this.ControlPart?.Equals(controlPart ?? "", StringComparison.OrdinalIgnoreCase) == true;

        public static string ExtractDevice(string path)
        {
            int slashIndex = path.IndexOf('/');
            if (slashIndex < 0) return path;
            return path[..slashIndex];
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
