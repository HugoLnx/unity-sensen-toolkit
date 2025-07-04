#if SENSEN_UI_HEAT
using System;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using UnityEngine;

namespace SensenToolkit
{
    public class ScreenModeSettingsDropdown : ADynamicSettingsDropdown
    {
        public override IEnumerable<DynamicDropdownItemData> GetDropdownItems()
        {
            return Enum.GetValues(typeof(FullScreenMode))
                .Cast<FullScreenMode>()
                .Select(mode => new DynamicDropdownItemData()
                {
                    Name = GetNameOf(mode),
                    Key = ScreenService.ScreenModeToKey(mode)
                });
        }

        private string GetNameOf(FullScreenMode mode)
        {
            return mode switch
            {
                FullScreenMode.ExclusiveFullScreen => "Exclusive Fullscreen",
                FullScreenMode.FullScreenWindow => "Fullscreen Window",
                FullScreenMode.MaximizedWindow => "Maximized Window",
                FullScreenMode.Windowed => "Windowed",
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            };
        }
    }
}
#endif
