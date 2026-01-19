using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SensenToolkit.InputRebinding.Data;

namespace SensenToolkit.InputRebinding.Internal
{
    public static class BindingDisplayNameGenerator
    {
        private const string BUTTON_SHORTNAME_PREFIX = "Btn";
        private static readonly Regex s_blankRegex = new(@"\s+", RegexOptions.Compiled);
        public static string GenerateDisplayNameFor(BindingPlus plus, bool shortenForComposite = false)
        {
            string buttonName = CustomButtonNameFor(plus, shortenForComposite);

            if (string.IsNullOrEmpty(buttonName))
            {
                buttonName = plus.IsComposite
                    ? BuildCompositeDisplayStringFor(plus)
                    : plus.Binding.ToDisplayString();
            }
            string displayString = plus.IsKnownStandardDevice || shortenForComposite
                ? buttonName
                : BuildUnknownDeviceDisplayString(plus.CustomDeviceShortName, buttonName);

            return s_blankRegex.Replace(displayString, "");
        }

        private static string CustomButtonNameFor(BindingPlus cb, bool shortenForComposite = false)
        {
            if (cb.IsComposite) return CustomButtonNameForComposite(cb);
            switch (cb.Path.Device)
            {
                case "<Mouse>":
                case "<VirtualMouse>":
                case "<Pointer>":
                    bool isMouseMove = cb.Path.MatchesControl("delta") || cb.Path.MatchesControl("position");
                    if (isMouseMove) return "MousePosition";

                    bool isClick = cb.Path.Control.Contains("button", StringComparison.OrdinalIgnoreCase);
                    if (isClick) return cb.Path.Control.Capitalize().Replace("button", "Click", StringComparison.OrdinalIgnoreCase);

                    return null;
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
                        return $"{BUTTON_SHORTNAME_PREFIX}0";
                    }
                    string joyStr = BuildSubControlDisplayString(cb, "stick");
                    if (joyStr != null) return joyStr;

                    joyStr = BuildSubControlDisplayString(cb, "hat");
                    if (joyStr != null) return joyStr;
                    return null;
                default:
                    if (cb.Path.MatchesControl("trigger"))
                    {
                        // If is exactly "trigger", then it's the main trigger button
                        return $"{BUTTON_SHORTNAME_PREFIX}0";
                    }

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
                        string btnSuffix = cb.Binding.ToDisplayString();
                        btnSuffix = s_blankRegex.Replace(btnSuffix, "")
                            .Replace("button", "", StringComparison.OrdinalIgnoreCase)
                            .Replace(BUTTON_SHORTNAME_PREFIX, "", StringComparison.OrdinalIgnoreCase);
                        return $"{BUTTON_SHORTNAME_PREFIX}{btnSuffix.ToUpperInvariant()}";
                    }
                    return null;
            }
        }
        private static string BuildSubControlDisplayString(BindingPlus plus, string subControlFilter)
        {
            string controlName = plus.Path.Control;
            if (controlName != subControlFilter) return null;

            string compositeGroupName = GetCompositeGroupNameFor(plus) ?? controlName?.Capitalize();
            return string.IsNullOrEmpty(plus.Path.ControlPart)
                ? compositeGroupName
                : $"{compositeGroupName}{plus.Path.ControlPart.Capitalize()}";
        }

        private static string CustomButtonNameForComposite(BindingPlus cb)
        {
            string commonCompositeGroupName = null;
            foreach (BindingPlus part in cb.CompositeChildren)
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

        private static string GetCompositeGroupNameFor(BindingPlus part)
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
                        bool isSecondary = part.ParentComposite.CompositeChildren.Any(p => p.Path.MatchesControl("z1"));
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

        private static string BuildCompositeDisplayStringFor(BindingPlus plus)
        {
            IReadOnlyList<BindingPlus> compositeParts = plus.CompositeChildren;
            string controlName = compositeParts[0].Path.Control;
            foreach (BindingPlus part in compositeParts)
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
                        compositeParts[0].CustomDeviceShortName,
                        controlName
                    );
            }

            List<string> displayStrings = new();
            bool areAllSingleChar = true;
            foreach (BindingPlus part in compositeParts)
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
