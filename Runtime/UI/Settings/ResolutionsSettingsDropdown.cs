#if SENSEN_UI_HEAT
using System.Collections.Generic;
using System.Linq;
using MyBox;
using UnityEngine;

namespace SensenToolkit
{
    public class ResolutionsSettingsDropdown : ADynamicSettingsDropdown
    {
        [SerializeField, ReadOnly] private DisplayService _displayService;
        [SerializeField, ReadOnly] private ScreenService _screenService;

        private void Awake()
        {
            _displayService = DisplayService.Instance;
            _screenService = ScreenService.Instance;
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
            return _screenService
            .GetUpdatedResolutionKeys()
            .Select(resolutionKey => new DynamicDropdownItemData()
            {
                Name = resolutionKey,
            });
        }
    }
}
#endif
