using System;
using EasyButtons;
using MyBox;
using SensenToolkit;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;

namespace SensenToolkit
{
    [Serializable]
    public class KeyRebindingLocalizedTexts
    {
        [SerializeField, MustBeAssigned] private LocalizedString _rebindingCanceled;
        [SerializeField, MustBeAssigned] private LocalizedString _rebindingErrorDifferentDevice;
        [SerializeField, MustBeAssigned] private LocalizedString _rebindingErrorDuplicateBinding;
        [SerializeField, MustBeAssigned] private LocalizedString _rebindingPressToCancel;

        public string GetRebindingCanceledText() => _rebindingCanceled.GetLocalizedString();
        public string GetRebindingErrorDifferentDeviceText() => _rebindingErrorDifferentDevice.GetLocalizedString();
        public string GetRebindingErrorDuplicateBindingText() => _rebindingErrorDuplicateBinding.GetLocalizedString();
        public string GetRebindingPressToCancelText(string keyName) => _rebindingPressToCancel.GetLocalizedString(keyName);
    }

    public class KeyRebindingService : APermanentSingleton<KeyRebindingService>
    {
        [SerializeField] private KeyRebindingLocalizedTexts _localizedTexts;
        public KeyRebindingLocalizedTexts LocalizedTexts => _localizedTexts;
        private InputToolkitService InputToolkit => InputToolkitService.Instance;
        private InputRebindingSerializer Serializer
            => _serializer ??= new InputRebindingSerializer(InputToolkit.Actions, InputToolkit.OriginalActions);
        private InputRebindingSerializer _serializer;

        private string _serializedDebug;

        public event Action OnRebindsLoaded = delegate { };

        private void OnEnable()
        {
            InputToolkit.BindActionCollection(SetActionCollection);
        }

        protected override void OnDisableAny()
        {
            if (!InputToolkitService.HasInstance) return;
            InputToolkit.UnbindActionCollection(SetActionCollection);
        }

        public string Serialize(bool pretty = false)
        {
            string serialized = Serializer.Serialize(out bool hasRebindings, pretty);
            return hasRebindings ? serialized : null;
        }

        public bool DeserializeAndLoad(string serialized)
        {
            if (string.IsNullOrWhiteSpace(serialized))
            {
                Debug.LogWarning("[KeyRebindingService] Provided Serialized is null or empty.");
                return false;
            }
            bool success = Serializer.LoadSerializedJson(serialized);
            OnRebindsLoaded.Invoke();

            return success;
        }

        public void ResetToDefaults()
        {
            InputUtils.ReplaceBindings(
                source: InputToolkit.OriginalActions,
                target: InputToolkit.Actions
            );
            OnRebindsLoaded.Invoke();
        }

        [Button]
        private void PrintSerializeDebug()
        {
            string serialized = Serialize(pretty: true);
            Debug.Log("[BumashutaInputService] Serialized Actions...");
            Debug.Log(serialized);
            _serializedDebug = serialized;
        }

        [Button]
        private void LoadSerializeDebug()
        {
            if (string.IsNullOrEmpty(_serializedDebug))
            {
                Debug.LogWarning("[BumashutaInputService] No serialized data to load.");
                return;
            }

            bool success = DeserializeAndLoad(_serializedDebug);
            Debug.Log($"[BumashutaInputService] Loaded Serialized Actions: Success={success}");
        }

        private void SetActionCollection(IInputActionCollection2 _)
        {
            _serializer = null;
        }
    }
}
