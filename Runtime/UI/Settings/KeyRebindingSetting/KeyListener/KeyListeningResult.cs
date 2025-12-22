using UnityEngine.InputSystem;

namespace SensenToolkit.InputRebinding.Internal
{
    public enum KeyListeningResultType
    {
        Listened,
        Canceled,
    }

    public struct KeyListeningResult
    {
        public InputDevice Device;
        public KeyListeningResultType Type;
        public string NewPath;
        public InputAction Action;

        public bool HasListened => Type == KeyListeningResultType.Listened;
        public bool HasCanceled => Type == KeyListeningResultType.Canceled;
    }
}
