#if SENSEN_UI_HEAT
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MyBox;
using UnityEngine;

namespace SensenToolkit
{
    public class DisplaySettingsDropdown : ADynamicSettingsDropdown
    {
        [SerializeField, ReadOnly] private DisplayService _displayService;

        private void Awake()
        {
            _displayService = DisplayService.Instance;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _displayService.OnDisplaysChanged += UpdateDropdownOptions;
        }

        private void UpdateDropdownOptions(List<DisplayInfo> _)
        {
            RecreateDropdownItems();
        }

        public override IEnumerable<DynamicDropdownItemData> GetDropdownItems()
        {
            return _displayService.AllDisplays.Select(display => new DynamicDropdownItemData
            {
                Name = Sanitize(display.name),
                Key = display.name,
            });
        }

        private static string Sanitize(string str)
        {
            str = Regex.Replace(str.Trim(), @"[^A-Za-z0-9\s]+", "", RegexOptions.Compiled | RegexOptions.CultureInvariant);
            str = Regex.Replace(str, @"\s+", " ", RegexOptions.Compiled | RegexOptions.CultureInvariant);
            return str;
        }
    }
}
#endif
