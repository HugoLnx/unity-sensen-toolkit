#if SENSEN_UI_HEAT
using System.Collections.Generic;
using UnityEngine;

namespace SensenToolkit
{
    public class GenericSettingsDropdown : ADynamicSettingsDropdown
    {
        [SerializeField] private List<DynamicDropdownItemData> _items;
        public override IEnumerable<DynamicDropdownItemData> GetDropdownItems()
        {
            return _items;
        }
    }
}
#endif
