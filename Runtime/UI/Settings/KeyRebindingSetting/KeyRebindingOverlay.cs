using MyBox;
using TMPro;
using UnityEngine;

namespace SensenToolkit
{
    public class KeyRebindingOverlay : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private Color _actionColor = Color.purple;
        [SerializeField] private string _actionSizeTagValue = "1.5em";

        [Header("References")]
        [SerializeField, MustBeAssigned] private TMP_Text _listeningText;
        [SerializeField, MustBeAssigned] private TMP_Text _keyText;
        [SerializeField, MustBeAssigned] private AnimatedUIBlink _keyBlink;
        [SerializeField, MustBeAssigned] private TMP_Text _errorText;
        [SerializeField, MustBeAssigned] private FadableContent _content;
        [SerializeField, MustBeAssigned] private FadableContent _errorContent;
        [SerializeField, AutoProperty] private PanelFadable _panel;
        private Coroutine _hideCoroutine;

        private void Start()
        {
            _panel.InstantHide();
            _errorContent.InstantHide();
            _content.InstantShow();
        }

        public void ShowListening(string actionName)
        {
            _errorContent.InstantHide();
            ResetError();
            _content.ApplyWhenHidden(() =>
            {
                _listeningText.text = $"Listening key/button for <b><size={_actionSizeTagValue}><color={_actionColor.ToHex()}>{actionName}</color></size></b>";
                UpdateKeyName(null);
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
            this.TryStopCoroutine(ref _hideCoroutine);
            if (delay <= 0f) RawHideNow();
            else
            {
                _hideCoroutine = StartCoroutine(
                    Coroutinesx.WaitAndExecute(delay, OnDelayedHide, unscaled: true));
            }
        }

        private void OnDelayedHide()
        {
            RawHideNow();
        }

        private void RawHideNow()
        {
            _panel.Hide();
        }

        private void ResetError()
        {
            _errorContent.HideFading();
        }
    }
}
