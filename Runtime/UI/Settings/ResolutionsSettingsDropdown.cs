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

        protected override void Awake()
        {
            base.Awake();
            _displayService = DisplayService.Instance;
            _screenService = ScreenService.Instance;
            EnsureDropdownItems();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _displayService.OnDisplaysChanged += UpdateDropdownOptions;
        }

        private void OnDisable()
        {
            _displayService.OnDisplaysChanged -= UpdateDropdownOptions;
        }

        private void UpdateDropdownOptions(List<DisplayInfo> _)
        {
            RecreateDropdownItems();
        }

        public override IEnumerable<DynamicDropdownItemData> GetDropdownItems()
        {
            if (_screenService == null) yield break;

            foreach (string resolutionKey in _screenService.GetUpdatedResolutionKeys())
            {
                yield return new DynamicDropdownItemData()
                {
                    Name = resolutionKey,
                    Key = resolutionKey
                };
            }
        }
    }
}
#endif
