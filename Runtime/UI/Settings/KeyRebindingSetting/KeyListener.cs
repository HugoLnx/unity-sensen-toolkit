using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputActionRebindingExtensions;

namespace SensenToolkit
{
    public enum KeyListeningResultType
    {
        Success,
        Canceled,
    }

    public struct KeyListeningResult
    {
        public InputDevice Device;
        public KeyListeningResultType Type;
        public string NewPath;
        public InputAction Action;

        public bool IsSuccess => Type == KeyListeningResultType.Success;
        public bool IsCanceled => Type == KeyListeningResultType.Canceled;
    }

    public class KeyListener
    {
        private InputAction _blankAction;
        private bool _cancelThroughEscape;
        private bool _ignoreMouseDelta;

        public KeyListener(bool cancelThroughEscape = true, bool ignoreMouseDelta = true)
        {
            _blankAction = new InputAction(type: InputActionType.Button);
            _blankAction.Disable();
            _cancelThroughEscape = cancelThroughEscape;
            _ignoreMouseDelta = ignoreMouseDelta;
        }

        public async UniTask<KeyListeningResult> ListenToKey(float timeout = 10f)
        {
            RebindingOperation op = _blankAction
            .PerformInteractiveRebinding()
            .WithRebindAddingNewBinding()
            .WithTimeout(timeout);
            if (_cancelThroughEscape)
            {
                op = op
                .WithCancelingThrough("<Keyboard>/escape")
                .WithControlsExcluding("<Keyboard>/escape");
            }
            if (_ignoreMouseDelta)
            {
                op = op
                .WithControlsExcluding("<Pointer>/delta")
                .WithControlsExcluding("<Pointer>/position")
                .WithControlsExcluding("<Mouse>/delta")
                .WithControlsExcluding("<Mouse>/position");
            }

            KeyListeningResult result = new();
            KeyListeningResultType? resultType = null;

            op = op
            .OnApplyBinding((operation, newPath) =>
            {
                result.Device = operation.selectedControl.device;
                result.NewPath = newPath;
            })
            .OnComplete(operation =>
            {
                resultType = KeyListeningResultType.Success;
            })
            .OnCancel(operation =>
            {
                resultType = KeyListeningResultType.Canceled;
            })
            .Start();

            await UniTask.WaitUntil(() => resultType.HasValue);
            result.Type = resultType.Value;
            op.Dispose();
            return result;
        }
    }
}
