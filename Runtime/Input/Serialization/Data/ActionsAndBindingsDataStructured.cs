using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace SensenToolkit
{
    public class ActionsAndBindingsDataStructured
    {
        public BindingPlusCollection AllBindings { get; private set; }
        private readonly Dictionary<string, BindingPlusCollection> _bindingsByActionKey = new();
        public IReadOnlyDictionary<string, BindingPlusCollection> BindingsByActionKey => _bindingsByActionKey;
        public HashSet<InputActionMap> ActionMaps { get; } = new();

        public ActionsAndBindingsDataStructured(IInputActionCollection2 actions)
        {
            AllBindings = BuildAllBindings(actions);

            Dictionary<string, List<BindingPlus>> bindingsByActionKey = new();
            foreach (BindingPlus binding in AllBindings)
            {
                if (binding.ActionMap != null) ActionMaps.Add(binding.ActionMap);
                string actionKey = binding.ActionKey;
                if (!bindingsByActionKey.TryGetValue(actionKey, out List<BindingPlus> builtBindings))
                {
                    builtBindings = new List<BindingPlus>();
                    bindingsByActionKey[actionKey] = builtBindings;
                }

                builtBindings.Add(binding);
            }

            foreach ((string actionKey, List<BindingPlus> bindings) in bindingsByActionKey)
            {
                _bindingsByActionKey[actionKey] = BindingPlusCollection.Build(bindings);
            }
        }

        private BindingPlusCollection BuildAllBindings(IInputActionCollection2 actions)
        {
            BindingPlusCollectionBuilder builder = new();
            foreach (InputAction action in actions)
            {
                builder.AddRange(action, action.bindings);
            }
            return builder.BuildNormalized();
        }
    }
}
