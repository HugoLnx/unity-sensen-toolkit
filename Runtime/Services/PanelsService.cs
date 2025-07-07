using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SensenToolkit
{
    public class PanelsService : ATransientSingleton<PanelsService>
    {
        [SerializeField] private bool _bindAutoShortcuts = true;
        private Stack<PanelFadable> _stack = new();

        private static bool IsBackPressedThisFrame =>
            Keyboard.current?.escapeKey?.wasPressedThisFrame == true
            || Keyboard.current?.backspaceKey?.wasPressedThisFrame == true
            || Mouse.current?.rightButton?.wasPressedThisFrame == true;

        public delegate void PanelBackEventHandler(PanelFadable previousTopPanel, PanelFadable topPanel);
        public event PanelBackEventHandler OnBack = delegate { };

        private void OnEnable()
        {
            if (_bindAutoShortcuts) StartCoroutine(BindAutoShortcuts());
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

        private void OnPanelHidden(PanelFadable panel)
        {
            PanelFadable topPanel;
            do
            {
                topPanel = PopTop();
            } while (topPanel != null && topPanel != panel);
        }

        private IEnumerator BindAutoShortcuts()
        {
            while (true)
            {
                yield return null; // Wait for the next frame
                if (_stack.Count == 0) continue;

                if (!IsBackPressedThisFrame) continue;
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
    }
}
