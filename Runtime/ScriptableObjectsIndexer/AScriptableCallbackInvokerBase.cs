using UnityEngine;

namespace SensenToolkit.Internal
{
    [System.Serializable]
    public abstract class AScriptableCallbackInvokerBase
    {
        public abstract bool TryInvoke(ScriptableObject obj);
    }
}
