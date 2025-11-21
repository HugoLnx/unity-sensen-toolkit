using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SensenToolkit
{
    public static class BindingDisplayStringUtils
    {
        private static readonly Regex s_blankRegex = new(@"\s+", RegexOptions.Compiled);
        public static string GenerateDisplayStringFor(BindingMetadata bindingMetadata, bool shortenForComposite = false)
        {
            string buttonName = CustomButtonNameFor(bindingMetadata, shortenForComposite);

            if (string.IsNullOrEmpty(buttonName))
            {
                buttonName = bindingMetadata.IsComposite
                    ? BuildCompositeDisplayStringFor(bindingMetadata)
                    : bindingMetadata.Binding.ToDisplayString();
            }
            string displayString = bindingMetadata.IsKnownStandardDevice || shortenForComposite
                ? buttonName
                : BuildUnknownDeviceDisplayString(bindingMetadata.DeviceShortName, buttonName);

            return s_blankRegex.Replace(displayString, "");
        }

        private static string CustomButtonNameFor(BindingMetadata cb, bool shortenForComposite = false)
        {
            if (cb.IsComposite) return CustomButtonNameForComposite(cb);
            switch (cb.Path.Device)
            {
                case "<Mouse>":
                case "<Pointer>":
                    switch (cb.Path.Control)
                    {
                        case "leftButton": return "LeftClick";
                        case "rightButton": return "RightClick";
                        case "middleButton": return "MiddleClick";
                        case "delta":
                        case "position":
                            return "MousePosition";
                        default: return null;
                    }
                case "<Keyboard>":
                    if (cb.Path.Control.Count() == 1)
                    {
                        string keyName = cb.Path.Control.ToUpperInvariant();
                        return shortenForComposite ? keyName : $"Key{keyName}";
                    }
                    else return null;
                case "<Joystick>":
                    if (cb.Path.MatchesControl("trigger"))
                    {
                        return "Btn0";
                    }
                    string joyStr = BuildSubControlDisplayString(cb, "stick");
                    if (joyStr != null) return joyStr;

                    joyStr = BuildSubControlDisplayString(cb, "hat");
                    if (joyStr != null) return joyStr;
                    return null;
                default:
                    if (
                        cb.Path.Control.Contains("trigger", StringComparison.OrdinalIgnoreCase)
                        || cb.Path.Control.Contains("shoulder", StringComparison.OrdinalIgnoreCase)
                    )
                    {
                        return cb.Path.Control.Capitalize();
                    }
                    string gpadStr = BuildSubControlDisplayString(cb, "dpad");
                    if (gpadStr != null) return gpadStr;

                    gpadStr = BuildSubControlDisplayString(cb, "leftStick");
                    if (gpadStr != null) return gpadStr;

                    gpadStr = BuildSubControlDisplayString(cb, "rightStick");
                    if (gpadStr != null) return gpadStr;

                    if (cb.Path.Control.Contains("button", StringComparison.OrdinalIgnoreCase))
                    {
                        gpadStr = cb.Binding.ToDisplayString();
                        gpadStr = gpadStr.Replace("utton", "tn", StringComparison.OrdinalIgnoreCase);
                        gpadStr = s_blankRegex.Replace(gpadStr, "");
                        if (gpadStr.Contains("Btn")) return gpadStr;
                        else return $"Btn{gpadStr}";
                    }
                    return null;
            }
        }
        private static string BuildSubControlDisplayString(BindingMetadata cb, string subControlFilter)
        {
            string controlName = cb.Path.Control;
            if (controlName != subControlFilter) return null;

            string compositeGroupName = GetCompositeGroupNameFor(cb) ?? controlName?.Capitalize();
            return string.IsNullOrEmpty(cb.Path.ControlPart)
                ? compositeGroupName
                : $"{compositeGroupName}{cb.Path.ControlPart.Capitalize()}";
        }

        private static string CustomButtonNameForComposite(BindingMetadata cb)
        {
            string commonCompositeGroupName = null;
            foreach (BindingMetadata part in cb.CompositeParts)
            {
                string compositeGroupName = GetCompositeGroupNameFor(part);

                if (compositeGroupName == null
                    || (commonCompositeGroupName != null && commonCompositeGroupName != compositeGroupName))
                {
                    return null;
                }
                commonCompositeGroupName = compositeGroupName;
            }

            return commonCompositeGroupName;
        }

        private static string GetCompositeGroupNameFor(BindingMetadata part)
        {
            bool isArrowKey = part.IsKeyboardAndMouse && part.Path.Control.EndsWith("Arrow");
            if (isArrowKey) return "ArrowKeys";

            if (part.Path.MatchesDevice("<Joystick>"))
            {
                switch (part.Path.Control)
                {
                    case "dpad": return "Dpad";
                    case "stick": return "AltLeftStick";
                    case "hat": return "AltDpad";
                    case "z":
                        return "AltRightStick";
                    case "z1":
                        return "AltRightStick2";
                    case "rz":
                        bool isSecondary = part.ParentComposite.CompositeParts.Any(p => p.Path.MatchesControl("z1"));
                        return "AltRightStick" + (isSecondary ? "2" : "");
                }
            }
            else
            {
                switch (part.Path.Control)
                {
                    case "dpad": return "Dpad";
                    case "leftStick": return "LeftStick";
                    case "rightStick": return "RightStick";
                }
            }

            return part.Path.Control.Capitalize();
    }

        private static string BuildUnknownDeviceDisplayString(string deviceShortName, string buttonName)
            => $"{deviceShortName}#{buttonName}";

        private static string BuildCompositeDisplayStringFor(BindingMetadata bindingMetadata)
        {
            List<BindingMetadata> compositeParts = bindingMetadata.CompositeParts;
            string controlName = compositeParts[0].Path.Control;
            foreach (BindingMetadata part in compositeParts)
            {
                if (!part.Path.MatchesControl(controlName))
                {
                    controlName = null;
                    break;
                }
            }

            if (controlName != null)
            {
                return compositeParts[0].IsKnownStandardDevice
                    ? controlName
                    : BuildUnknownDeviceDisplayString(
                        compositeParts[0].DeviceShortName,
                        controlName
                    );
            }

            List<string> displayStrings = new();
            bool areAllSingleChar = true;
            foreach (BindingMetadata part in compositeParts)
            {
                string partDisplayString = part.DisplayStringShortenedForComposite;
                displayStrings.Add(partDisplayString);
                if (partDisplayString.Length != 1) areAllSingleChar = false;
            }
            return areAllSingleChar
                ? string.Concat(displayStrings)
                : string.Join(",", compositeParts.Select(part => part.DisplayStringShortenedForComposite));
        }
    }
}
