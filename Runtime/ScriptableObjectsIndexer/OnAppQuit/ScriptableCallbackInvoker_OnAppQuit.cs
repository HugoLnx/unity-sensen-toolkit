using UnityEngine;
using SensenToolkit;

namespace SensenToolkit.Internal
{
    public class ScriptableCallbackInvoker_OnAppQuit : AScriptableCallbackInvoker<IScriptableCallbackSubscriber_OnAppQuit>
    {
        public override void Invoke(IScriptableCallbackSubscriber_OnAppQuit subscriber)
        {
            subscriber.ScriptableCallback_OnAppQuit();
        }
    }
}
