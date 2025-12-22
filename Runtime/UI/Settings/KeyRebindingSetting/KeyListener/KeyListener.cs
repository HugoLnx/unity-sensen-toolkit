using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputActionRebindingExtensions;

namespace SensenToolkit.InputRebinding.Internal
{
    public class KeyListener
    {
        private InputAction _blankAction;
        private bool _cancelThroughEscape;
        private IEnumerable<string> _ignoreBindingPaths;

        public KeyListener(
            bool cancelThroughEscape = true,
            IEnumerable<string> ignoreBindingPaths = null
        )
        {
            _blankAction = new InputAction(type: InputActionType.Button);
            _blankAction.Disable();
            _cancelThroughEscape = cancelThroughEscape;
            _ignoreBindingPaths = ignoreBindingPaths ?? new string[0];
        }

        public async UniTask<KeyListeningResult> ListenToKey(InputAction action, float timeout = 10f)
        {
            RebindingOperation op = _blankAction
            .PerformInteractiveRebinding()
            .WithRebindAddingNewBinding()
            .WithTimeout(timeout);
            if (_cancelThroughEscape)
            {
                op = op
                .WithCancelingThrough(InputConstants.ESCAPE_KEY_PATH)
                .WithControlsExcluding(InputConstants.ESCAPE_KEY_PATH);
            }

            foreach (string bindingPath in _ignoreBindingPaths)
            {
                op = op.WithControlsExcluding(bindingPath);
            }

            KeyListeningResult result = new()
            {
                Action = action,
            };
            KeyListeningResultType? resultType = null;

            op = op
            .OnApplyBinding((operation, newPath) =>
            {
                result.Device = operation.selectedControl.device;
                result.NewPath = newPath;
            })
            .OnComplete(operation =>
            {
                resultType = KeyListeningResultType.Listened;
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
