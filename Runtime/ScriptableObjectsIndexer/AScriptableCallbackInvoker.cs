using System.Collections.Generic;
using UnityEngine;

namespace SensenToolkit.Internal
{

    public abstract class AScriptableCallbackInvoker<T> : AScriptableCallbackInvokerBase
    {
        public abstract void Invoke(T subscriber);
        public override bool TryInvoke(ScriptableObject obj)
        {
            if (obj is T subscriber)
            {
                Invoke(subscriber);
                return true;
            }
            return false;
        }

    }
}
