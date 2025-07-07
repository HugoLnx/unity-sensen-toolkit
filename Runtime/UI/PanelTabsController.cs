#if DOTWEEN
using MyBox;
using UnityEngine;

namespace SensenToolkit
{
    public class PanelTabsController : MonoBehaviour
    {
        [SerializeField, AutoProperty(AutoPropertyMode.Children)]
        private PanelTabLink[] _links;

        [SerializeField, AutoProperty(AutoPropertyMode.Children)]
        private RadioButtonGroup _tabGroup;

        private void OnEnable()
        {
            _tabGroup.OnStateChanged += OnTabStateChanged;
        }

        private void OnDisable()
        {
            _tabGroup.OnStateChanged -= OnTabStateChanged;
        }

        private void OnTabStateChanged()
        {
            RadioButton activeBtn = _tabGroup.ActiveButton;
            foreach (PanelTabLink cfg in _links)
            {
                if (cfg.Button == activeBtn) cfg.Panel.Show();
                else cfg.Panel.Hide();
            }
        }
    }
}
#endif
