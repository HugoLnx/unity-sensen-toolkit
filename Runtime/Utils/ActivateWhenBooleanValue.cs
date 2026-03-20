using MyBox;
using UnityEngine;

namespace SensenToolkit
{
    public class ActivateWhenBooleanValue : MonoBehaviour
    {
        [SerializeField, MustBeAssigned] private BoolValueSO _valueSo;
        [SerializeField] private bool _invert;

        private void Awake()
        {
            _valueSo.OnValueChanged += OnValueChanged;
            RefreshActiveState();
        }

        private void OnDestroy()
        {
            _valueSo.OnValueChanged -= OnValueChanged;
        }

        private void OnValueChanged(BoolValueSO _) => RefreshActiveState();

        private void RefreshActiveState()
        {
            if (this == null || gameObject == null) return;
            gameObject.SetActive(_invert ? !_valueSo.Value : _valueSo.Value);
        }
    }
}
