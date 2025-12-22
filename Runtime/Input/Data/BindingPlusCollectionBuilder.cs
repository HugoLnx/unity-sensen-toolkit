using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace SensenToolkit.InputRebinding.Data
{
    public struct BindingPlusBuildData
    {
        public InputActionMap ActionMap;
        public InputAction Action;
        public InputBinding Binding;
    }

    public class BindingPlusCollectionBuilder
    {
        List<BindingPlusBuildData> _bindings = new();

        public static BindingPlusCollection Build(InputAction action, InputActionMap overrideMap = null)
        {
            return new BindingPlusCollectionBuilder()
                .AddRange(action, action.bindings, overrideMap)
                .Build();
        }

        public BindingPlusCollectionBuilder Reset()
        {
            _bindings.Clear();
            return this;
        }

        public BindingPlusCollectionBuilder Add(InputActionPlus action, InputBinding binding)
            => Add(action.Action, binding, action.ActionMap);

        public BindingPlusCollectionBuilder AddRange(InputActionPlus action, IEnumerable<InputBinding> bindings)
            => AddRange(action.Action, bindings, action.ActionMap);

        public BindingPlusCollectionBuilder AddRange(
            InputAction action,
            IEnumerable<InputBinding> bindings,
            InputActionMap actionMapOverride = null
        )
        {
            foreach (InputBinding binding in bindings)
            {
                Add(action, binding, actionMapOverride);
            }
            return this;
        }

        public BindingPlusCollectionBuilder Add(
            InputAction action,
            InputBinding binding,
            InputActionMap actionMapOverride = null
        )
        {
            _bindings.Add(new BindingPlusBuildData
            {
                ActionMap = actionMapOverride ?? action.actionMap,
                Action = action,
                Binding = binding,
            });
            return this;
        }

        public BindingPlusCollection Build()
            => BindingPlusCollection.Build(BuildBindingList());

        public BindingPlusCollection BuildNormalized()
            => BindingPlusCollection.BuildNormalized(BuildBindingList());

        private List<BindingPlus> BuildBindingList()
        {
            List<BindingPlus> bindingsList = new();
            List<BindingPlus> tmpCompositeParts = new();

            for (int i = 0; i < _bindings.Count; i++)
            {
                BindingPlusBuildData data = _bindings[i];

                if (data.Binding.isComposite)
                {
                    string compositeActionKey = InputUtils.GenerateActionKey(data.Action, data.ActionMap);
                    tmpCompositeParts.Clear();
                    i += 1;
                    while (i < _bindings.Count)
                    {
                        BindingPlusBuildData partData = _bindings[i];
                        if (!partData.Binding.isPartOfComposite) break;

                        string partActionKey = InputUtils.GenerateActionKey(partData.Action, partData.ActionMap);
                        if (partActionKey != compositeActionKey) break;

                        tmpCompositeParts.Add(BindingPlus.Build(
                            partData.Action,
                            partData.Binding,
                            partData.ActionMap
                        ));
                        i += 1;
                    }

                    bindingsList.Add(BindingPlus.Build(
                        data.Action,
                        data.Binding,
                        data.ActionMap,
                        tmpCompositeParts
                    ));

                    i -= 1; // Adjust for outer loop increment
                    continue;
                }

                bindingsList.Add(BindingPlus.Build(
                    data.Action,
                    data.Binding,
                    data.ActionMap
                ));
            }

            return bindingsList;
        }
    }
}
