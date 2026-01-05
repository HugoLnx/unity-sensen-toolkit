using EasyButtons;
using MyBox;
using SensenToolkit.Internal;
using UnityEngine;

namespace SensenToolkit
{
    public class ScriptableObjectsService : APermanentSingleton<ScriptableObjectsService>, IAppCore_BootAwake_Internal
    {
        [SerializeField, AutoProperty(AutoPropertyMode.Asset)] private ScriptableObjectsIndexer _indexer;

        public static void AppCore_BootAwake_Internal() => Initialize();

        private static void Initialize()
        {
            ScriptableObjectsService instance = GetInstanceIfExists();
            if (instance == null)
            {
                Debug.LogWarning($"[{nameof(ScriptableObjectsService)}] Instance not found during {nameof(Initialize)}. Aborting initialization.");
                return;
            }

            if (instance._indexer == null)
            {
                Debug.LogWarning($"[{nameof(ScriptableObjectsService)}] indexer is not assigned. Aborting initialization.");
                return;
            }
            instance.InvokeCallbacks(new ScriptableCallbackInvoker_OnBoot_Internal());
            instance.InvokeCallbacks(new ScriptableCallbackInvoker_OnBoot());
        }

        public void InvokeCallbacks(AScriptableCallbackInvokerBase invoker)
        {
            foreach (ScriptableObject obj in _indexer.All)
            {
                invoker.TryInvoke(obj);
            }
        }

        [Button]
        private void RefreshIndexer()
        {
            if (_indexer != null)
            {
                _indexer.RefreshIndex();
            }
        }
    }
}
