using System.Collections;
using MyBox;
using UnityEngine;

namespace SensenToolkit
{
    public class KeyRebindingSettingsPanel : MonoBehaviour
    {
        [SerializeField, MustBeAssigned] private StringValueSO _rebindingSerializationValue;
        private KeyRebindingService RebindingService => KeyRebindingService.Instance;

        [SerializeField, AutoProperty] private PanelChildVisibilityEvents _visibility;
        private bool _syncEnabled = false;

        private void Awake()
        {
            _visibility.OnHidden += OnHidden;
        }

        private void OnDestroy()
        {
            _visibility.OnHidden -= OnHidden;
        }

        private IEnumerator Start()
        {
            for (int i = 0; i < 3; i++) yield return null;
            _syncEnabled = true;
        }

        private void OnHidden()
        {
            if (!_syncEnabled) return;
            string serialized = RebindingService.Serialize(pretty: false);
            _rebindingSerializationValue.Value = serialized;
        }
    }
}
