using System;
using EasyButtons;
using SensenToolkit;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SensenToolkit
{
    public class KeyRebindingService : APermanentSingleton<KeyRebindingService>
    {
        private InputToolkitService InputToolkit => InputToolkitService.Instance;
        private InputRebindingSerializer Serializer
            => _serializer ??= new InputRebindingSerializer(InputToolkit.Actions, InputToolkit.OriginalActions);
        private InputRebindingSerializer _serializer;

        private string _serializedDebug;

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
            return Serializer.LoadSerializedJson(serialized);
        }

        public void ResetToDefaults()
        {
            InputUtils.ReplaceBindings(
                source: InputToolkit.OriginalActions,
                target: InputToolkit.Actions
            );
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
