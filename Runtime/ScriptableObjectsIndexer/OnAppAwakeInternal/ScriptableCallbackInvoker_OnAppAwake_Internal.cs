using UnityEngine;

namespace SensenToolkit.Internal
{
    public class ScriptableCallbackInvoker_OnAppAwake_Internal : AScriptableCallbackInvoker<IScriptableCallbackSubscriber_OnAppAwake_Internal>
    {
        public override void Invoke(IScriptableCallbackSubscriber_OnAppAwake_Internal subscriber)
        {
            subscriber.ScriptableCallback_OnAppAwake_Internal();
        }
    }
}
