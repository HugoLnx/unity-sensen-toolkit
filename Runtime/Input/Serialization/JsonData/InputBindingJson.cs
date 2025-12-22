using UnityEngine.InputSystem;
using SensenToolkit;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System;
using System.Collections.Generic;

namespace SensenToolkit.InputRebindingSerialization
{
    [System.Serializable]
    internal class InputBindingJson
    {
        [SerializeField] public string ActionIdRaw;
        [SerializeField] public string Name;
        [SerializeField] public string Id;
        [SerializeField] public string Path;
        [SerializeField] public string Interactions;
        [SerializeField] public string Processors;
        [SerializeField] public List<string> Groups;
        [SerializeField] public string OverridePath;
        [SerializeField] public string OverrideInteractions;
        [SerializeField] public string OverrideProcessors;
        [SerializeField] public bool IsComposite;
        [SerializeField] public bool IsPartOfComposite;

        private System.Guid? _actionId;
        public System.Guid ActionId => _actionId ??= string.IsNullOrEmpty(ActionIdRaw)
            ? Guid.Empty
            : Guid.Parse(ActionIdRaw);
        public InputBinding Binding => _binding ??= ToInputBinding();

        private InputBinding? _binding;

        public static InputBindingJson From(InputBinding binding, System.Guid? actionId)
        {
            return new InputBindingJson
            {
                ActionIdRaw = actionId == null || actionId == Guid.Empty ? null : actionId.Value.ToString(),
                Name = binding.name,
                Id = binding.id.ToString(),
                Path = binding.path,
                Interactions = binding.interactions,
                Processors = binding.processors,
                OverridePath = binding.overridePath,
                OverrideInteractions = binding.overrideInteractions,
                OverrideProcessors = binding.overrideProcessors,
                Groups = BindingGroups.Split(binding.groups).ToList(),
                IsComposite = binding.isComposite,
                IsPartOfComposite = binding.isPartOfComposite,
            };
        }

        public InputBinding ToInputBinding()
        {
            InputBinding binding = new()
            {
                name = Name,
                id = Guid.Parse(Id),
                path = Path,
                interactions = Interactions,
                processors = Processors,
                overridePath = String.IsNullOrEmpty(OverridePath) ? null : OverridePath,
                overrideInteractions = String.IsNullOrEmpty(OverrideInteractions) ? null : OverrideInteractions,
                overrideProcessors = String.IsNullOrEmpty(OverrideProcessors) ? null : OverrideProcessors,
                groups = BindingGroups.Join(Groups),
                isComposite = IsComposite,
                isPartOfComposite = IsPartOfComposite,
            };
            return binding;
        }
    }
}
