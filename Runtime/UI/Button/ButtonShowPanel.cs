using MyBox;
using SensenToolkit;
using UnityEngine;
using UnityEngine.UI;

namespace SensenToolkit
{
    public class ButtonShowPanel : MonoBehaviour
    {
        [SerializeField, AutoProperty] private Button _button;
        [SerializeField, MustBeAssigned] private PanelFadable _panel;

        private void Awake()
        {
            _button.onClick.AddListener(OnButtonClicked);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            _panel.Show();
        }
    }
}
