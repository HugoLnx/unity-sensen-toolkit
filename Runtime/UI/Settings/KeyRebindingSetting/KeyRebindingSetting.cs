using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EasyButtons;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputActionRebindingExtensions;

namespace SensenToolkit
{
    public class KeyRebindingSetting : MonoBehaviour
    {
        private const string DEVICE_SHORTNAME_PREFIX = "[DEVICE]";
        private static readonly Regex s_blankRegex = new(@"\s+", RegexOptions.Compiled);
        private static readonly Regex s_versionRegex = new(@"\d[\.,\d_-]+", RegexOptions.Compiled);
        private static readonly Regex s_specialCharsRegex = new(@"[^\d\w]", RegexOptions.Compiled);
        private static readonly Regex s_firstWordRegex = new(@"[^a-zA-Z]*([A-Z][A-Z]+|[A-Z][a-z]+|[a-z]+)", RegexOptions.Compiled);

        [SerializeField]
        private List<string> _blockedDeletions = new(){
            "<Keyboard>/escape",
            "<Keyboard>/upArrow",
            "<Keyboard>/downArrow",
            "<Keyboard>/leftArrow",
            "<Keyboard>/rightArrow",
            "<Mouse>/delta",
            "<Mouse>/position",
            "<Pointer>/delta",
            "<Pointer>/position",
        };
        [SerializeField] private Color _performingColor = Color.purple;
        [SerializeField] private Color _hoveredColor = Color.red;
        [SerializeField] private Color _defaultBindingColor = Color.cyan;
        [SerializeField] private Color _customBindingColor = Color.green;
        [SerializeField] private DynamicInputActionReference _actionReference;
        [SerializeField] private bool _cancelThroughEscape = true;
        [SerializeField] private bool _ignoreMouseDelta = true;
        [SerializeField] private TMP_Text _keyText;
        [SerializeField] private Button _addButton;
        [SerializeField] private Button _addDefaultsButton;
        [SerializeField, AutoProperty(AutoPropertyMode.Children)]
        private InteractiveTextLinks _interactiveTextLinks;
        [SerializeField, AutoProperty]
        private PanelChildVisibilityEvents _visibility;

        [SerializeField, AutoProperty(AutoPropertyMode.Scene, allowEmpty: true)]
        private InputToolkitService _inputToolkit;

        private bool _performingAction;
        private Color _originalTextColor;
        private InputAction _testAction = null;
        private HashSet<string> _hoveredBindings = new();
        private InputAction _originalAction;
        private HashSet<string> _defaultBindingPaths = new();
        private HashSet<string> _blockedDeletionsSet;
        private HashSet<string> BlockedDeletionsSet => _blockedDeletionsSet ??= new(_blockedDeletions);

        public bool IsVisible => _visibility.IsVisible;

        private void Awake()
        {
            _originalAction = _actionReference.OriginalActionClone();
            foreach (InputBinding binding in _originalAction.bindings)
            {
                _defaultBindingPaths.Add(binding.effectivePath);
            }
            if (_inputToolkit != null)
            {
                _inputToolkit.BindActionCollection(SetActionCollection);
            }
            _visibility.OnShow += OnShow;
            _visibility.OnHidden += OnHidden;
        }

        private void Start()
        {
            _originalTextColor = _keyText.color;
            RecloneTestAction();
            RefreshIfVisible();
        }

        private void OnDestroy()
        {
            if (_inputToolkit != null)
            {
                _inputToolkit.UnbindActionCollection(SetActionCollection);
            }
            _visibility.OnShow -= OnShow;
            _visibility.OnHidden -= OnHidden;
            DisposeTestAction();
        }


        private void OnShow()
        {
            _addButton.onClick.AddListener(OnAddClicked);
            _addDefaultsButton.onClick.AddListener(OnAddDefaultsClicked);
            _interactiveTextLinks.OnLinkHovered += OnLinkHovered;
            _interactiveTextLinks.OnLinkUnhovered += OnLinkUnhovered;
            _interactiveTextLinks.OnLinkClicked += OnLinkClicked;
            EnsureTestActionEnabled();
            RefreshComponents();
        }


