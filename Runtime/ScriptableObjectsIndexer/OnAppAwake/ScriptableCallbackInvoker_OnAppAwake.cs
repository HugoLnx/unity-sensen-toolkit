using UnityEngine;
using SensenToolkit;

namespace SensenToolkit.Internal
{
    public class ScriptableCallbackInvoker_OnAppAwake : AScriptableCallbackInvoker<IScriptableCallbackSubscriber_OnAppAwake>
    {
        public override void Invoke(IScriptableCallbackSubscriber_OnAppAwake subscriber)
        {
            subscriber.ScriptableCallback_OnAppAwake();
        }
    }
}
