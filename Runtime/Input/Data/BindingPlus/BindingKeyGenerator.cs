using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.Pool;

namespace SensenToolkit
{
    // Key for uniquely identifying a binding
    // It's specially useful when checking the default bindings to delete
    // when loading the serialized rebinding data
    internal static class BindingKeyGenerator
    {
        public static string GenerateCompositeBindingKey(
            InputAction action,
            InputBinding compositeBinding,
            IEnumerable<InputBinding> compositeParts,
            InputActionMap actionMapOverride = null
        )
        {
            List<string> allKeys = ListPool<string>.Get();
            allKeys.Add(GenerateSingleBindingKey(action, compositeBinding, actionMapOverride));
            List<InputBinding> childrenOrdered = ListPool<InputBinding>.Get();
            childrenOrdered.AddRange(compositeParts);
            childrenOrdered.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
            foreach (InputBinding part in childrenOrdered)
            {
                allKeys.Add(GenerateCompositePartBindingShortKey(part));
            }
            ListPool<InputBinding>.Release(childrenOrdered);

            string key = string.Join(";", allKeys);
            ListPool<string>.Release(allKeys);

            return key;
        }

        public static string GenerateSingleBindingKey(InputActionPlus a, InputBinding b)
            => GenerateSingleBindingKey(a.Action, b, a.ActionMap);
        public static string GenerateSingleBindingKey(
            InputAction a, InputBinding b, InputActionMap actionMapOverride = null
        ) => b.isComposite
            ? $"{InputUtils.GenerateActionKey(a, actionMapOverride)}:composite-head:{b.path}"
            : $"{InputUtils.GenerateActionKey(a, actionMapOverride)}:{b.name}:{b.path}{(b.isPartOfComposite ? ":composite-part" : "")}:{BindingGroups.Normalize(b.groups)}";

        public static string GenerateCompositePartBindingShortKey(InputBinding b)
        {
            Assertx.IsTrue(b.isPartOfComposite, "Binding is not a composite part.");
            return $"{b.name}:{b.path}:{BindingGroups.Normalize(b.groups)}";
        }
    }
}
