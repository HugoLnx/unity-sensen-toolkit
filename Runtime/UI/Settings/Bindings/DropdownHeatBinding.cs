#if SENSEN_UI_HEAT
using MyBox;
using UnityEngine;
using Michsky.UI.Heat;
using System;
using System.Text.RegularExpressions;

namespace SensenToolkit
{
    public class DropdownHeatBinding : AUiBindingBase<StringValueSO, string, int>
    {
        [SerializeField, AutoProperty] private Dropdown _dropdown;
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

        protected override string FromUIValue(int uiValue)
        {
            Dropdown.Item item = _dropdown.items[uiValue];
            return ItemToKey(item);
        }
        protected override int ToUIValue(string value)
        {
            int index = _dropdown.items.FindIndex(
                item => string.Equals(ItemToKey(item), value, StringComparison.OrdinalIgnoreCase));
            return index >= 0 ? index : 0; // Default to first item if not found
        }

        private string ItemToKey(Dropdown.Item item)
        {
            if (string.IsNullOrEmpty(item.localizationKey))
            {
                return Regex.Replace(
                    item.itemName.ToLowerInvariant(),
                    @"[^a-z0-9]+", "",
                    RegexOptions.Compiled | RegexOptions.CultureInvariant
                );
            }

            return item.localizationKey;
        }
    }
}
#endif
