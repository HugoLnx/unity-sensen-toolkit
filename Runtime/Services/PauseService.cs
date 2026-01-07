using System.Collections;
using MyBox;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SensenToolkit
{
    public class PauseService : ATransientSingleton<PauseService>
    {
        [Tooltip("Optional action reference for 'pause' action.")]
        [SerializeField] private DynamicInputActionReference _pauseAction;
        [Tooltip("Optional action reference for 'unpause' action.")]
        [SerializeField] private DynamicInputActionReference _unpauseAction;
        [Tooltip("If true, automatically bind 'pause' key shortcuts (P, Escape).")]
        [SerializeField] private bool _autobindKeyShortcuts = true;
        [SerializeField, ReadOnly] private bool _isPaused = false;
        private InputToolkitService _inputToolkit;
        private bool? _lastIsPaused = null;
        private MultiHolderHub _blockHolders;
        private MultiHolderHub BlockHolders => _blockHolders ??= CreateBlockHolders();

        public bool IsPaused => _isPaused && IsAllowedToPause;
        private bool IsAllowedToPause => !BlockHolders.IsHolding;

        public event System.Action<bool> OnChanged = delegate { };

        protected override void AwakeAny()
        {
            base.AwakeAny();
            SwitchPausedTo(false);
            _pauseAction.OnBindingAdd += action => action.performed += OnPauseActionPerformed;
            _pauseAction.OnBindingRemove += action => action.performed -= OnPauseActionPerformed;
            _unpauseAction.OnBindingAdd += action => action.performed += OnUnpauseActionPerformed;
            _unpauseAction.OnBindingRemove += action => action.performed -= OnUnpauseActionPerformed;
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
            if (_autobindKeyShortcuts)
            {
                StartCoroutine(AutoKeysShortcutBindingLoop());
            }
            _pauseAction.EnsureBinded();
            _unpauseAction.EnsureBinded();
        }

        protected override void OnDisableAny()
        {
            base.OnDisableAny();
            _pauseAction.EnsureUnbinded();
            _unpauseAction.EnsureUnbinded();
        }

        protected override void OnDestroyAny()
        {
            base.OnDestroyAny();
            InputToolkitService.UnassignAndRemoveListener(this);
        }

        public void SetActionCollection(IInputActionCollection2 actions)
        {
            _pauseAction.SetActionCollection(actions);
            _unpauseAction.SetActionCollection(actions);
        }

        private void OnPauseActionPerformed(InputAction.CallbackContext _)
            => SwitchPausedTo(true);

        private void OnUnpauseActionPerformed(InputAction.CallbackContext _)
            => SwitchPausedTo(false);

        private IEnumerator AutoKeysShortcutBindingLoop()
        {
            while (true)
            {
                yield return null; // Wait for the next frame

                Keyboard keyboard = Keyboard.current;
                if (keyboard == null) yield break;

                if (_isPaused)
                {
                    bool pressedUnpause = keyboard?.pKey?.wasPressedThisFrame == true;
                    if (pressedUnpause) SwitchPausedTo(false);
                }
                else
                {
                    bool pressedPause = keyboard?.pKey?.wasPressedThisFrame == true ||
                                        keyboard?.escapeKey?.wasPressedThisFrame == true;
                    if (pressedPause) SwitchPausedTo(true);
                }
            }
        }

        public void SwitchPausedTo(bool isPaused)
        {
            _isPaused = isPaused;
            RefreshPauseState();
        }

        private void RefreshPauseState()
        {
            bool isPaused = IsPaused;
            if (isPaused == _lastIsPaused) return;

            _lastIsPaused = isPaused;
            OnChanged.Invoke(isPaused);
        }

        public void BlockPause(Component blocker) => BlockHolders.Hold(blocker);
        public void UnblockPause(Component blocker) => BlockHolders.Release(blocker);
        private MultiHolderHub CreateBlockHolders() => new(onChanged: OnBlockersChanged);
        private void OnBlockersChanged(bool isBlocked) => RefreshPauseState();
    }
}
