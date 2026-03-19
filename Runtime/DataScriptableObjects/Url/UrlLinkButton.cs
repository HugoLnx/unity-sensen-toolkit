using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace SensenToolkit
{
    [RequireComponent(typeof(Button))]
    public class UrlLinkButton : MonoBehaviour
    {
        [Tooltip("If true you can directly input the url in the inspector, otherwise it will use the UrlSO")]
        [SerializeField] private bool _useLooseUrl = false;
        [ConditionalField(nameof(_useLooseUrl), inverse: true)]
        [SerializeField] private UrlSO _urlSo;
        [ConditionalField(nameof(_useLooseUrl))]
        [SerializeField] private string _urlString;
        [SerializeField, AutoProperty] private Button _button;

        private string Url => (_useLooseUrl ? _urlString : _urlSo != null ? _urlSo.name : string.Empty).Trim();

        private void Awake()
        {
            _button.onClick.AddListener(OnClick);
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(Url))
            {
                Debug.LogError($"Url is empty for {gameObject.name}", this);
            }
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            if (string.IsNullOrWhiteSpace(Url))
            {
                Debug.LogError($"Url is empty for {gameObject.name}", this);
                return;
            }
            Application.OpenURL(Url);
        }
    }
}
