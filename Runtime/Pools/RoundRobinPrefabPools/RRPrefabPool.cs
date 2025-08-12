using UnityEngine;
using System.Collections.Generic;

namespace SensenToolkit
{
    public class RRPrefabPool
    {
        private Transform _container;
        private Component _prefab;
        private List<Component> _instances;
        private int _currentInx = 0;
        private int _poolSize;

        public RRPrefabPool(
            Transform container,
            Component prefab,
            int size
        )
        {
            _prefab = prefab;
            _container = container;
            _poolSize = size;
            _instances = new List<Component>(size);
        }

        public void AddInstance(Component instance)
        {
            Assertx.IsNotNull(instance);
            Assertx.IsTrue(instance.transform.parent == _container, "Instance parent must be the container.");
            if (_instances.Count >= _poolSize) return;

            _instances.Add(instance);
            instance.gameObject.SetActive(false);
        }

        public void EnsureInstances()
        {
            for (int i = _instances.Count; i < _poolSize; i++)
            {
                var obj = GameObject.Instantiate(_prefab.gameObject, _container);
                obj.name = $"[{i}] {_prefab.name}";
                obj.SetActive(false);
                _instances.Add(obj.transform);
            }
        }

        public Component GetInstance()
        {
            if (_instances.Count == 0)
            {
                return null;
            }

            Component instance = _instances[_currentInx];
            _currentInx = (_currentInx + 1) % _poolSize;
            if (!instance.gameObject.activeInHierarchy)
            {
                instance.gameObject.SetActive(true);
            }
            return instance;
        }
    }
}
