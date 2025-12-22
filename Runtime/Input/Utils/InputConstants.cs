using System.Collections.Generic;

namespace SensenToolkit
{
    public static class InputConstants
    {
        public static readonly IReadOnlyList<string> Vector2CompositeNames = new List<string>()
        {
            "up", "right", "down", "left",
        };
        public static readonly IReadOnlyList<string> AxisCompositeNames = new List<string>()
        {
            "positive", "negative",
        };

        public static IReadOnlyList<string> Vector2CompositeControls => Vector2CompositeNames;
        private static Dictionary<string, int> s_compositeNamesOrderIndexes;
        public static Dictionary<string, int> CompositeNamesOrderIndexes => s_compositeNamesOrderIndexes ??= GenerateCompositeNamesOrderIndexes();

        private static Dictionary<string, int> GenerateCompositeNamesOrderIndexes()
        {
            int priority = 0;
            Dictionary<string, int> priorityDict = new();

            foreach (string name in AxisCompositeNames)
            {
                priorityDict[name] = priority;
                // priorityDict[name.ToLowerInvariant()] = priority;
                priority += 1;
            }

            foreach (string name in Vector2CompositeNames)
            {
                priorityDict[name] = priority;
                // priorityDict[name.ToLowerInvariant()] = priority;
                priority += 1;
            }

            return priorityDict;
        }
    }
}