        protected void OnHidden()
        {
            _addButton.onClick.RemoveListener(OnAddClicked);
            _addDefaultsButton.onClick.RemoveListener(OnAddDefaultsClicked);
            _interactiveTextLinks.OnLinkHovered -= OnLinkHovered;
            _interactiveTextLinks.OnLinkUnhovered -= OnLinkUnhovered;
            _interactiveTextLinks.OnLinkClicked -= OnLinkClicked;
            _actionReference.EnsureUnbinded();
            EnsureTestActionDisabled();
        }

        public void SetActionCollection(IInputActionCollection2 actions)
        {
            _actionReference.SetActionCollection(actions);
            RecloneTestAction();
        }

        private void RefreshIfVisible()
        {
            if (!IsVisible) return;
            RefreshComponents();
        }


        [Button]
        private void RefreshComponents()
        {
            _keyText.text = GetBindingDisplayString(_actionReference.Action);
            // _keyText.color = _performingAction ? _performingColor : _originalTextColor;
        }

        private string GetBindingDisplayString(InputAction action)
        {
            if (action == null || action.bindings.Count == 0) return "Unbound";

            IEnumerable<ClassifiedBinding> classifiedBindings = CreateClassifiedBindings(action.bindings);

            List<string> displayStrings = new();
            foreach (ClassifiedBinding cb in classifiedBindings)
            {
                string str = cb.DisplayString;
                string bindingId = cb.Binding.id.ToString();
                bool isPerforming = _performingAction
                    && _testAction != null
                    && _testAction.activeControl != null
                    && cb.EnumerateAllBindings().Any((cb) => InputControlPath.Matches(cb.Binding.effectivePath, _testAction.activeControl));
                str = $"<link=\"{bindingId}\">{str}</link>";
                bool isHovered = _hoveredBindings.Contains(bindingId);
                bool canBeDeleted = CanBindingBeDeleted(cb);
                bool isInteracting = isPerforming || isHovered;
                if (isInteracting)
                {
                    if (isPerforming)
                    {
                        str = $"<color={_performingColor.ToHex()}><i>{str}</i></color>";
                    }
                    if (isHovered && canBeDeleted)
                    {
                        str = $"<s><i><u>{str}</u></i></s>";
                        if (!isPerforming) str = $"<color={_hoveredColor.ToHex()}>{str}</color>";
                    }
                    else if (isHovered)
                    {
                        str = $"<i>{str}</i>";
                    }
                }
                else if (cb.IsDefaultBinding)
                {
                    str = $"<color={_defaultBindingColor.ToHex()}>{str}</color>";
                }
                else
                {
                    str = $"<color={_customBindingColor.ToHex()}>{str}</color>";
                }

                displayStrings.Add(str);
            }
            return string.Join(" | ", displayStrings);
        }

        private IEnumerable<ClassifiedBinding> CreateClassifiedBindings(IReadOnlyList<InputBinding> bindings)
        {
            for (int i = 0; i < bindings.Count; i++)
            {
                InputBinding binding = bindings[i];
                if (binding.isPartOfComposite)
                {
                    Debug.LogError($"[KeyRebindingSetting:{_actionReference.Action.name}] Skipping composite part binding: {binding.ToDisplayString()}");
                    continue;
                }
                if (binding.isComposite)
                {
                    List<ClassifiedBinding> compositeParts = new();
                    int compositeOrderIndex = i;
                    i++;
                    while (i < bindings.Count && bindings[i].isPartOfComposite)
                    {
                        ClassifiedBinding bindingPart = ClassifyBinding(bindings[i], i);
                        compositeParts.Add(bindingPart);
                        i++;
                    }
                    i--;

                    var compositeBinding = new ClassifiedBinding
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

                    foreach (ClassifiedBinding part in compositeParts)
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
                    yield return ClassifyBinding(binding, i);
                }
            }
        }

        private ClassifiedBinding ClassifyBinding(InputBinding binding, int orderIndex)
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

            bool isDefaultBinding = _defaultBindingPaths.Contains(binding.effectivePath);

            string path = binding.effectivePath;
            string[] pathParts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            return new ClassifiedBinding
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

