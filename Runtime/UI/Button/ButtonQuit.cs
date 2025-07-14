using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace SensenToolkit
{
    public class ButtonQuit : MonoBehaviour
    {
        [SerializeField, AutoProperty] private Button _button;

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
            Application.Quit();
        }
    }
}
