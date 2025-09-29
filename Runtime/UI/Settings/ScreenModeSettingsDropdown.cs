#if SENSEN_UI_HEAT
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

namespace SensenToolkit
{
    [System.Serializable]
    public class ModeI18nPair
    {
        public FullScreenMode Mode;
        public LocalizedString NameI18n;
    }

    public class ScreenModeSettingsDropdown : ADynamicSettingsDropdown
    {
        [SerializeField] private List<ModeI18nPair> _modesI18n;
        public override IEnumerable<DynamicDropdownItemData> GetDropdownItems()
        {
            return _modesI18n
                .Select(pair => new DynamicDropdownItemData()
                {
                    NameI18n = pair.NameI18n,
                    Key = ScreenService.ScreenModeToKey(pair.Mode)
                });
        }
    }
}
#endif