        private void OnAddClicked()
        {
            Debug.Log($"[Bind:{_actionReference.Action.name}] Start");
            InputAction action = _actionReference.Action;
            bool wasEnabled = action.enabled;
            action.Disable();
            RebindingOperation op = action.PerformInteractiveRebinding()
            .WithTimeout(10f);
            if (_cancelThroughEscape)
            {
                op = op
                .WithCancelingThrough("<Keyboard>/escape")
                .WithControlsExcluding("<Keyboard>/escape");
            }
            if (_ignoreMouseDelta)
            {
                op = op
                .WithControlsExcluding("<Pointer>/delta")
                .WithControlsExcluding("<Pointer>/position")
                .WithControlsExcluding("<Mouse>/delta")
                .WithControlsExcluding("<Mouse>/position");
            }

            op = op
            .OnApplyBinding((operation, newPath) =>
            {
                InputDevice device = operation.selectedControl.device;
                string[] newPathParts = newPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (device is Joystick && newPathParts?[1].Equals("hat", StringComparison.OrdinalIgnoreCase) == true)
                {
                    newPathParts[0] = "<Joystick>";
                    newPath = string.Join('/', newPathParts);
                }
                bool isKeyboardAndMouse = device is Keyboard || device is Mouse;
                string mainGroup = isKeyboardAndMouse ? _inputToolkit.BindingGroupKeyboardAndMouse : _inputToolkit.BindingGroupGamepad;
                bool isKnownGamepad = IsKnownStandardGamepadPath(newPath);
                bool isKnownDevice = isKeyboardAndMouse || isKnownGamepad;
                string unknownDeviceGroup = isKnownDevice ? null : DeviceToGroupName(device);
                string unknownDeviceShortName = isKnownDevice ? null : DeviceShortName(device);

                InputAction action = _actionReference.Action;
                bool isAlreadyBound = action.controls.Any(control => InputControlPath.Matches(newPath, control));
                if (isAlreadyBound)
                {
                    Debug.Log($"[Bind:{_actionReference.Action.name}] Path {newPath} is already bound, skipping adding new binding.");
                }
                else
                {
                    string groups = isKnownDevice
                        ? mainGroup
                        : $"{mainGroup};{unknownDeviceGroup};{DEVICE_SHORTNAME_PREFIX}{unknownDeviceShortName}";

                    action.AddBinding(new InputBinding
                    {
                        path = newPath,
                        groups = groups
                    });
                }
            })
            .OnComplete(operation =>
            {
                operation.Dispose();
                if (wasEnabled) action.Enable();
                OnRebindComplete();
            })
            .OnCancel(operation =>
            {
                operation.Dispose();
                if (wasEnabled) action.Enable();
                Debug.Log($"[Bind:{_actionReference.Action.name}] Canceled");
            })
            .Start();
        }

        private void OnRebindComplete()
        {
            RecloneTestAction();
            RefreshComponents();
        }

        private void OnAddDefaultsClicked()
        {
            if (_originalAction == null) return;

            ChangeAction((action) =>
            {
                var currentBindings = action.bindings.ToList();
                var safeLoop = new SafeLoop(250);
                while (action.bindings.Count > 0)
                {
                    action.ChangeBinding(0).Erase();
                    safeLoop.Count();
                }
                foreach (InputBinding binding in _originalAction.bindings)
                {
                    action.AddBinding(binding);
                }

                foreach (InputBinding binding in currentBindings)
                {
                    bool isAlreadyBound = action.bindings.Any(b => binding.effectivePath == b.effectivePath);
                    if (isAlreadyBound) continue;
                    action.AddBinding(binding);
                }
            });
        }

