using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using EasyButtons;
using MyBox;
using SensenToolkit.InputRebinding.Data;
using SensenToolkit.InputRebinding.Internal;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace SensenToolkit
{
    public class KeyRebindingSetting : MonoBehaviour
    {
        [Header("Styling")]
        [SerializeField] private Color _performingColor = Color.purple;
        [SerializeField] private Color _hoveredColor = Color.red;
        [SerializeField] private Color _defaultBindingColor = Color.cyan;
        [SerializeField] private Color _customBindingColor = Color.green;

        [Header("Config")]
        [SerializeField] private DynamicInputActionReference _actionReference;
        [SerializeField] private List<BindingReplicationInstruction> _replicationInstructions = new();
        [SerializeField] private bool _cancelThroughEscape = true;
        [SerializeField]
        private RebindingMacroConfig _keyboardEscapeConfig = new()
        {
            BlockListening = true,
            BlockDeletion = false,
        };
        [SerializeField] private RebindingMacroConfig _keyboardArrowsConfig;
        [SerializeField] private RebindingMacroConfig _gamepadDpadConfig;
        [SerializeField] private RebindingMacroConfig _gamepadLeftStickConfig;

        [Header("Localization")]
        [SerializeField, MustBeAssigned] private LocalizedString _actionNameI18n;
        [SerializeField, MustBeAssigned] private LocalizedString _upPartNameI18n;
        [SerializeField, MustBeAssigned] private LocalizedString _downPartNameI18n;
        [SerializeField, MustBeAssigned] private LocalizedString _leftPartNameI18n;
        [SerializeField, MustBeAssigned] private LocalizedString _rightPartNameI18n;
        [Header("References")]
        [SerializeField, MustBeAssigned] private TMP_Text _keyText;
        [SerializeField, MustBeAssigned] private Button _addButton;
        [SerializeField, MustBeAssigned] private Button _addDefaultsButton;
        [SerializeField, AutoProperty(AutoPropertyMode.Children)]
        private InteractiveTextLinks _interactiveTextLinks;
        [SerializeField, AutoProperty]
        private PanelChildVisibilityEvents _visibility;

        [SerializeField, AutoProperty(AutoPropertyMode.Scene)]
        private KeyRebindingOverlay _overlay;

        private bool _performingAction;
        private InputAction _testAction = null;
        private HashSet<string> _hoveredBindings = new();
        private InputAction _originalAction;
        private BindingPlusCollection _originalBindings;
        private List<BindingPlus> _originalBindingsPlus;
        private KeyListeningMetadataProcessor _rebindingProcessor;
        private KeyListener _keyListener;

        private HashSet<string> _blockedDeletionsSet;
        private HashSet<string> BlockedDeletionsSet => _blockedDeletionsSet ??= new(EnumerateBlockedDeletionsSet());

        [NonSerialized] private List<string> _ignoredBindingPaths;
        private List<string> IgnoredBindingPaths => _ignoredBindingPaths ??= new(EnumerateIgnoredBindingPaths());

        public bool IsVisible => _visibility.IsVisible;

        private bool IsVector2ActionType => _originalAction?.type == InputActionType.Value
            && _originalAction?.expectedControlType == "Vector2";

        private void Awake()
        {
            _originalAction = _actionReference.OriginalActionClone();

            _visibility.OnShow += OnShow;
            _visibility.OnHidden += OnHidden;
            _rebindingProcessor = new KeyListeningMetadataProcessor(
                keyboardAndMouseGroup: InputToolkitService.BindingGroupKeyboardAndMouse,
                gamepadGroup: InputToolkitService.BindingGroupGamepad
            );
            _keyListener = new KeyListener(
                cancelThroughEscape: _cancelThroughEscape,
                ignoreBindingPaths: IgnoredBindingPaths
            );
            InputToolkitService.AddAssignListener(this,
                assign: (s) => s.BindActionCollection(SetActionCollection),
                unassign: (s) => s.UnbindActionCollection(SetActionCollection)
            );
        }

        private void Start()
        {
            _originalBindings = new BindingPlusCollectionBuilder()
                .AddRange(_actionReference.Action, _originalAction.bindings)
                .Build();
            RecloneTestAction();
            RefreshIfVisible();
        }

        private void OnEnable()
        {
            KeyRebindingService.AddAssignListener(this,
                forceInstance: false,
                assign: (s) => KeyRebindingService.Instance.OnRebindsLoaded += RefreshIfVisible,
                unassign: (s) => KeyRebindingService.Instance.OnRebindsLoaded -= RefreshIfVisible
            );
        }

        private void OnDisable()
        {
            KeyRebindingService.UnassignAndRemoveListener(this);
        }

        private void OnDestroy()
        {
            InputToolkitService.UnassignAndRemoveListener(this);
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
            foreach (BindingReplicationInstruction instruction in _replicationInstructions)
            {
                instruction.TargetActionReference.SetActionCollection(actions);
            }
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
        }

        private string GetBindingDisplayString(InputAction action)
        {
            if (action == null || action.bindings.Count == 0) return "Unbound";

            BindingPlusCollection bindings = BindingPlusCollectionBuilder.Build(action);

            List<string> displayStrings = new();
            foreach (BindingPlus plus in bindings)
            {
                string str = plus.DisplayString;
                string bindingId = plus.Binding.id.ToString();
                bool isPerforming = _performingAction
                    && _testAction != null
                    && _testAction.activeControl != null
                    && plus.EnumerateBindingsWithPath()
                        .Any((b) => InputControlPath.Matches(b.Binding.effectivePath, _testAction.activeControl));
                str = $"<link=\"{bindingId}\">{str}</link>";
                bool isHovered = _hoveredBindings.Contains(bindingId);
                bool canBeDeleted = CanBindingBeDeleted(plus);
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
                else if (plus.IsDefaultBinding)
                {
                    str = $"<color={_defaultBindingColor.ToHex()}>{str}</color>";
                }
                else
                {
                    str = $"<color={_customBindingColor.ToHex()}>{str}</color>";
                }

                displayStrings.Add(str);
            }
            return string.Join(" ", displayStrings);
        }

        private void OnAddClicked() => OnAddClickedAsync().Forget();

        private async UniTaskVoid OnAddClickedAsync()
        {
            await StartListeningWizard(_actionReference.Action);
        }

        private async UniTask StartListeningWizard(InputAction action)
        {
            bool isSingleKey = action.type == InputActionType.Button;
            if (isSingleKey)
            {
                await StartSingleKeyListeningWizard(action);
                return;
            }

            if (IsVector2ActionType)
            {
                await StartVector2ListeningWizard(action);
                return;
            }
        }

        private async UniTask StartSingleKeyListeningWizard(InputAction action)
        {
            _overlay.ShowListening(GetActionHumanName());

            KeyListeningResult result = await _keyListener.ListenToKey(action: action);

            if (result.HasListened)
            {
                KeyListeningMetadata rebindingMetadata = _rebindingProcessor.ProcessKeyListeningResult(result);
                var newBinding = BindingPlus.Build(_actionReference.Action, rebindingMetadata.NewBinding);
                _overlay.UpdateKeyName(newBinding.DisplayString);

                TryToAppendBinding(newBinding);
            }

            _overlay.Hide(delay: result.HasListened ? 0.15f : 0f);
        }

        private async UniTask StartVector2ListeningWizard(InputAction action)
        {
            Vector2ListeningWizard wizard = new(
                action: action,
                keyListener: _keyListener,
                overlay: _overlay,
                rebindingProcessor: _rebindingProcessor,
                getActionName: GetActionHumanName,
                getPartHumanName: GetPartHumanName
            );

            Vector2ListeningWizardResult wizardResult = await wizard.CaptureComposite();

            if (!wizardResult.IsSuccess)
            {
                Debug.Log("[Bind] Vector2 Listening Wizard was cancelled or failed.");
                return;
            }

            TryToAppendBinding(wizardResult.NewBinding);
        }

        private void TryToAppendBinding(BindingPlus newBinding)
        {
            InputAction action = _actionReference.Action;
            BindingPlusCollection bindings = BindingPlusCollectionBuilder.Build(action);
            bool isAlreadyBound = bindings.ContainsEquivalent(newBinding);
            if (isAlreadyBound)
            {
                string newPath = newBinding.Path.AsString;
                Debug.Log($"[Bind:{action.name}] Path {newPath} is already bound, skipping adding new binding.");
                return;
            }

            bindings = bindings.WithAppended(newBinding);

            ChangeAction(bindings.ReplaceActionBindings);
        }

        private void OnAddDefaultsClicked()
        {
            if (_originalAction == null) return;

            BindingPlusCollection newBindings = BindingPlusCollectionBuilder
                .Build(_actionReference.Action)
                .WithPrependedDefaultBindings(_originalBindings);

            ChangeAction(newBindings.ReplaceActionBindings);
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
            InputAction action = _actionReference.Action;

            int bindingIndex = action.bindings.IndexOf(b => b.id == bindingId);
            if (bindingIndex < 0) return;

            InputBinding binding = action.bindings[bindingIndex];
            BindingPlusCollection bindingsMetadata = BindingPlusCollectionBuilder.Build(action);
            BindingPlus bindingData = bindingsMetadata.FirstOrDefault(b => b.Binding.id == bindingId);
            if (!CanBindingBeDeleted(bindingData)) return;

            ChangeAction((_) =>
            {
                action.ChangeBinding(bindingIndex).Erase();
            });
        }

        private bool CanBindingBeDeleted(BindingPlus binding)
        {
            bool isDeletionBlocked = binding
                .EnumerateBindingsWithPath()
                .All((bingingData) => bingingData.IsComposite || BlockedDeletionsSet.Contains(bingingData.Binding.effectivePath));
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

        private void ChangeAction(Action<InputAction> changeBehaviour)
        {
            InputAction action = _actionReference.Action;
            bool wasEnabled = action.enabled;
            action.Disable();
            changeBehaviour.Invoke(action);
            if (wasEnabled) action.Enable();
            ReplicateBindings();
            RecloneTestAction();
            RefreshIfVisible();
        }

        private void ReplicateBindings()
        {
            foreach (BindingReplicationInstruction instruction in _replicationInstructions)
            {
                InputAction sourceAction = _actionReference.Action;
                InputAction targetAction = instruction.TargetActionReference.Action;

                bool wasEnabled = targetAction.enabled;
                targetAction.Disable();
                var safeLoop = new SafeLoop(250);
                while (targetAction.bindings.Count > 0)
                {
                    targetAction.ChangeBinding(0).Erase();
                    safeLoop.Count();
                }

                List<InputBinding> bindingsToReplicate = new();
                foreach (InputBinding binding in sourceAction.bindings)
                {
                    if (!instruction.IsReplicationAllowed(binding.effectivePath)) continue;
                    bindingsToReplicate.Add(binding);
                }
                for (int i = 0; i < bindingsToReplicate.Count; i++)
                {
                    InputUtils.AddNextBindingsToAction(targetAction, bindingsToReplicate, ref i);
                }
                if (wasEnabled) targetAction.Enable();
            }
        }

        private IEnumerable<string> EnumerateBlockedDeletionsSet()
        {
            if (_keyboardEscapeConfig.BlockDeletion)
            {
                yield return InputConstants.ESCAPE_KEY_PATH;
            }

            if (_keyboardArrowsConfig.BlockDeletion)
            {
                foreach (string path in InputConstants.KeyboardArrowPaths)
                {
                    yield return path;
                }
            }

            if (_gamepadDpadConfig.BlockDeletion)
            {
                foreach (string path in InputConstants.GamepadDpadPaths)
                {
                    yield return path;
                }
            }

            if (_gamepadLeftStickConfig.BlockDeletion)
            {
                foreach (string path in InputConstants.GamepadLeftStickPaths)
                {
                    yield return path;
                }
            }
        }

        private IEnumerable<string> EnumerateIgnoredBindingPaths()
        {
            if (_cancelThroughEscape || _keyboardEscapeConfig.BlockListening)
            {
                yield return InputConstants.ESCAPE_KEY_PATH;
            }

            foreach (string path in InputConstants.MousePositionPaths)
            {
                yield return path;
            }

            if (_keyboardArrowsConfig.BlockListening)
            {
                foreach (string path in InputConstants.KeyboardArrowPaths)
                {
                    yield return path;
                }
            }

            if (!_gamepadDpadConfig.BlockListening)
            {
                foreach (string path in InputConstants.GamepadDpadPaths)
                {
                    yield return path;
                }
            }

            if (_gamepadLeftStickConfig.BlockListening)
            {
                foreach (string path in InputConstants.GamepadLeftStickPaths)
                {
                    yield return path;
                }
            }
        }

        private string GetActionHumanName() => _actionNameI18n.GetLocalizedString();
        private string GetPartHumanName(string partName)
        {
            return partName.ToLowerInvariant() switch
            {
                "up" => _upPartNameI18n.GetLocalizedString(),
                "down" => _downPartNameI18n.GetLocalizedString(),
                "left" => _leftPartNameI18n.GetLocalizedString(),
                "right" => _rightPartNameI18n.GetLocalizedString(),
                _ => partName,
            };
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
        private void PrintBindingsMetadataDebugInfo()
        {
            InputAction action = _actionReference.Action;
            BindingPlusCollection bindingsMetadata = BindingPlusCollectionBuilder.Build(action);
            Debug.Log($"[BindingsMetadata:{action.name}] Total: {bindingsMetadata.Count()}");
            foreach (BindingPlus b in bindingsMetadata)
            {
                Debug.Log(string.Join(" | ", new string[]
                {
                    $"[{b.Binding.effectivePath}] {b.DisplayString}",
                    $"{(b.IsKeyboardAndMouse ? "keyboard&mouse" : "")}",
                    $"{(b.IsKnownStandardDevice ? "knownDevice" : "")}",
                    $"{(b.IsComposite ? "isComposite" : "")}",
                    $"DeviceId:{b.DeviceId}",
                    $"DeviceShortName: {b.CustomDeviceShortName}",
                    $"IsDefaultBinding: {b.IsDefaultBinding}",
                    $"PathDevice: {b.Path.Device}",
                    $"PathControl: {b.Path.Control}",
                    $"PathControlPart: {b.Path.ControlPart}"
                }));
                if (b.IsComposite && b.CompositeChildren != null)
                {
                    foreach (BindingPlus part in b.CompositeChildren)
                    {
                        Debug.Log(string.Join(" | ", new string[]
                        {
                            $"\t[Part:{part.Binding.effectivePath}] {part.DisplayString}",
                            $"{(part.IsKeyboardAndMouse ? "keyboard&mouse" : "")}",
                            $"{(part.IsKnownStandardDevice ? "knownDevice" : "")}",
                            $"DeviceIdGroup:{part.DeviceId}",
                            $"DeviceShortName: {part.CustomDeviceShortName}",
                            $"IsDefaultBinding: {part.IsDefaultBinding}",
                            $"PathDevice: {part.Path.Device}",
                            $"PathControl: {part.Path.Control}",
                            $"PathControlPart: {part.Path.ControlPart}"
                        }));
                    }
                }
            }
        }
    }
}
