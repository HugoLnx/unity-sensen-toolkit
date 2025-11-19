using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SensenToolkit
{
    public static class BindingDisplayStringUtils
    {
        private static readonly Regex s_blankRegex = new(@"\s+", RegexOptions.Compiled);
        public static string GenerateDisplayStringFor(ClassifiedBinding classifiedBinding, bool shortenForComposite = false)
        {
            string buttonName = CustomButtonNameFor(classifiedBinding, shortenForComposite);

            if (string.IsNullOrEmpty(buttonName))
            {
                buttonName = classifiedBinding.IsComposite
                    ? BuildCompositeDisplayStringFor(classifiedBinding)
                    : classifiedBinding.Binding.ToDisplayString();
            }
            string displayString = classifiedBinding.IsKnownDevice
                ? buttonName
                : BuildUnknownDeviceDisplayString(classifiedBinding.DeviceShortName, buttonName);

            return s_blankRegex.Replace(displayString, "");
        }

        private static string CustomButtonNameFor(ClassifiedBinding cb, bool shortenForComposite = false)
        {
            if (cb.IsComposite) return CustomButtonNameForComposite(cb);
            switch (cb.PathDeviceName)
            {
                case "<Mouse>":
                case "<Pointer>":
                    switch (cb.PathControlName)
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
                    if (cb.PathControlName.Count() == 1)
                    {
                        string keyName = cb.PathControlName.ToUpperInvariant();
                        return shortenForComposite ? keyName : $"Key{keyName}";
                    }
                    else return null;
                case "<Joystick>":
                    if (cb.PathControlName.Equals("trigger", StringComparison.OrdinalIgnoreCase))
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
                        cb.PathControlName.Contains("trigger", StringComparison.OrdinalIgnoreCase)
                        || cb.PathControlName.Contains("shoulder", StringComparison.OrdinalIgnoreCase)
                    )
                    {
                        return cb.PathControlName.Capitalize();
                    }
                    string gpadStr = BuildSubControlDisplayString(cb, "dpad");
                    if (gpadStr != null) return gpadStr;

                    gpadStr = BuildSubControlDisplayString(cb, "leftStick");
                    if (gpadStr != null) return gpadStr;

                    gpadStr = BuildSubControlDisplayString(cb, "rightStick");
                    if (gpadStr != null) return gpadStr;

                    if (cb.PathControlName.Contains("button", StringComparison.OrdinalIgnoreCase))
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
        private static string BuildSubControlDisplayString(ClassifiedBinding cb, string subControlFilter)
        {
            string subControlName = cb.PathSubControlName ?? cb.PathControlName;
            if (subControlName != subControlFilter) return null;

            string compositeGroupName = GetCompositeGroupNameFor(cb) ?? subControlName?.Capitalize();
            return string.IsNullOrEmpty(cb.PathSubControlName)
                ? compositeGroupName
                : $"{compositeGroupName}{cb.PathControlName.Capitalize()}";
        }

        private static string CustomButtonNameForComposite(ClassifiedBinding cb)
        {
            string commonCompositeGroupName = null;
            foreach (ClassifiedBinding part in cb.CompositeParts)
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

        private static string GetCompositeGroupNameFor(ClassifiedBinding part)
        {
            bool isArrowKey = part.IsKeyboardAndMouse && part.PathControlName.EndsWith("Arrow");
            if (isArrowKey) return "ArrowKeys";
            string subControlName = part.PathSubControlName ?? part.PathControlName;

            if (part.PathDeviceName == "<Joystick>")
            {
                switch (subControlName)
                {
                    case "dpad": return "Dpad";
                    case "stick": return "AltLeftStick";
                    case "hat": return "AltDpad";
                    case "z":
                        return "AltRightStick";
                    case "z1":
                        return "AltRightStick2";
                    case "rz":
                        bool isSecondary = part.ParentComposite.CompositeParts.Any(p => p.PathControlName == "z1");
                        return "AltRightStick" + (isSecondary ? "2" : "");
                }
            }
            else
            {
                switch (subControlName)
                {
                    case "dpad": return "Dpad";
                    case "leftStick": return "LeftStick";
                    case "rightStick": return "RightStick";
                }
            }

            return part.PathSubControlName.Capitalize();
    }

        private static string BuildUnknownDeviceDisplayString(string deviceShortName, string buttonName)
            => $"{deviceShortName}#{buttonName}";

        private static string BuildCompositeDisplayStringFor(ClassifiedBinding classifiedBinding)
        {
            List<ClassifiedBinding> compositeParts = classifiedBinding.CompositeParts;
            string subControl = compositeParts[0].PathSubControlName;
            foreach (ClassifiedBinding part in compositeParts)
            {
                if (part.PathSubControlName != subControl)
                {
                    subControl = null;
                    break;
                }
            }

            if (subControl != null)
            {
                return compositeParts[0].IsKnownDevice
                    ? subControl
                    : BuildUnknownDeviceDisplayString(
                        compositeParts[0].DeviceShortName,
                        subControl
                    );
            }

            List<string> displayStrings = new();
            bool areAllSingleChar = true;
            foreach (ClassifiedBinding part in compositeParts)
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
