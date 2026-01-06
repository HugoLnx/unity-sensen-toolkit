using System;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace SensenToolkit
{
    public class KeyRebindingOverlay : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private Color _actionColor = Color.purple;
        [SerializeField] private string _actionSizeTagValue = "1.5em";
        [Header("Localization")]
        [SerializeField] private LocalizedString _listeningTextI18n;

        [Header("References")]
        [SerializeField, MustBeAssigned] private TMP_Text _listeningText;
        [SerializeField, MustBeAssigned] private TMP_Text _keyText;
        [SerializeField, MustBeAssigned] private AnimatedUIBlink _keyBlink;
        [SerializeField, MustBeAssigned] private TMP_Text _errorText;
        [SerializeField, MustBeAssigned] private TMP_Text _infoCancelKeyText;
        [SerializeField, MustBeAssigned] private FadableContent _content;
        [SerializeField, MustBeAssigned] private FadableContent _errorContent;
        [SerializeField, AutoProperty] private PanelFadable _panel;
        private InputToolkitService InputToolkit => InputToolkitService.GetInstanceIfExists();
        private Coroutine _hideCoroutine;

        private void Start()
        {
            _panel.InstantHide();
            _errorContent.InstantHide();
            _content.InstantShow();
        }

        public void ShowListening(string actionName)
        {
            if (InputToolkit != null) InputToolkit.PauseInput();
            _errorContent.InstantHide();
            UpdateInfoCancelText();
            ResetError();
            _content.ApplyWhenHidden(() =>
            {
                string keyNameText = $"<b><size={_actionSizeTagValue}><color={_actionColor.ToHex()}>{actionName}</color></size></b>";
                _listeningText.text = _listeningTextI18n.GetLocalizedString(keyNameText);
                UpdateKeyName(null);
            });
            if (_panel.IsVisible) _content.HideAndReshowFading();
            else _content.InstantShow();
            _panel.Show();
        }

        private void UpdateInfoCancelText()
        {
            KeyRebindingLocalizedTexts texts = KeyRebindingService.Instance.LocalizedTexts;
            _infoCancelKeyText.text = $"({texts.GetRebindingPressToCancelText("Esc")})";
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
                _errorText.text = errorMessage;
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
            if (InputToolkit != null) InputToolkit.ResumeInput();
            _panel.Hide();
        }

        private void ResetError()
        {
            _errorContent.HideFading();
        }
    }
}
