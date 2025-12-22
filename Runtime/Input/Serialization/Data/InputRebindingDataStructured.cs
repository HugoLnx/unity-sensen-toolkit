using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace SensenToolkit.InputRebindingSerialization
{
    internal class InputRebindingDataStructured
    {
        public readonly IReadOnlyList<InputActionMap> ActionMaps;
        public readonly IReadOnlyList<InputActionPlus> Actions;
        public readonly BindingPlusCollection BindingDeletions;
        public readonly BindingPlusCollection BindingAdditions;
        public IReadOnlyDictionary<string, HashSet<string>> DeletedBindingsByActionKey { get; private set; }
        public IReadOnlyDictionary<string, BindingPlusCollection> AdditionBindingsByActionKey { get; private set; }

        public InputRebindingDataStructured(
            IReadOnlyList<InputActionMap> actionMaps,
            IReadOnlyList<InputActionPlus> actions,
            BindingPlusCollection bindingDeletions,
            BindingPlusCollection bindingAdditions
        )
        {
            ActionMaps = actionMaps;
            Actions = actions;
            BindingDeletions = bindingDeletions;
            BindingAdditions = bindingAdditions;
            EnsureComplete();
        }

        public static InputRebindingDataStructured BuildFromJsonData(InputRebindingJsonData jsonData)
        {
            List<InputActionMap> actionMaps = jsonData
                .ActionMaps.ConvertAll(m => m.ActionMap);
            Dictionary<Guid, InputActionMap> actionMapById = new();
            foreach (InputActionMap actionMap in actionMaps)
            {
                actionMapById[actionMap.id] = actionMap;
            }

            List<InputActionPlus> actions = new();
            Dictionary<Guid, InputActionPlus> actionById = new();
            foreach (InputActionJson actionJson in jsonData.Actions)
            {
                Guid? actionMapId = actionJson.ActionMapId;
                if (!actionMapId.HasValue) continue;

                actionMapById.TryGetValue(actionMapId.Value, out InputActionMap actionMap);
                var action = InputActionPlus.FromInputAction(actionJson.Action, actionMap);
                actions.Add(action);
                actionById[action.Action.id] = action;
            }

            BindingPlusCollectionBuilder collectionBuilder = new();
            foreach (InputBindingJson bindingJson in jsonData.BindingDeletions)
            {
                Guid? actionId = bindingJson.ActionId;
                if (!actionId.HasValue) continue;
                actionById.TryGetValue(actionId.Value, out InputActionPlus action);
                collectionBuilder.Add(action, bindingJson.Binding);
            }
            BindingPlusCollection bindingDeletions = collectionBuilder.Build();

            collectionBuilder.Reset();
            foreach (InputBindingJson bindingJson in jsonData.BindingAdditions)
            {
                Guid? actionId = bindingJson.ActionId;
                if (!actionId.HasValue) continue;
                actionById.TryGetValue(actionId.Value, out InputActionPlus action);
                collectionBuilder.Add(action, bindingJson.Binding);
            }
            BindingPlusCollection bindingAdditions = collectionBuilder.Build();

            return new InputRebindingDataStructured(
                actionMaps,
                actions,
                bindingDeletions,
                bindingAdditions
            );
        }

        private void EnsureComplete()
        {
            Dictionary<string, HashSet<string>> deletedBindingsByActionKey = new();
            foreach (BindingPlus binding in BindingDeletions)
            {
                string actionKey = binding.ActionKey;
                HashSet<string> deletionKeys = deletedBindingsByActionKey.GetValueOrDefault(actionKey, new());
                deletionKeys.Add(binding.Key);
                deletedBindingsByActionKey[actionKey] = deletionKeys;
            }
            DeletedBindingsByActionKey = deletedBindingsByActionKey;

            Dictionary<string, List<BindingPlus>> additionsListByActionKey = new();
            foreach (BindingPlus binding in BindingAdditions)
            {
                string actionKey = binding.ActionKey;
                List<BindingPlus> additions = additionsListByActionKey.GetValueOrDefault(actionKey, new());
                additions.Add(binding);
                additionsListByActionKey[actionKey] = additions;
            }

            var additionBindingsByActionKey = new Dictionary<string, BindingPlusCollection>();
            foreach ((string actionKey, List<BindingPlus> additions) in additionsListByActionKey)
            {
                additionBindingsByActionKey[actionKey] = BindingPlusCollection.Build(additions);
            }

            AdditionBindingsByActionKey = additionBindingsByActionKey;
        }
    }
}
