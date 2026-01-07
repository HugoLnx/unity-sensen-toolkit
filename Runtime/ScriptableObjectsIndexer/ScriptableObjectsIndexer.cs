using System;
using System.Collections.Generic;
using System.Linq;
using EasyButtons;
using SensenToolkit.Internal;


#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace SensenToolkit
{
    [System.Serializable]
    public class CallbackInvokerSubscribers
    {
        public string InvokerTypeName;
        public List<ScriptableObject> Subscribers;
    }

    [CreateAssetMenu(menuName = "Sensen/SOIndexer")]
    public class ScriptableObjectsIndexer : ScriptableObject
    {
        [SerializeField] private List<ScriptableObject> _all = new();
        [SerializeField] private List<CallbackInvokerSubscribers> _callbackSubscribers = new();
        [NonSerialized] private Dictionary<string, CallbackInvokerSubscribers> _callbackSubscribersDict = new();
        private Dictionary<string, CallbackInvokerSubscribers> CallbackSubscribersDict => EnsureCallbackSubscribersDict();

        [NonSerialized]
        private List<AScriptableCallbackInvokerBase> _callbackInvokers = new()
        {
            new ScriptableCallbackInvoker_OnAppAwake_Internal(),
            new ScriptableCallbackInvoker_OnAppAwake(),
            new ScriptableCallbackInvoker_OnAppQuit(),
        };

        public IReadOnlyList<ScriptableObject> All => _all;


#if UNITY_EDITOR
        private void OnEnable()
        {
            if (Application.isPlaying) return;
            RefreshIndex();
        }
        private void OnValidate()
        {
            if (Application.isPlaying) return;
            RefreshIndex();
        }
#endif

        public void InvokeCallbacks<T>() where T : AScriptableCallbackInvokerBase, new()
        {
            if (!CallbackSubscribersDict.TryGetValue(typeof(T).Name, out CallbackInvokerSubscribers data))
            {
                Debug.LogWarning($"[{nameof(ScriptableObjectsIndexer)}] No subscribers found for invoker type {typeof(T).Name}. Aborting callbacks invocation.");
                return;
            }

            AScriptableCallbackInvokerBase invoker = new T();
            foreach (ScriptableObject subscriber in data.Subscribers)
            {
                invoker.TryInvoke(subscriber);
            }
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [Button]
        public void RefreshIndex()
        {
#if UNITY_EDITOR
            _all.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:ScriptableObject"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ScriptableObject so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (so != null && so != this)
                {
                    _all.Add(so);
                }
            }

            _callbackSubscribers.Clear();
            foreach (AScriptableCallbackInvokerBase invoker in _callbackInvokers)
            {
                CallbackInvokerSubscribers data = new()
                {
                    InvokerTypeName = invoker.GetType().Name,
                    Subscribers = new List<ScriptableObject>()
                };

                foreach (ScriptableObject so in _all)
                {
                    if (invoker.CanInvoke(so))
                    {
                        data.Subscribers.Add(so);
                    }
                }

                _callbackSubscribers.Add(data);
            }
            EditorUtility.SetDirty(this);
#endif
        }

        private Dictionary<string, CallbackInvokerSubscribers> EnsureCallbackSubscribersDict()
        {
            if (_callbackSubscribersDict != null
                && _callbackSubscribersDict.Count == _callbackSubscribers.Count)
            {
                return _callbackSubscribersDict;
            }
            _callbackSubscribersDict.Clear();

            _callbackSubscribersDict = _callbackSubscribers.ToDictionary(
                data => data.InvokerTypeName,
                data => data
            );

            return _callbackSubscribersDict;
        }
    }
}
