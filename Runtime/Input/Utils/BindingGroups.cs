using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem.Utilities;

namespace SensenToolkit
{
    public static class BindingGroups
    {
        public const char SEPARATOR = ';';

        public static string Join(IEnumerable<string> groups)
            => string.Join(SEPARATOR, groups);

        public static IEnumerable<string> Split(string groups)
        {
            if (string.IsNullOrEmpty(groups)) yield break;
            IEnumerable<string> enumerable = groups
                .Split(SEPARATOR, StringSplitOptions.RemoveEmptyEntries)
                .Select(g => g.Trim())
                .Where(g => !string.IsNullOrEmpty(g));

            foreach (string g in enumerable)
            {
                yield return g;
            }
        }

        public static string Normalize(string groups)
        {
            if (string.IsNullOrEmpty(groups)) return string.Empty;
            return Join(Split(groups));
        }
    }
}