        private string DeviceShortName(InputDevice device)
        {
            string manufacturer = (device.description.manufacturer ?? "").Trim();
            string product = (device.description.product ?? "").Trim();

            if (!String.IsNullOrEmpty(manufacturer)) manufacturer = s_blankRegex.Replace(manufacturer, "");
            product = s_blankRegex.Replace(product, " ");

            if (String.IsNullOrEmpty(manufacturer) && !String.IsNullOrEmpty(product))
            {
                Match firstWordMatch = s_firstWordRegex.Match(product);
                if (firstWordMatch.Success
                    && firstWordMatch.Length < (product.Length - 2)
                    && firstWordMatch.Length >= 2)
                {
                    manufacturer = firstWordMatch.Value;
                    product = product
                    .Replace(manufacturer, "", StringComparison.OrdinalIgnoreCase)
                    .Trim();
                }
            }

            product = s_blankRegex.Replace(product, "");
            if (String.IsNullOrEmpty(product)) product = device.name ?? "Unknown";

            product = product
                .Replace("generic", "Gn", StringComparison.OrdinalIgnoreCase)
                .Replace("usb", "U", StringComparison.OrdinalIgnoreCase)
                .Replace("controller", "Ct", StringComparison.OrdinalIgnoreCase)
                .Replace("gamepad", "Gd", StringComparison.OrdinalIgnoreCase)
                .Replace("joystick", "Jy", StringComparison.OrdinalIgnoreCase)
                .Replace("wired", "Wd", StringComparison.OrdinalIgnoreCase)
                .Replace("wireless", "Ws", StringComparison.OrdinalIgnoreCase)
                .Replace("android", "Ad", StringComparison.OrdinalIgnoreCase)
                .Replace("elite", "El", StringComparison.OrdinalIgnoreCase)
                .Replace("dualshock", "Du", StringComparison.OrdinalIgnoreCase);
            Match versionMatch = s_versionRegex.Match(device.name);
            string version = versionMatch.Success ? versionMatch.Value : "";
            if (!string.IsNullOrEmpty(version))
            {
                product = product
                    .Replace(version, "", StringComparison.OrdinalIgnoreCase)
                    .Trim();
                version = s_specialCharsRegex.Replace(version, "");
            }

            const int TARGET_LENGTH = 7;
            const int MAX_VERSION_LENGTH = 3;
            int manufacturerLength = Mathf.Min(3, manufacturer.Length);
            int versionLength = Mathf.Min(MAX_VERSION_LENGTH, version.Length);
            int productLength = Mathf.Min(TARGET_LENGTH - manufacturerLength, product.Length);
            string shortName = String.IsNullOrEmpty(manufacturer)
                ? ""
                : manufacturer[..manufacturerLength].Capitalize(forceLowerEnding: true);
            shortName += product[..productLength].Capitalize(forceLowerEnding: true);
            shortName += version[..versionLength].ToLowerInvariant();
            return shortName;
        }

        private string DeviceToGroupName(InputDevice device)
        {
            string hashInput = $"{device.name}{device.displayName}{device.description.manufacturer}{device.description.product}{device.description.serial}{device.description.interfaceName}{device.description.version}";
            string fullHash = Hash128.Compute(hashInput).ToString();

            return $"{DeviceShortName(device)}{fullHash[..8]}";
        }

        private void OnActionPerformed(InputAction.CallbackContext context)
        {
            _performingAction = true;
            RefreshIfVisible();
        }

        private void OnActionCanceled(InputAction.CallbackContext context)
        {
            _performingAction = false;
            RefreshIfVisible();
        }

        private void OnLinkClicked(ShallowLinkInfo info)
        {
            var bindingId = Guid.Parse(info.Id);
            ChangeAction((action) =>
            {
                int bindingIndex = action.bindings.IndexOf(b => b.id == bindingId);
                if (bindingIndex < 0) return;

                InputBinding binding = action.bindings[bindingIndex];
                ClassifiedBinding cb = ClassifyBinding(binding, bindingIndex);
                if (!CanBindingBeDeleted(cb)) return;

                action.ChangeBinding(bindingIndex).Erase();
            });
        }

        private bool CanBindingBeDeleted(ClassifiedBinding binding)
        {
            bool isDeletionBlocked = binding
                .EnumerateAllBindings()
                .All((cb) => cb.IsComposite || BlockedDeletionsSet.Contains(cb.Binding.effectivePath));
            return !isDeletionBlocked;
        }

        private void OnLinkHovered(ShallowLinkInfo info)
        {
            _hoveredBindings.Add(info.Id);
            RefreshIfVisible();
        }

        private void OnLinkUnhovered(ShallowLinkInfo info)
        {
            _hoveredBindings.Remove(info.Id);
            RefreshIfVisible();
        }

        private void RecloneTestAction()
        {
            bool wasEnabled = false;
            if (_testAction != null)
            {
                wasEnabled = _testAction.enabled;
                EnsureTestActionDisabled();
                _testAction.performed -= OnActionPerformed;
                _testAction.canceled -= OnActionCanceled;
                _testAction.Dispose();
                _testAction = null;
            }

            InputAction action = _actionReference.Action?.Clone();
            if (action == null) return;

            _testAction = action;
            _testAction.performed += OnActionPerformed;
            _testAction.canceled += OnActionCanceled;
            if (wasEnabled) EnsureTestActionEnabled();
            else EnsureTestActionDisabled();
        }

        private void EnsureTestActionEnabled()
        {
            if (_testAction == null) return;
            _testAction.Enable();
        }

