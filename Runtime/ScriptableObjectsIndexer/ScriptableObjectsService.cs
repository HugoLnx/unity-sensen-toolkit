using System;
using EasyButtons;
using MyBox;
using SensenToolkit.Internal;
using UnityEngine;

namespace SensenToolkit
{
    public class ScriptableObjectsService : APermanentSingleton<ScriptableObjectsService>,
        IAppCore_AppAwake_Internal,
        IAppCore_AppAwake,
        IAppCore_AppQuit
    {
        [SerializeField, AutoProperty(AutoPropertyMode.Asset)] private ScriptableObjectsIndexer _indexer;

        public static void AppCore_AppAwake_Internal()
            => StaticInvokeCallbacks<ScriptableCallbackInvoker_OnAppAwake_Internal>();

        public static void AppCore_AppAwake()
            => StaticInvokeCallbacks<ScriptableCallbackInvoker_OnAppAwake>();

        public static void AppCore_AppQuit()
            => StaticInvokeCallbacks<ScriptableCallbackInvoker_OnAppQuit>();

        public static void StaticInvokeCallbacks<T>() where T : AScriptableCallbackInvokerBase, new()
        {
            ScriptableObjectsService instance = InstanceLookup();
            if (instance == null) return;
            instance._indexer.InvokeCallbacks<T>();
        }

        [Button]
        private void RefreshIndexer()
        {
            if (_indexer != null)
            {
                _indexer.RefreshIndex();
            }
        }

        private static ScriptableObjectsService InstanceLookup()
        {
            ScriptableObjectsService instance = GetInstanceIfExists();
            if (instance == null)
            {
                Debug.LogWarning($"[{nameof(ScriptableObjectsService)}] Instance not found. Aborting callbacks calls.");
                return null;
            }

            if (instance._indexer == null)
            {
                Debug.LogWarning($"[{nameof(ScriptableObjectsService)}] indexer is not assigned. Aborting callbacks calls.");
                return null;
            }

            return instance;
        }
    }
}
