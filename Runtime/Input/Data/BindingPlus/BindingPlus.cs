using System;
using System.Collections.Generic;
using System.Linq;
using SensenToolkit.InputRebinding.Internal;
using UnityEngine.InputSystem;

namespace SensenToolkit.InputRebinding.Data
{
    public class BindingPlus
    {
        public const string DEVICE_SHORTNAME_PREFIX = "[DVCE_NAME]";
        public const string DEVICE_ID_PREFIX = "[DVCE_ID]";
        public const string CUSTOM_BINDING_GROUP = "[CUSTOM_BINDING]";
        public InputActionMap ActionMap { get; private set; }
        public InputAction Action { get; private set; }
        public InputBinding Binding { get; private set; }
        private InputBindingPath _path;
        public InputBindingPath Path => _path ??= InputBindingPath.FromFullPath(Binding.path);
        public bool IsComposite => Binding.isComposite;
        public bool IsPartOfComposite => Binding.isPartOfComposite;
        public IReadOnlyList<BindingPlus> CompositeChildren => _mutableCompositeChildren;
        private List<BindingPlus> _mutableCompositeChildren = new();
        public BindingPlus ParentComposite { get; private set; }
        private string _bindingKey = null;
        public string Key => EnforceBindingKey();
        private string _actionKey = null;
        public string ActionKey => EnforceActionKey();

        private string _equivalenceKey = null;
        public string EquivalenceKey => EnforceEquivalenceKey();

        private string _compositePartComparisonKey = null;
        public string CompositePartComparisonKey => _compositePartComparisonKey ??= EnforceCompositePartComparisonKey();

        private List<string> _groups;
        public IReadOnlyList<string> Groups => _groups ??= InputBindingGroups
            .Split(Binding.groups)
            .ToList();

        private HashSet<string> _groupsSet;
        public HashSet<string> GroupsSet => _groupsSet ??= new HashSet<string>(Groups);

        // TODO: Use keyboardAndMouse group set in the service through the editor
        private bool? _isKeyboardAndMouse;
        public bool IsKeyboardAndMouse => _isKeyboardAndMouse ??= GroupsSet
            .Contains(InputToolkitService.DEFAULT_KEYBOARD_AND_MOUSE_GROUP);

        public bool IsCustomBinding => IsComposite
            ? CompositeChildren.Any(child => child.IsCustomBinding)
            : GroupsSet.Contains(BindingPlus.CUSTOM_BINDING_GROUP);
        public bool IsDefaultBinding => !IsCustomBinding;

        private bool _resolvedDeviceNameGroup;
        private string _deviceNameGroup;
        private string DeviceNameGroup => ResolveDeviceNameGroup();

        private bool _resolvedDeviceIdGroup;
        private string _deviceIdGroup;
        private string DeviceIdGroup => ResolveDeviceIdGroup();

        private string _customDeviceId;
        private string CustomDeviceId => ResolveDeviceId();

        public string DeviceId => IsKnownStandardDevice ? Path.Device : CustomDeviceId;

        private string _customDeviceShortName;
        public string CustomDeviceShortName => ResolveDeviceShortName();

        public bool IsCustomDevice => !string.IsNullOrEmpty(DeviceNameGroup) && !string.IsNullOrEmpty(DeviceIdGroup);
        public bool IsKnownStandardDevice => !IsCustomDevice;

        private string _displayString;
        public string DisplayString => _displayString ??= BindingDisplayNameGenerator.GenerateDisplayNameFor(this);

        private string _displayStringShortenedForComposite;
        public string DisplayStringShortenedForComposite => _displayStringShortenedForComposite
            ??= BindingDisplayNameGenerator.GenerateDisplayNameFor(this, shortenForComposite: true);

        public static BindingPlus Build(
            InputActionPlus action,
            InputBinding binding,
            IReadOnlyCollection<BindingPlus> compositeChildren = null)
        {
            return Build(action.Action, binding, action.ActionMap, compositeChildren);
        }

