#if SENSEN_UI_HEAT
using MyBox;
using UnityEngine;
using Michsky.UI.Heat;

namespace SensenToolkit
{
    public class DropdownHeatFloatBinding : AUiBindingBase<FloatValueSO, float, int>
    {
        [SerializeField, AutoProperty] private Dropdown _dropdown;
        [SerializeField] private bool _roundValue = false;
        [SerializeField] private float _valueOnParseFail = -1f;
        protected override bool ShouldWaitToAddListener => true;

        protected override void BindUiChanges()
        {
            _dropdown.onValueChanged.AddListener(OnUiValueChanged);
        }

        protected override void UnbindUiChanges()
        {
            _dropdown.onValueChanged.RemoveListener(OnUiValueChanged);
        }

        protected override void SetUiValue(int value)
        {
            _dropdown.SetDropdownIndex(value);
        }

        protected override int GetUiValue()
        {
            return _dropdown.selectedItemIndex;
        }

        protected override float FromUIValue(int uiValue)
        {
            string itemName = _dropdown.items[uiValue].itemName;
            return NameToFloat(itemName);
        }
        protected override int ToUIValue(float value)
        {
            int index = _dropdown.items.FindIndex(item => value == NameToFloat(item.itemName));
            return index >= 0 ? index : 0; // Default to first item if not found
        }

        private float NameToFloat(string itemName)
        {
            if (!float.TryParse(itemName, out float itemValue))
            {
                return _valueOnParseFail;
            }
            return _roundValue ? Mathf.Round(itemValue) : itemValue;
        }
    }
}
#endif
