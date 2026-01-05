using UnityEngine;

namespace SensenToolkit.Internal
{
    public class ScriptableCallbackInvoker_OnBoot : AScriptableCallbackInvoker<IScriptableCallbackSubscriber_OnBoot>
    {
        public override void Invoke(IScriptableCallbackSubscriber_OnBoot subscriber)
        {
            subscriber.ScriptableCallback_OnBoot();
        }
    }
}
