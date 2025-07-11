using System;
using EasyButtons;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace SensenToolkit
{
    public class RadioButton : MonoBehaviour
    {
        [SerializeField] private bool _shouldDeactivateOnClick = false;
        [Header("Inactive Colors")]
        [SerializeField] private ColorBlock _inactiveColors;
        [Header("Active Colors")]
        [SerializeField] private ColorBlock _activeColors;
        [SerializeField, AutoProperty] private Button _btn;

        public bool IsActive { get; private set; }
        public bool ShouldDeactivateOnClick
        {
            get => _shouldDeactivateOnClick;
            set => _shouldDeactivateOnClick = value;
        }
        public event Action<RadioButton, bool> OnStateChanged;

        private void OnEnable()
        {
            _btn.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            _btn.onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            if (IsActive)
            {
                if (_shouldDeactivateOnClick) SwitchTo(false);
                else return;
            }
            else
            {
                SwitchTo(true);
            }
        }

        public void SwitchTo(bool turnOn, bool callbacks = true)
        {
            bool hasChanged = IsActive != turnOn;
            IsActive = turnOn;
            RefreshButtonState();
            if (callbacks && hasChanged) OnStateChanged?.Invoke(this, IsActive);
        }

        public void SetInteractive(bool interactive)
        {
            _btn.interactable = interactive;
            if (!interactive) SwitchTo(false, callbacks: false);
        }

        public void Reset()
        {
            SwitchTo(false, callbacks: false);
            RefreshButtonState();
        }

        private void RefreshButtonState()
        {
            if (IsActive) StyleAsActive();
            else StyleAsInactive();
        }

        [Button]
        private void StyleAsActive()
        {
            _btn.colors = _activeColors;
        }

        [Button]
        private void StyleAsInactive()
        {
            _btn.colors = _inactiveColors;
        }
    }
}
