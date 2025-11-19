using MyBox;
using TMPro;
using UnityEngine;

namespace SensenToolkit
{
    public class KeyRebindingOverlay : MonoBehaviour
    {
        [SerializeField, MustBeAssigned] private TMP_Text _listeningText;
        [SerializeField, MustBeAssigned] private TMP_Text _keyText;
        [SerializeField, MustBeAssigned] private AnimatedUIBlink _keyBlink;
        [SerializeField, MustBeAssigned] private TMP_Text _errorText;
        [SerializeField, MustBeAssigned] private FadableContent _content;
        [SerializeField, MustBeAssigned] private FadableContent _errorContent;
        [SerializeField, AutoProperty] private PanelFadable _panel;

        private void Start()
        {
            _panel.InstantHide();
            _errorContent.InstantHide();
            _content.InstantShow();
        }

        public void ShowListening(string actionName)
        {
            _errorContent.InstantHide();
            ResetKey();
            _content.ApplyWhenHidden(() =>
            {
                _listeningText.text = $"Listening key/button for \"{actionName}\"";
            });
            if (_panel.IsVisible) _content.HideAndReshowFading();
            else _content.InstantShow();
            _panel.Show();
        }

        public void UpdateKeyName(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                _keyText.text = "...";
                _keyBlink.EnsureBlink();
            }
            else
            {
                _keyText.text = text;
                _keyBlink.StopBlink();
            }
        }

        public void ShowError(string errorMessage)
        {
            _errorContent.ApplyWhenHidden(() =>
            {
                _errorText.text = $"Error: {errorMessage}";
            });
            _errorContent.HideAndReshowFading();
        }

        public void Hide(float delay = 0f)
        {
            StopAllCoroutines();
            if (delay <= 0f) _panel.Hide();
            else StartCoroutine(Coroutinesx.WaitAndExecute(delay, () => _panel.Hide()));
        }

        private void ResetKey()
        {
            ResetError();
            UpdateKeyName(null);
        }

        private void ResetError()
        {
            _errorContent.HideFading();
        }
    }
}
