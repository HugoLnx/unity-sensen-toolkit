using System;
using System.Collections.Generic;
using UnityEngine;

namespace SensenToolkit.Internal
{
    public class SingletonAssignCallbacks<T>
    where T : MonoBehaviour
    {
        private class Callbacks
        {
            public Action<T> Assign = null;
            public Action<T> Unassign = null;

            public Callbacks(Action<T> assign, Action<T> unassign)
            {
                Assign = assign;
                Unassign = unassign;
            }
        }

        private Dictionary<object, Callbacks> _callbacksDict = new();
        private T _lastAssignedInstance = null;

        public void InvokeCallbacksIfChanged(T instance)
        {
            bool isSameInstance = _lastAssignedInstance == instance;
            if (isSameInstance) return;

            if (_lastAssignedInstance != null)
            {
                foreach (Callbacks callbacks in _callbacksDict.Values)
                {
                    callbacks.Unassign?.Invoke(_lastAssignedInstance);
                }
            }

            if (instance != null)
            {
                foreach (Callbacks callbacks in _callbacksDict.Values)
                {
                    callbacks.Assign?.Invoke(instance);
                }
            }

            _lastAssignedInstance = instance;
        }

        public void AddAssignListeners(object key, Action<T> assign = null, Action<T> unassign = null)
        {
            if (_callbacksDict.ContainsKey(key)) return;

            _callbacksDict[key] = new Callbacks(assign, unassign);
        }

        public void UnassignAndRemoveAssignListener(object key)
        {
            if (key == null || !_callbacksDict.TryGetValue(key, out Callbacks callbacks)) return;
            _callbacksDict.Remove(key);

            if (_lastAssignedInstance == null) return;
            callbacks.Unassign?.Invoke(_lastAssignedInstance);
        }

        public void Reset()
        {
            _callbacksDict.Clear();
            _lastAssignedInstance = null;
        }
    }
}
