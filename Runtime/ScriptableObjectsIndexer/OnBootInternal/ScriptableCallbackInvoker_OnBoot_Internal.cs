using UnityEngine;

namespace SensenToolkit.Internal
{
    public class ScriptableCallbackInvoker_OnBoot_Internal : AScriptableCallbackInvoker<IScriptableCallbackSubscriber_OnBoot_Internal>
    {
        public override void Invoke(IScriptableCallbackSubscriber_OnBoot_Internal subscriber)
        {
            subscriber.ScriptableCallback_OnBoot_Internal();
        }
    }
}
