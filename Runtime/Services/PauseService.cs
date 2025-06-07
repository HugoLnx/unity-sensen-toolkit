using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SensenToolkit
{
    public class PauseService : ATransientSingleton<PauseService>
    {
        [SerializeField] private bool _bindShortcuts = true;
        [SerializeField, AutoProperty(AutoPropertyMode.Scene)]
        private FreezeService _freezeService;
        [SerializeField, ReadOnly] private bool _isPaused = false;
        private readonly HashSet<Component> _blockers = new();

        public bool IsPaused => _isPaused && IsAllowedToPause;
        private bool IsAllowedToPause => _blockers.Count == 0;

        public event System.Action<bool> OnChanged;

        protected override void AwakeAny()
        {
            base.AwakeAny();
            SwitchPausedTo(false);
        }

        private void Update()
        {
            if (!_bindShortcuts) return;
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;

            bool pressedPause = keyboard.enterKey.wasPressedThisFrame ||
                                keyboard.pKey.wasPressedThisFrame ||
                                keyboard.escapeKey.wasPressedThisFrame;
            if (pressedPause) SwitchPausedTo(!_isPaused);
        }

        public void SwitchPausedTo(bool isPaused)
        {
            bool wasPaused = IsPaused;
            _isPaused = isPaused;
            RefreshPauseState(wasPaused);
        }

        public void BlockPause(Component blocker)
        {
            if (blocker == null) return;
            bool wasPaused = IsPaused;
            _blockers.Add(blocker);
            RefreshPauseState(wasPaused);
        }

        public void UnblockPause(Component blocker)
        {
            if (blocker == null) return;
            bool wasPaused = IsPaused;
            _blockers.Remove(blocker);
            RefreshPauseState(wasPaused);
        }

        private void RefreshPauseState(bool wasPaused)
        {
            bool isPaused = IsPaused;
            if (isPaused == wasPaused) return;

            if (isPaused) _freezeService.Freeze(this);
            else _freezeService.Unfreeze(this);

            OnChanged?.Invoke(isPaused);
        }
    }
}
