using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SensenToolkit
{
    public class PanelsService : ATransientSingleton<PanelsService>
    {
        [Tooltip("Optional action reference for 'back' action.")]
        [SerializeField] private DynamicInputActionReference _backAction;
        [Tooltip("If true, automatically bind 'back' key shortcuts (Escape, Backspace, Right Mouse Button).")]
        [SerializeField] private bool _autobindKeyShortcuts = true;
        [Header("References")]
        [SerializeField, AutoProperty(AutoPropertyMode.Scene, allowEmpty: true)]
        private FreezeService _freezeService;
        private InputToolkitService _inputToolkit;

        private Stack<PanelFadable> _stack = new();
        public bool IsHoldingFocus => FocusHoldersHub.IsHolding;
        public bool CanGoBack => _stack.Count > 0;
        private MultiHolderHub _focusHoldersHub;
        private MultiHolderHub FocusHoldersHub => _focusHoldersHub ??= CreateFocusHoldersHub();

        public delegate void PanelBackEventHandler(PanelFadable previousTopPanel, PanelFadable topPanel);
        public event PanelBackEventHandler OnBack = delegate { };
        public event Action<bool> OnHoldingFocusChanged = delegate { };

        protected override void AwakeAny()
        {
            base.AwakeAny();
            _backAction.OnBindingAdd += action => action.performed += OnBackActionPerformed;
            _backAction.OnBindingRemove += action => action.performed -= OnBackActionPerformed;
            InputToolkitService.AddAssignListener(this,
                forceInstance: false,
                assign: (s) =>
                {
                    _inputToolkit = s;
                    s.BindActionCollection(SetActionCollection);
                },
                unassign: (s) =>
                {
                    s.UnbindActionCollection(SetActionCollection);
                }
            );
        }

        private void OnEnable()
        {
            if (_autobindKeyShortcuts) StartCoroutine(BindKeyShortcuts());
            _backAction.EnsureBinded();
        }

        protected override void OnDisableAny()
        {
            base.OnDisableAny();
            _backAction.EnsureUnbinded();
        }

        protected override void OnDestroyAny()
        {
            base.OnDestroyAny();
            InputToolkitService.UnassignAndRemoveListener(this);
        }

        public void PushTop(PanelFadable panel)
        {
            if (panel == null) return;
            _stack.Push(panel);
            panel.OnHidden += OnPanelHidden;
        }

        public void GoBack(PanelFadable currentPanel = null)
        {
            if (currentPanel != null && _stack.Count > 0 && _stack.Peek() != currentPanel)
            {
                return;
            }
            PanelFadable previousTopPanel = PopTop();
            PanelFadable topPanel = _stack.Count > 0 ? _stack.Peek() : null;
            OnBack.Invoke(previousTopPanel, topPanel);
        }

        public void AddFocusHolder(PanelFadable panel) => FocusHoldersHub.Hold(panel);
        public void RemoveFocusHolder(PanelFadable panel) => FocusHoldersHub.Release(panel);

        public void SetActionCollection(IInputActionCollection2 actions)
        {
            _backAction.SetActionCollection(actions);
        }

        private void OnPanelHidden(PanelFadable panel)
        {
            PanelFadable topPanel;
            do
            {
                topPanel = PopTop();
            } while (topPanel != null && topPanel != panel);
        }

        private void OnBackActionPerformed(InputAction.CallbackContext context)
        {
            if (!CanGoBack) return;
            GoBack();
        }

        private IEnumerator BindKeyShortcuts()
        {
            while (true)
            {
                yield return null; // Wait for the next frame
                if (!CanGoBack) continue;

                Keyboard keyboard = Keyboard.current;
                Mouse mouse = Mouse.current;
                bool isBackPressedThisFrame =
                    keyboard?.escapeKey?.wasPressedThisFrame == true
                    || keyboard?.backspaceKey?.wasPressedThisFrame == true
                    || mouse?.rightButton?.wasPressedThisFrame == true;

                if (!isBackPressedThisFrame) continue;
                GoBack();
            }
        }

        private PanelFadable PopTop()
        {
            if (_stack.Count == 0) return null;
            PanelFadable previousTopPanel = _stack.Pop();
            previousTopPanel.OnHidden -= OnPanelHidden;
            previousTopPanel.Hide();
            return previousTopPanel;
        }

        private MultiHolderHub CreateFocusHoldersHub()
            => new(onChanged: OnHoldingFocusChangedReaction);

        private void OnHoldingFocusChangedReaction(bool _)
        {
            if (_freezeService != null)
            {
                if (IsHoldingFocus) _freezeService.HoldFreeze(this);
                else _freezeService.ReleaseFreeze(this);
            }
            OnHoldingFocusChanged?.Invoke(IsHoldingFocus);
        }
    }
}
