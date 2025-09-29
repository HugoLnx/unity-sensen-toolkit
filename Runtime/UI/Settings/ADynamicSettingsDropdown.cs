#if SENSEN_UI_HEAT
using System;
using System.Collections.Generic;
using System.Linq;
using Michsky.UI.Heat;
using MyBox;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Serialization;
using UnityLocalization = UnityEngine.Localization.Settings.LocalizationSettings;

namespace SensenToolkit
{
    [System.Serializable]
    public struct DynamicDropdownItemData
    {
        [SerializeField]
        [FormerlySerializedAs("Name")]
        private string _name;
        [SerializeField]
        private LocalizedString _nameI18n;
        public string Key;
        public bool IsLocalized => _nameI18n != null && !_nameI18n.IsEmpty;
        public string Name
        {
            get => IsLocalized ? _nameI18n.GetLocalizedString() : _name;
            set => _name = value;
        }
        public LocalizedString NameI18n
        {
            get => IsLocalized ? _nameI18n : null;
            set => _nameI18n = value;
        }
    }

    public abstract class ADynamicSettingsDropdown : MonoBehaviour
    {
        [SerializeField, AutoProperty] private Dropdown _dropdown;
        [SerializeField, AutoProperty] private AUiBindingBaseAbstract _dropdownBinding;
        private bool _createdDropdownItems;

        public abstract IEnumerable<DynamicDropdownItemData> GetDropdownItems();

        protected virtual void Awake()
        {
            UnityLocalization.SelectedLocaleChanged += OnLocalizationChanged;
            EnsureDropdownItems();
        }

        protected virtual void OnEnable()
        {
            EnsureDropdownItems();
        }

        protected virtual void OnDestroy()
        {
            UnityLocalization.SelectedLocaleChanged -= OnLocalizationChanged;
        }

        private void OnLocalizationChanged(Locale locale)
        {
            RecreateDropdownItems();
        }

        protected void EnsureDropdownItems()
        {
            if (_createdDropdownItems) return;
            RecreateDropdownItems();
        }

        protected void RecreateDropdownItems()
        {
            foreach (Dropdown.Item item in _dropdown.items.ToArray())
            {
                _dropdown.RemoveItem(item.itemName, false);
            }

            var itemsData = GetDropdownItems().ToList();
            if (itemsData.Count == 0) return;
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
            _createdDropdownItems = true;
        }
    }
}
#endif
