#if SENSEN_UI_HEAT
using System.Collections.Generic;
using System.Linq;
using Michsky.UI.Heat;
using MyBox;
using UnityEngine;

namespace SensenToolkit
{
    public struct DynamicDropdownItemData
    {
        public string Name;
        public string Key;
    }

    public abstract class ADynamicSettingsDropdown : MonoBehaviour
    {
        [SerializeField, AutoProperty] private Dropdown _dropdown;
        [SerializeField, AutoProperty] private DropdownHeatBinding _dropdownBinding;

        public abstract IEnumerable<DynamicDropdownItemData> GetDropdownItems();

        protected virtual void OnEnable()
        {
            RecreateDropdownItems();
        }

        protected void RecreateDropdownItems()
        {
            foreach (Dropdown.Item item in _dropdown.items.ToArray())
            {
                _dropdown.RemoveItem(item.itemName, false);
            }

            var itemsData = GetDropdownItems().ToList();
            foreach (DynamicDropdownItemData itemData in itemsData)
            {
                _dropdown.CreateNewItem(itemData.Name, false);
            }

            for (int i = 0; i < itemsData.Count; i++)
            {
                Dropdown.Item dropdownItem = _dropdown.items[i];
                DynamicDropdownItemData itemData = itemsData[i];
                dropdownItem.itemName = itemData.Name;
                dropdownItem.localizationKey = itemData.Key;
            }

            _dropdown.Initialize();

            _dropdownBinding.PushCurrentValueToUi();
        }
    }
}
#endif