        public static BindingPlus Build(InputAction action, InputBinding binding, InputActionMap actionMapOverride = null, IReadOnlyCollection<BindingPlus> compositeChildren = null)
        {
            bool hasChildren = compositeChildren != null && compositeChildren.Count > 0;
            Assertx.IsFalse(binding.isComposite && !hasChildren, "Composite binding must have composite children.");

            InputActionMap actionMap = actionMapOverride ?? action.actionMap;
            BindingPlus bindingPlus = new()
            {
                ActionMap = actionMap,
                Action = action,
                Binding = binding,
            };

            if (hasChildren)
            {
                bindingPlus.SetPath(compositeChildren.ElementAt(0).Path.Clone());
                foreach (BindingPlus child in compositeChildren)
                {
                    child.ParentComposite = bindingPlus;
                    if (!bindingPlus.Path.MatchesControl(child.Path.Control))
                    {
                        bindingPlus.Path.SetControlPart(null);
                        bindingPlus.Path.SetControl(null);
                    }
                }
                bindingPlus._mutableCompositeChildren.AddRange(compositeChildren);
                bindingPlus._mutableCompositeChildren.Sort((a, b) => string.Compare(
                    a.CompositePartComparisonKey,
                    b.CompositePartComparisonKey,
                    StringComparison.Ordinal));
            }

            return bindingPlus;
        }

        private void SetPath(InputBindingPath path)
        {
            _path = path;
        }

        private string EnforceBindingKey()
        {
            if (_bindingKey != null) return _bindingKey;
            _bindingKey = Binding.isComposite
                ? BindingKeyGenerator.GenerateCompositeBindingKey(
                    Action,
                    Binding,
                    CompositeChildren.Select(plus => plus.Binding),
                    ActionMap
                )
                : BindingKeyGenerator.GenerateSingleBindingKey(
                    Action,
                    Binding,
                    ActionMap
                );

            return _bindingKey;
        }

        private string EnforceEquivalenceKey()
        {
            if (_equivalenceKey != null) return _equivalenceKey;
            _equivalenceKey = Binding.isComposite
                ? BindingEquivalenceKeyGenerator.GenerateCompositeBindingEquivalenceKey(
                    Action,
                    CompositeChildren.Select(plus => plus.Binding),
                    ActionMap
                )
                : BindingEquivalenceKeyGenerator.GenerateSingleBindingEquivalenceKey(
                    Action,
                    Binding,
                    ActionMap
                );

            return _equivalenceKey;
        }

        private string EnforceCompositePartComparisonKey()
        {
            if (_compositePartComparisonKey != null) return _compositePartComparisonKey;
            _compositePartComparisonKey = GenerateComparisonKeyCompositePartBinding(this.Binding);
            return _compositePartComparisonKey;
        }

        public static string GenerateComparisonKeyCompositePartBinding(InputBinding b)
        {
            int score = InputConstants.CompositeNamesOrderIndexes.GetValueOrDefault(b.name, 99);
            return $"{score:D2}{b.name}";
        }

        public IEnumerable<BindingPlus> EnumerateBindingsWithPath()
        {
            if (IsComposite)
            {
                foreach (BindingPlus part in CompositeChildren)
                {
                    yield return part;
                }
            }
            else
            {
                yield return this;
            }
        }

        private string EnforceActionKey()
        {
            if (_actionKey != null) return _actionKey;
            _actionKey = InputUtils.GenerateActionKey(Action, ActionMap);
            return _actionKey;
        }

        private string ResolveDeviceNameGroup()
        {
            if (_resolvedDeviceNameGroup) return _deviceNameGroup;

            string group = Groups.FirstOrDefault(g => g.StartsWith(BindingPlus.DEVICE_SHORTNAME_PREFIX, StringComparison.OrdinalIgnoreCase));
            _deviceNameGroup = group;
            _resolvedDeviceNameGroup = true;
            return _deviceNameGroup;
        }

        private string ResolveDeviceIdGroup()
        {
            if (_resolvedDeviceIdGroup) return _deviceIdGroup;
            string group = Groups.FirstOrDefault(g => g.StartsWith(BindingPlus.DEVICE_ID_PREFIX, StringComparison.OrdinalIgnoreCase));;
            _resolvedDeviceIdGroup = true;
            _deviceIdGroup = group;
            return _deviceIdGroup;
        }

        private string ResolveDeviceShortName()
        {
            if (_customDeviceShortName != null) return _customDeviceShortName;
            string group = ResolveDeviceNameGroup();
            if (group == null) return null;
            _customDeviceShortName = group[BindingPlus.DEVICE_SHORTNAME_PREFIX.Length..];
            return _customDeviceShortName;
        }

        private string ResolveDeviceId()
        {
            if (_customDeviceId != null) return _customDeviceId;
            string group = ResolveDeviceIdGroup();
            if (group == null) return null;
            _customDeviceId = group[BindingPlus.DEVICE_ID_PREFIX.Length..];
            return _customDeviceId;
        }

        public BindingPlus CloneWithNewBinding(InputBinding binding)
        {
            return Build(Action, binding, ActionMap, CompositeChildren);
        }
    }
}
