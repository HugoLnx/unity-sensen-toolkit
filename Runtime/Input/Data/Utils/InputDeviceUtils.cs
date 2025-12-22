using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SensenToolkit.InputRebinding.Data
{
    public static class InputDeviceUtils
    {
        private static readonly Regex s_blankRegex = new(@"\s+", RegexOptions.Compiled);
        private static readonly Regex s_versionRegex = new(@"\d[\.,\d_-]+", RegexOptions.Compiled);
        private static readonly Regex s_specialCharsRegex = new(@"[^\d\w]", RegexOptions.Compiled);
        private static readonly Regex s_firstWordRegex = new(@"[^a-zA-Z]*([A-Z][A-Z]+|[A-Z][a-z]+|[a-z]+)", RegexOptions.Compiled);
        private static readonly (string Pattern, string Replacement)[] s_deviceShortNameReplacements =
        new (string, string)[]
        {
            ("generic", "Gn"),
            ("usb", "U"),
            ("controller", "Ct"),
            ("gamepad", "Gd"),
            ("joystick", "Jy"),
            ("wired", "Wd"),
            ("wireless", "Ws"),
            ("android", "Ad"),
            ("elite", "El"),
            ("dualshock", "Du"),
        };

        public static string GenerateShortName(InputDevice device)
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

            foreach ((string Pattern, string Replacement) pair in s_deviceShortNameReplacements)
            {
                product = product.Replace(pair.Pattern, pair.Replacement, StringComparison.OrdinalIgnoreCase);
            }
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

        public static string GenerateId(InputDevice device)
        {
            string hashInput = $"{device.name}{device.displayName}{device.description.manufacturer}{device.description.product}{device.description.serial}{device.description.interfaceName}{device.description.version}";
            string fullHash = Hash128.Compute(hashInput).ToString();

            return $"{GenerateShortName(device)}{fullHash[..8]}";
        }
    }
}
