using System.Collections.Generic;

namespace SensenToolkit.InputRebinding.Internal
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

        public const string ESCAPE_KEY_PATH = "<Keyboard>/escape";
        public static readonly string[] MousePositionPaths = new[]
        {
            "<Mouse>/position",
            "<Mouse>/delta",
            "<VirtualMouse>/position",
            "<VirtualMouse>/delta",
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
