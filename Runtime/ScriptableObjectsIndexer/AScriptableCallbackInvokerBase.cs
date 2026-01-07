using UnityEngine;

namespace SensenToolkit.Internal
{
    [System.Serializable]
    public abstract class AScriptableCallbackInvokerBase
    {
        public abstract bool CanInvoke(ScriptableObject obj);
        public abstract bool TryInvoke(ScriptableObject obj);
    }
}
