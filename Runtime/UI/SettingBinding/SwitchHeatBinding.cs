#if SENSEN_UI_HEAT
using MyBox;
using UnityEngine;
using Michsky.UI.Heat;

namespace SensenToolkit
{
    public class SwitchHeatBinding : AUiBindingBase<BoolValueSO, bool, bool>
    {
        [SerializeField, AutoProperty] private SwitchManager _switch;

        protected override void BindUiChanges()
        {
            _switch.onValueChanged.AddListener(OnUiValueChanged);
        }

        protected override void UnbindUiChanges()
        {
            _switch.onValueChanged.RemoveListener(OnUiValueChanged);
        }

        protected override void SetUiValue(bool value)
        {
            _switch.isOn = value;
        }

        protected override bool GetUiValue()
        {
            return _switch.isOn;
        }

        protected override bool FromUIValue(bool uiValue) => uiValue;
        protected override bool ToUIValue(bool value) => value;
    }
}
#endif
