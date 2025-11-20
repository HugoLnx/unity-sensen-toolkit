using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SensenToolkit
{
    public class BindingMetadata
    {
        public InputBinding Binding;
        public string PathDeviceName;
        public string PathSubControlName;
        public string PathControlName;
        public bool IsKeyboardAndMouse;
        public bool IsKnownDevice;
        public string DeviceIdGroup;
        public string DeviceShortName;
        public int OrderIndex;
        public bool IsDefaultBinding;
        public bool IsComposite;
        public List<BindingMetadata> CompositeParts;
        public BindingMetadata ParentComposite;

        private string _displayString;
        public string DisplayString => _displayString ??= BindingDisplayStringUtils.GenerateDisplayStringFor(this);
        private string _displayStringShortenedForComposite;
        public string DisplayStringShortenedForComposite => _displayStringShortenedForComposite
            ??= BindingDisplayStringUtils.GenerateDisplayStringFor(this, shortenForComposite: true);

        public IEnumerable<BindingMetadata> EnumerateAllBindings()
        {
            yield return this;
            if (IsComposite && CompositeParts != null)
            {
                foreach (BindingMetadata part in CompositeParts)
                {
                    yield return part;
                }
            }
        }
    }

    public class BindingMetadataProcessor
    {
        private const string DEVICE_SHORTNAME_PREFIX = RebindingMetadataProcessor.DEVICE_SHORTNAME_PREFIX;
        private readonly InputToolkitService _inputToolkit;
        private readonly InputAction _originalAction;
        private HashSet<string> _defaultBindingPaths;
        private HashSet<string> DefaultBindingPaths => _defaultBindingPaths
            ??= new(_originalAction.bindings.Select(b => b.effectivePath));

        public BindingMetadataProcessor(
            InputToolkitService inputToolkit,
            InputAction originalAction
        )
        {
            _inputToolkit = inputToolkit;
            _originalAction = originalAction;
        }

        public IEnumerable<BindingMetadata> ProcessAllBindings(IReadOnlyList<InputBinding> bindings)
        {
            for (int i = 0; i < bindings.Count; i++)
            {
                InputBinding binding = bindings[i];
                if (binding.isPartOfComposite)
                {
                    Debug.LogError($"[KeyRebindingSetting:{_originalAction.name}] Skipping composite part binding: {binding.ToDisplayString()}");
                    continue;
                }
                if (binding.isComposite)
                {
                    List<BindingMetadata> compositeParts = new();
                    int compositeOrderIndex = i;
                    i++;
                    while (i < bindings.Count && bindings[i].isPartOfComposite)
                    {
                        BindingMetadata bindingPart = ProcessSingleBinding(bindings[i], i);
                        compositeParts.Add(bindingPart);
                        i++;
                    }
                    i--;

                    var compositeBinding = new BindingMetadata
                    {
                        Binding = binding,
                        IsComposite = true,
                        CompositeParts = compositeParts,
                        DeviceIdGroup = compositeParts[0].DeviceIdGroup,
                        DeviceShortName = compositeParts[0].DeviceShortName,
                        IsDefaultBinding = compositeParts[0].IsDefaultBinding,
                        IsKeyboardAndMouse = compositeParts[0].IsKeyboardAndMouse,
                        IsKnownDevice = compositeParts[0].IsKnownDevice,
                        PathDeviceName = compositeParts[0].PathDeviceName,
                        PathControlName = null,
                        PathSubControlName = compositeParts[0].PathSubControlName,
                        OrderIndex = compositeOrderIndex
                    };

                    foreach (BindingMetadata part in compositeParts)
                    {
                        part.ParentComposite = compositeBinding;
                        if (part.PathSubControlName != compositeBinding.PathSubControlName)
                        {
                            compositeBinding.PathSubControlName = null;
                            break;
                        }
                    }

                    yield return compositeBinding;
                }
                else
                {
                    yield return ProcessSingleBinding(binding, i);
                }
            }
        }

        public BindingMetadata ProcessSingleBinding(InputBinding binding, int orderIndex = -1)
        {
            HashSet<string> groups = new((binding.groups ?? "").Split(';', StringSplitOptions.RemoveEmptyEntries));
            bool isKeyboardAndMouse = groups.Contains(_inputToolkit.BindingGroupKeyboardAndMouse);
            groups.ExceptWith(_inputToolkit.BindingGroups);
            bool isKnownDevice = groups.Count == 0;
            string deviceNameGroup = isKnownDevice
                ? null
                : groups.FirstOrDefault(g => g.StartsWith(DEVICE_SHORTNAME_PREFIX, StringComparison.OrdinalIgnoreCase));
            string deviceShortName = null;
            if (deviceNameGroup != null)
            {
                groups.Remove(deviceNameGroup);
                deviceShortName = deviceNameGroup[DEVICE_SHORTNAME_PREFIX.Length..];
            }

            string deviceIdGroup = isKnownDevice ? null : groups.First();

            bool isDefaultBinding = DefaultBindingPaths.Contains(binding.effectivePath);

            string path = binding.effectivePath;
            string[] pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            return new BindingMetadata
            {
                Binding = binding,
                IsKeyboardAndMouse = isKeyboardAndMouse,
                IsKnownDevice = isKnownDevice,
                DeviceIdGroup = deviceIdGroup,
                DeviceShortName = deviceShortName,
                OrderIndex = orderIndex,
                IsDefaultBinding = isDefaultBinding,
                PathDeviceName = pathParts[0],
                PathSubControlName = pathParts.Length >= 3 ? string.Join('/', pathParts[1..^1]) : null,
                PathControlName = pathParts.Length >= 2 ? pathParts[^1] : null
            };
        }

        public static bool IsKnownStandardGamepadPath(string path)
        {
            string newPathDevicePrefix = ExtractPathDevicePrefix(path);
            bool isGamepadPath = newPathDevicePrefix.Equals("<gamepad>", StringComparison.OrdinalIgnoreCase);
            if (isGamepadPath) return true;

            bool isJoystickPath = newPathDevicePrefix.Equals("<joystick>", StringComparison.OrdinalIgnoreCase);
            // If is not joystick nor gamepad path, then it's not known gamepad
            if (!isJoystickPath) return false;

            // <Joystick>/Trigger has different trigger button on different joystick models
            bool isStandardizedJoystickPath = !path.Contains("trigger", StringComparison.OrdinalIgnoreCase);
            return isStandardizedJoystickPath;
        }

        private static string ExtractPathDevicePrefix(string path)
        {
            int slashIndex = path.IndexOf('/');
            if (slashIndex < 0) return path;
            return path[..slashIndex];
        }
    }
}
