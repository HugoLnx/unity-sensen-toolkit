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

                Keyboard keyboard = Keyboard.current;
                Mouse mouse = Mouse.current;
                if (keyboard == null) continue;

                if (!IsBackPressedThisFrame) continue;
                PopTop();
            }
        }

        private PanelFadable PopTop()
        {
            if (_stack.Count == 0) return null;
            PanelFadable topPanel = _stack.Pop();
            topPanel.OnHidden -= OnPanelHidden;
            topPanel.Hide();
            return topPanel;
        }
    }
}
