using System.Collections.Generic;
using System.Linq;
using SensenToolkit.InputRebinding.Data;
using SensenToolkit.InputRebinding.Internal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace SensenToolkit
{
    public class InputRebindingSerializer
    {
        private IInputActionCollection2 _originalActions;
        private IInputActionCollection2 _actions;
        private ActionsAndBindingsDataStructured _originalData;
        private ActionsAndBindingsDataStructured _currentData;
        private HashSet<string> _originalGroups;

        public InputRebindingSerializer(
            IInputActionCollection2 actions,
            IInputActionCollection2 originalActions
        )
        {
            _actions = actions;
            _originalActions = originalActions;

            _originalData = new ActionsAndBindingsDataStructured(_originalActions);
            _originalGroups = BuildOriginalGroupsSet();
        }

        public string Serialize(bool pretty = false)
        {
            RecreateCurrentData();

            List<BindingPlus> deletionsList = new();
            foreach (BindingPlus original in _originalData.AllBindings)
            {
                bool isOnCurrentActions = _currentData.AllBindings.Keys.Contains(original.Key);
                if (!isOnCurrentActions) deletionsList.Add(original);
            }
            var deletions = BindingPlusCollection.Build(deletionsList);

            List<BindingPlus> additionsList = new();
            foreach (BindingPlus current in _currentData.AllBindings)
            {
                bool isOnOriginalActions = _originalData.AllBindings.Keys.Contains(current.Key);
                if (!isOnOriginalActions) additionsList.Add(current);
            }
            var additions = BindingPlusCollection.Build(additionsList);

            InputRebindingJsonData jsonData = new()
            {
                ActionMaps = _originalData.ActionMaps.Select(m => InputActionMapJson.From(m)).ToList(),
                Actions = _originalActions.Select(a => InputActionJson.From(a)).ToList(),
                BindingDeletions = deletions
                    .Flattened()
                    .Select(b => InputBindingJson.From(b.Binding, b.Action.id))
                    .ToList(),
                BindingAdditions = additions
                    .Flattened()
                    .Select(b => InputBindingJson.From(b.Binding, b.Action.id))
                    .ToList(),
            };

            return JsonUtility.ToJson(jsonData, pretty);
        }

        public bool LoadSerializedJson(string serializedJson)
        {
            RecreateCurrentData();
            InputRebindingDataStructured data = ParseJsonAndStructureData(serializedJson);
            if (data == null) return false;

            foreach (InputAction action in _actions)
            {
                string actionKey = InputUtils.GenerateActionKey(action);
                InputAction originalAction = _originalActions.FindAction(actionKey);
                if (originalAction == null)
                {
                    Debug.LogWarning($"Original action not found '{action.name}'");
                    continue;
                }

                List<BindingPlus> newBindings = new();
                newBindings.AddRange(SelectOriginalBindingsToReAdd(actionKey, data));
                newBindings.AddRange(SelectCustomBindingsToAdd(actionKey, data));
                BindingPlusCollection
                    .Build(newBindings)
                    .ReplaceActionBindings(action);
            }

            return true;
        }

        private List<BindingPlus> SelectOriginalBindingsToReAdd(string actionKey, InputRebindingDataStructured data)
        {
            HashSet<string> deletedBindingKeys = data.DeletedBindingsByActionKey.GetValueOrDefault(actionKey, null);
            IReadOnlyList<BindingPlus> originalBindings = _originalData.BindingsByActionKey.GetValueOrDefault(actionKey, null);

            List<BindingPlus> originalBindingsToReAdd = new();
            if (originalBindings == null) return originalBindingsToReAdd;

            for (int i = 0; i < originalBindings.Count; i++)
            {
                BindingPlus binding = originalBindings[i];
                InputBinding rawBinding = binding.Binding;
                bool isDeleted = false;
                if (rawBinding.isComposite)
                {
                    int nextInx = i + 1;
                    BindingPlus nextBinding = nextInx < originalBindings.Count ? originalBindings[nextInx] : null;
                    isDeleted = deletedBindingKeys?.Contains(binding.Key) == true
                        || nextBinding == null
                        || deletedBindingKeys?.Contains(nextBinding.Key) == true;
                }
                else
                {
                    isDeleted = deletedBindingKeys?.Contains(binding.Key) == true;
                }

                if (isDeleted) continue;

                originalBindingsToReAdd.Add(binding);
            }

            return originalBindingsToReAdd;
        }

        private List<BindingPlus> SelectCustomBindingsToAdd(string actionKey, InputRebindingDataStructured data)
        {
            List<BindingPlus> customBindings = new();
            BindingPlusCollection addedBindings = data
                .AdditionBindingsByActionKey
                .GetValueOrDefault(actionKey, null);

            if (addedBindings == null) return customBindings;
            foreach (BindingPlus binding in addedBindings)
            {
                IReadOnlyList<string> groups = binding.Groups;

                // Ensure the first group is one of the original groups
                if (!binding.IsComposite && groups.Count > 0 && !_originalGroups.Contains(groups[0]))
                {
                    string mostSimilarGroup = StringUtils.FindMostSimilar(_originalGroups, groups[0]);
                    groups = new List<string> { mostSimilarGroup }.Concat(groups.Skip(1)).ToList();
                }

                InputBinding newRawBinding = binding.Binding;
                newRawBinding.groups = InputBindingGroups.Join(groups);
                BindingPlus newBinding = binding.CloneWithNewBinding(newRawBinding);
                customBindings.Add(newBinding);
            }

            return customBindings;
        }

        private HashSet<string> BuildOriginalGroupsSet()
        {
            HashSet<string> originalGroups = new();
            foreach (InputAction action in _originalActions)
            {
                foreach (InputBinding b in action.bindings)
                {
                    if (string.IsNullOrEmpty(b.groups)) continue;
                    string[] groups = InputBindingGroups.Split(b.groups).ToArray();
                    foreach (string g in groups)
                    {
                        string g1 = g.Trim();
                        if (string.IsNullOrEmpty(g1)) continue;
                        originalGroups.Add(g1);
                    }
                }
            }

            return originalGroups;
        }
        private InputRebindingDataStructured ParseJsonAndStructureData(string serializedJson)
        {
            InputRebindingJsonData jsonData = JsonUtility.FromJson<InputRebindingJsonData>(serializedJson);
            if (jsonData == null || jsonData.IsEmpty) return null;

            return InputRebindingDataStructured.BuildFromJsonData(jsonData);
        }

        private void RecreateCurrentData()
        {
            _currentData = new ActionsAndBindingsDataStructured(_actions);
        }
    }
}
