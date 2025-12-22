using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace SensenToolkit
{
    // Key for fast checking if two bindings have the same behaviour/role
    internal static class BindingEquivalenceKeyGenerator
    {

        public static string GenerateCompositeBindingEquivalenceKey(
            InputAction a, IEnumerable<InputBinding> compositeParts, InputActionMap actionMapOverride = null
        )
        {
            List<string> partKeys = new();
            foreach (InputBinding part in compositeParts)
            {
                partKeys.Add($"{part.name}:{GenerateSingleBindingEquivalenceKey(a, part, actionMapOverride)}");
            }
            partKeys.Sort((a, b) => string.Compare(a, b, StringComparison.Ordinal));
            return string.Join("|", partKeys);
        }

        public static string GenerateSingleBindingEquivalenceKey(
            InputAction action, InputBinding binding, InputActionMap actionMap
        ) => binding.isComposite
            ? throw new InvalidOperationException("Composite bindings do not have equivalence keys without their parts.")
            : $"{InputUtils.GenerateActionKey(action, actionMap)}:{binding.effectivePath}:{(binding.isPartOfComposite ? ":composite-part" : "")}";
    }
}
