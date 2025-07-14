#if DOTWEEN
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace SensenToolkit
{
    public class ButtonBackPanel : MonoBehaviour
    {
        [SerializeField, AutoProperty(AutoPropertyMode.Parent)]
        private PanelFadable _parentPanel;
        [SerializeField, AutoProperty(AutoPropertyMode.Scene)]
        private PanelsService _panelsService;
        [SerializeField, AutoProperty(AutoPropertyMode.Children)]
        private Button _button;

        private void OnEnable()
        {
            _button.onClick.AddListener(OnButtonClicked);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            _panelsService.GoBack(_parentPanel);
        }
    }
}
#endif