        private void EnsureTestActionDisabled()
        {
            if (_testAction == null) return;
            _testAction.Disable();
        }

        private void DisposeTestAction()
        {
            if (_testAction == null) return;

            EnsureTestActionDisabled();
            _testAction.Dispose();
            _testAction = null;
        }

        private bool IsKnownStandardGamepadPath(string path)
        {
            string newPathDevicePrefix = ExtractPathPrefix(path);
            bool isGamepadPath = newPathDevicePrefix.Equals("<gamepad>", StringComparison.OrdinalIgnoreCase);
            if (isGamepadPath) return true;

            bool isJoystickPath = newPathDevicePrefix.Equals("<joystick>", StringComparison.OrdinalIgnoreCase);
            // If is not joystick nor gamepad path, then it's not known gamepad
            if (!isJoystickPath) return false;

            // <Joystick>/Trigger has different trigger button on different joystick models
            bool isStandardizedJoystickPath = !path.Contains("trigger", StringComparison.OrdinalIgnoreCase);
            return isStandardizedJoystickPath;
        }

        private string ExtractPathPrefix(string path)
        {
            int slashIndex = path.IndexOf('/');
            if (slashIndex < 0) return path;
            return path[..slashIndex];
        }

        private void ChangeAction(Action<InputAction> changeBehaviour)
        {
            InputAction action = _actionReference.Action;
            bool wasEnabled = action.enabled;
            action.Disable();
            changeBehaviour.Invoke(action);
            if (wasEnabled) action.Enable();
            RecloneTestAction();
            RefreshIfVisible();
        }

        [Button]
        private void PrintRawBindingsDebugInfo()
        {
            InputAction action = _actionReference.Action;
            Debug.Log($"[BindingsDebug:{action.name}] Total Bindings: {action.bindings.Count}");
            foreach (InputBinding binding in action.bindings)
            {
                Debug.Log($"[{binding.effectivePath}] {binding.ToDisplayString()} | {(binding.isComposite ? "isComposite" : "")} {(binding.isPartOfComposite ? "isPartOfComposite" : "")}");
            }
        }

        [Button]
        private void PrintClassifiedBindingsDebugInfo()
        {
            InputAction action = _actionReference.Action;
            IEnumerable<ClassifiedBinding> classifiedBindings = CreateClassifiedBindings(action.bindings);
            Debug.Log($"[ClassifiedBindingsDebug:{action.name}] Total Classified Bindings: {classifiedBindings.Count()}");
            foreach (ClassifiedBinding cb in classifiedBindings)
            {
                Debug.Log(string.Join(" | ", new string[]
                {
                    $"[{cb.Binding.effectivePath}] {cb.DisplayString}",
                    $"{(cb.IsKeyboardAndMouse ? "keyboard&mouse" : "")}",
                    $"{(cb.IsKnownDevice ? "knownDevice" : "")}",
                    $"{(cb.IsComposite ? "isComposite" : "")}",
                    $"DeviceIdGroup:{cb.DeviceIdGroup}",
                    $"DeviceShortName: {cb.DeviceShortName}",
                    $"IsDefaultBinding: {cb.IsDefaultBinding}",
                    $"PathDeviceName: {cb.PathDeviceName}",
                    $"PathSubControlName: {cb.PathSubControlName}",
                    $"PathControlName: {cb.PathControlName}",
                    $"OrderIndex: {cb.OrderIndex}"
                }));
                if (cb.IsComposite && cb.CompositeParts != null)
                {
                    foreach (ClassifiedBinding part in cb.CompositeParts)
                    {
                        Debug.Log(string.Join(" | ", new string[]
                        {
                            $"\t[Part:{part.Binding.effectivePath}] {part.DisplayString}",
                            $"{(part.IsKeyboardAndMouse ? "keyboard&mouse" : "")}",
                            $"{(part.IsKnownDevice ? "knownDevice" : "")}",
                            $"DeviceIdGroup:{part.DeviceIdGroup}",
                            $"DeviceShortName: {part.DeviceShortName}",
                            $"IsDefaultBinding: {part.IsDefaultBinding}",
                            $"PathDeviceName: {part.PathDeviceName}",
                            $"PathSubControlName: {part.PathSubControlName}",
                            $"PathControlName: {part.PathControlName}",
                            $"OrderIndex: {part.OrderIndex}"
                        }));
                    }
                }
            }
        }
    }
}
