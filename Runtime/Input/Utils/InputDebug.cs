using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SensenToolkit
{
    internal static class InputDebug
    {
        public static void DebugPrintBindingKeys(string title, HashSet<string> keyset)
        {
            List<string> lines = new();
            lines.Add($"---- {title} Binding Keys ({(keyset == null ? "is null" : keyset.Count.ToString())}) ----");
            if (keyset != null && keyset.Count > 0)
            {
                foreach (string key in keyset)
                {
                    lines.Add($"Binding Key: '{key}'");
                }
                lines.Add("----------------------");
            }
            Debug.Log(string.Join("\n", lines));
        }

        public static void DebugPrintBindings(string title, IEnumerable<BindingPlus> bindings)
        {
            List<string> lines = new();
            lines.Add($"---- [Bindings:{bindings.Count()}] {title} ----");
            foreach (BindingPlus binding in bindings)
            {
                InputAction action = binding.Action;
                InputActionMap actionMap = binding.ActionMap;
                string actionKey = InputUtils.GenerateActionKey(action, actionMap);
                InputBinding rawBinding = binding.Binding;
                lines.Add($"['{actionKey}' '{rawBinding.action}' '{rawBinding.path}'] '{rawBinding.name}' '{binding.Key}' 'isComposite: {rawBinding.isComposite}'");
            }
            lines.Add("----------------------");
            Debug.Log(string.Join("\n", lines));
        }

        public static void DebugPrintActions(string title, IInputActionCollection2 actions)
        {
            List<string> lines = new();
            lines.Add($"---- [Actions:{actions.Count()}] {title} ----");
            foreach (InputAction action in actions)
            {
                lines.Add($"'{action.actionMap?.name}' '{action.name}' bindings:{action.bindings.Count}");
            }
            lines.Add("----------------------");
            Debug.Log(string.Join("\n", lines));
        }
    }
}
