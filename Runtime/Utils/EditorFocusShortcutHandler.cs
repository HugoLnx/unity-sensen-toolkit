using System.Collections;
using MyBox;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SensenToolkit
{
    public class EditorFocusShortcutHandler : ATransientSingleton<EditorFocusShortcutHandler>
    {
        [SerializeField, AutoProperty(AutoPropertyMode.Scene, allowEmpty: true)]
        private FreezeService _freezeService;

        [SerializeField, AutoProperty(AutoPropertyMode.Scene, allowEmpty: true)]
        private InputBlockService _inputBlock;

        private bool _focusedInEditor = false;
        private Coroutine _coroutine;

        private void OnEnable()
        {
            EnsureTickLoop();
        }

        public void EnsureTickLoop()
        {
#if UNITY_EDITOR
            this.TryStopCoroutine(ref _coroutine);
            _coroutine = StartCoroutine(TickLoop());
#endif
        }

        private IEnumerator TickLoop()
        {
            while (true)
            {
                yield return null;

                Keyboard keyboard = Keyboard.current;
                bool hasPressedFocusToggle = keyboard != null
                    && keyboard.ctrlKey.isPressed && keyboard.fKey.wasPressedThisFrame;

                Mouse mouse = Mouse.current;
                bool hasPressedFocusGameplay = mouse != null && mouse.leftButton.wasPressedThisFrame;

                if (hasPressedFocusToggle) EnsureEditorFocusAs(!_focusedInEditor);
                else if (hasPressedFocusGameplay) EnsureEditorFocusAs(false);
            }
        }

        private void EnsureEditorFocusAs(bool focusedInEditor)
        {
            if (_focusedInEditor == focusedInEditor) return;
            if (focusedInEditor)
            {
                _freezeService.HoldFreeze(this);
                _inputBlock.Hold(this);
            }
            else
            {
                _freezeService.ReleaseFreeze(this);
                _inputBlock.Release(this);
            }
            _focusedInEditor = focusedInEditor;
        }
    }
}
