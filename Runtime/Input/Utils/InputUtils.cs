using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SensenToolkit
{
    public static class InputUtils
    {
        private static readonly SafeLoop s_safeLoop = new(1000);
        public static string GenerateActionKey(InputAction action, InputActionMap actionMapOverride = null)
        {
            if (action == null) return null;
            InputActionMap actionMap = actionMapOverride ?? action.actionMap;

            return actionMap != null
            ? $"{actionMap.name}/{action.name}"
            : action.name;
        }

        public static void AddNextBindingsToAction(InputAction action, IReadOnlyCollection<InputBinding> bindings, ref int i)
        {
            InputBinding b = bindings.ElementAt(i);

            if (b.isComposite)
            {
                InputActionSetupExtensions.CompositeSyntax composite = action
                    .AddCompositeBinding(b.path, b.effectiveInteractions, b.effectiveProcessors);
                action.ChangeBinding(action.bindings.Count - 1)
                    .WithName(b.name);
                i += 1;
                InputBinding partBinding = bindings.ElementAt(i);
                s_safeLoop.Reset();
                while (partBinding.isPartOfComposite)
                {
                    s_safeLoop.Count();
                    composite = composite.With(
                        name: partBinding.name,
                        binding: partBinding.path,
                        groups: partBinding.groups,
                        processors: partBinding.processors
                    );
                    i += 1;
                    if (i >= bindings.Count) break;
                    partBinding = bindings.ElementAt(i);
                }
                i -= 1;

                return;
            }

            if (b.isPartOfComposite)
            {
                Debug.LogWarning($"[LoadingSerialization] Orphaned part of composite binding found '{b.name}' in action '{action.name}'. Skipping.");
                return;
            }

            InputUtils.AddSingleBindingToAction(action, b);
        }

        public static void AddSingleBindingToAction(InputAction action, InputBinding b)
        {
            action.AddBinding(new InputBinding
            {
                name = b.name,
                path = b.path,
                groups = b.groups,
                action = action.name,
                isComposite = b.isComposite,
                isPartOfComposite = b.isPartOfComposite,
                interactions = b.interactions,
                processors = b.processors,

                overridePath = string.IsNullOrEmpty(b.overridePath)
                    ? null : b.overridePath,
                overrideInteractions = string.IsNullOrEmpty(b.overrideInteractions)
                    ? null : b.overrideInteractions,
                overrideProcessors = string.IsNullOrEmpty(b.overrideProcessors)
                    ? null : b.overrideProcessors,
            });
        }

        public static void RemoveAllBindings(InputAction action)
        {
            EnsureActionDisabled(action, () =>
            {
                s_safeLoop.Reset();
                while (action.bindings.Count > 0)
                {
                    action.ChangeBinding(0).Erase();
                    s_safeLoop.Count();
                }
            });
        }

        public static void EnsureActionDisabled(InputAction action, System.Action behaviour)
        {
            bool wasEnabled = action.enabled;
            if (wasEnabled) action.Disable();
            behaviour?.Invoke();
            if (wasEnabled) action.Enable();
        }
    }
}
