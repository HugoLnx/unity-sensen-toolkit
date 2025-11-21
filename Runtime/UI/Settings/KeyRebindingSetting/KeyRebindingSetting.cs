using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Cysharp.Threading.Tasks;
using EasyButtons;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace SensenToolkit
{
    [System.Serializable]
    public struct RebindingMacroConfig
    {
        public bool BlockListening;
        public bool BlockDeletion;
    }

    public class KeyRebindingSetting : MonoBehaviour
    {
        [Header("Styling")]
        [SerializeField] private Color _performingColor = Color.purple;
        [SerializeField] private Color _hoveredColor = Color.red;
        [SerializeField] private Color _defaultBindingColor = Color.cyan;
        [SerializeField] private Color _customBindingColor = Color.green;

        [Header("Config")]
        [SerializeField] private DynamicInputActionReference _actionReference;
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
        private InputToolkitService _inputToolkit;

        [SerializeField, AutoProperty(AutoPropertyMode.Scene)]
        private KeyRebindingOverlay _overlay;

        private bool _performingAction;
        private InputAction _testAction = null;
        private HashSet<string> _hoveredBindings = new();
        private InputAction _originalAction;
        private BindingMetadataProcessor _metadataProcessor;
        private RebindingMetadataProcessor _rebindingProcessor;
        private KeyListener _keyListener;

        private HashSet<string> _blockedDeletionsSet;
        private HashSet<string> BlockedDeletionsSet => _blockedDeletionsSet ??= new(EnumerateBlockedDeletionsSet());

        private List<string> _ignoredBindingPaths;
        private List<string> IgnoredBindingPaths => _ignoredBindingPaths ??= new(EnumerateIgnoredBindingPaths());

        public bool IsVisible => _visibility.IsVisible;

        private bool IsVector2ActionType => _originalAction?.type == InputActionType.Value
            && _originalAction?.expectedControlType == "Vector2";

        private void Awake()
        {
            _originalAction = _actionReference.OriginalActionClone();
            if (_inputToolkit != null)
            {
                _inputToolkit.BindActionCollection(SetActionCollection);
            }
            _visibility.OnShow += OnShow;
            _visibility.OnHidden += OnHidden;
            _metadataProcessor = new BindingMetadataProcessor(_inputToolkit, _originalAction);
            _rebindingProcessor = new RebindingMetadataProcessor(_inputToolkit, _originalAction);
            _keyListener = new KeyListener(
                cancelThroughEscape: _cancelThroughEscape,
                ignoreBindingPaths: IgnoredBindingPaths
            );
        }

        private void Start()
        {
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
        }

        private string GetBindingDisplayString(InputAction action)
        {
            if (action == null || action.bindings.Count == 0) return "Unbound";

            IEnumerable<BindingMetadata> bindingsMetadata = _metadataProcessor.ProcessAllBindings(action.bindings);

            List<string> displayStrings = new();
            foreach (BindingMetadata bindingData in bindingsMetadata)
            {
                string str = bindingData.DisplayString;
                string bindingId = bindingData.Binding.id.ToString();
                bool isPerforming = _performingAction
                    && _testAction != null
                    && _testAction.activeControl != null
                    && bindingData.EnumerateAllBindings().Any((b) => InputControlPath.Matches(b.Binding.effectivePath, _testAction.activeControl));
                str = $"<link=\"{bindingId}\">{str}</link>";
                bool isHovered = _hoveredBindings.Contains(bindingId);
                bool canBeDeleted = CanBindingBeDeleted(bindingData);
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
                else if (bindingData.IsDefaultBinding)
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
            InputAction action = _actionReference.Action;
            // Debug.Log($"[Bind:{action.name}:{action.type}:{action.expectedControlType}] Started");
            await StartListeningWizard(action);
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
                ApplySingleKeyListeningResult(result);
                RecloneTestAction();
                RefreshComponents();
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
                bindingProcessor: _metadataProcessor,
                getActionName: GetActionHumanName,
                getPartHumanName: GetPartHumanName
            );

            Vector2ListeningWizardResult wizardResult = await wizard.CaptureComposite();

            if (!wizardResult.IsSuccess) return;

            if (wizardResult.IsSingleCompositePart)
            {
                Vector2CompositionPartListeningResult singlePartResult = wizardResult.SingleCompositePartResult.Value;
                KeyListeningResult listeningResult = singlePartResult.ListeningResult;
                var path = BindingPathComponents.FromFullPath(listeningResult.NewPath);
                path.SetControlPart(null);
                listeningResult.NewPath = path.AsString;
                ApplySingleKeyListeningResult(listeningResult);
            }
            else
            {
                ApplyCompositeListeningResult(wizardResult);
            }
            RecloneTestAction();
            RefreshComponents();
        }

        private void ApplySingleKeyListeningResult(KeyListeningResult result)
        {
            if (!result.HasListened) return;

            RebindingMetadata rebindingMetadata = _rebindingProcessor.ProcessKeyListeningResult(result);
            InputBinding binding = rebindingMetadata.NewBinding;
            BindingMetadata bindingData = _metadataProcessor.ProcessSingleBinding(binding);
            _overlay.UpdateKeyName(bindingData.DisplayString);
            if (rebindingMetadata.IsAlreadyBound)
            {
                InputAction action = result.Action;
                string newPath = result.NewPath;
                // Debug.Log($"[Bind:{action.name}] Path {newPath} is already bound, skipping adding new binding.");
                return;
            }

            ChangeAction((action) =>
            {
                InputBinding b = rebindingMetadata.NewBinding;
                action.AddBinding(b.path).WithGroups(b.groups);
            });
        }

        private void ApplyCompositeListeningResult(Vector2ListeningWizardResult wizardResult)
        {
            List<InputBinding> partBindings = new();

            foreach (Vector2CompositionPartListeningResult partResult in wizardResult.AllResults)
            {
                KeyListeningResult listeningResult = partResult.ListeningResult;
                RebindingMetadata rebindingMetadata = _rebindingProcessor.ProcessKeyListeningResult(listeningResult);
                InputBinding binding = rebindingMetadata.NewBinding;
                binding.name = partResult.PartName;
                partBindings.Add(binding);
            }

            ChangeAction((action) =>
            {
                InputActionSetupExtensions.CompositeSyntax compositeBuilder = action.AddCompositeBinding("2DVector");
                foreach (InputBinding partBinding in partBindings)
                {
                    compositeBuilder.With(
                        name: partBinding.name,
                        binding: partBinding.path,
                        groups: partBinding.groups
                    );
                }
            });
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
            IEnumerable<BindingMetadata> bindingsMetadata = _metadataProcessor.ProcessAllBindings(action.bindings);
            BindingMetadata bindingData = bindingsMetadata.FirstOrDefault(b => b.Binding.id == bindingId);
            if (!CanBindingBeDeleted(bindingData)) return;

            ChangeAction((_) =>
            {
                action.ChangeBinding(bindingIndex).Erase();
            });
        }

        private bool CanBindingBeDeleted(BindingMetadata binding)
        {
            bool isDeletionBlocked = binding
                .EnumerateAllBindings()
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
            RecloneTestAction();
            RefreshIfVisible();
        }

        private IEnumerable<string> EnumerateBlockedDeletionsSet()
        {
            if (_keyboardEscapeConfig.BlockDeletion)
            {
                yield return RebindingMetadataProcessor.ESCAPE_KEY_PATH;
            }

            if (_keyboardArrowsConfig.BlockDeletion)
            {
                foreach (string path in RebindingMetadataProcessor.KeyboardArrowPaths)
                {
                    yield return path;
                }
            }

            if (_gamepadDpadConfig.BlockDeletion)
            {
                foreach (string path in RebindingMetadataProcessor.GamepadDpadPaths)
                {
                    yield return path;
                }
            }

            if (_gamepadLeftStickConfig.BlockDeletion)
            {
                foreach (string path in RebindingMetadataProcessor.GamepadLeftStickPaths)
                {
                    yield return path;
                }
            }
        }

        private IEnumerable<string> EnumerateIgnoredBindingPaths()
        {
            if (_cancelThroughEscape || _keyboardEscapeConfig.BlockListening)
            {
                yield return RebindingMetadataProcessor.ESCAPE_KEY_PATH;
            }

            foreach (string path in RebindingMetadataProcessor.MousePositionPaths)
            {
                yield return path;
            }

            if (_keyboardArrowsConfig.BlockListening)
            {
                foreach (string path in RebindingMetadataProcessor.KeyboardArrowPaths)
                {
                    yield return path;
                }
            }

            if (!_gamepadDpadConfig.BlockListening)
            {
                foreach (string path in RebindingMetadataProcessor.GamepadDpadPaths)
                {
                    yield return path;
                }
            }

            if (_gamepadLeftStickConfig.BlockListening)
            {
                foreach (string path in RebindingMetadataProcessor.GamepadLeftStickPaths)
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
            IEnumerable<BindingMetadata> bindingsMetadata = _metadataProcessor.ProcessAllBindings(action.bindings);
            Debug.Log($"[BindingsMetadata:{action.name}] Total: {bindingsMetadata.Count()}");
            foreach (BindingMetadata b in bindingsMetadata)
            {
                Debug.Log(string.Join(" | ", new string[]
                {
                    $"[{b.Binding.effectivePath}] {b.DisplayString}",
                    $"{(b.IsKeyboardAndMouse ? "keyboard&mouse" : "")}",
                    $"{(b.IsKnownStandardDevice ? "knownDevice" : "")}",
                    $"{(b.IsComposite ? "isComposite" : "")}",
                    $"DeviceIdGroup:{b.DeviceIdGroup}",
                    $"DeviceShortName: {b.DeviceShortName}",
                    $"IsDefaultBinding: {b.IsDefaultBinding}",
                    $"PathDevice: {b.Path.Device}",
                    $"PathControl: {b.Path.Control}",
                    $"PathControlPart: {b.Path.ControlPart}",
                    $"OrderIndex: {b.OrderIndex}"
                }));
                if (b.IsComposite && b.CompositeParts != null)
                {
                    foreach (BindingMetadata part in b.CompositeParts)
                    {
                        Debug.Log(string.Join(" | ", new string[]
                        {
                            $"\t[Part:{part.Binding.effectivePath}] {part.DisplayString}",
                            $"{(part.IsKeyboardAndMouse ? "keyboard&mouse" : "")}",
                            $"{(part.IsKnownStandardDevice ? "knownDevice" : "")}",
                            $"DeviceIdGroup:{part.DeviceIdGroup}",
                            $"DeviceShortName: {part.DeviceShortName}",
                            $"IsDefaultBinding: {part.IsDefaultBinding}",
                            $"PathDevice: {part.Path.Device}",
                            $"PathControl: {part.Path.Control}",
                            $"PathControlPart: {part.Path.ControlPart}",
                            $"OrderIndex: {part.OrderIndex}"
                        }));
                    }
                }
            }
        }
    }
}
