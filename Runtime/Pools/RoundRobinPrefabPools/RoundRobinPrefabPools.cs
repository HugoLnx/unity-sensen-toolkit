using UnityEngine;
using MyBox;
using System.Collections.Generic;
using System.Collections;
using EasyButtons;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SensenToolkit
{
    [System.Serializable]
    public class PredefinedRRPoolConfig
    {
        public Component Prefab;
        public Transform Container;
        public int Size;
    }

    public class RoundRobinPrefabPools : ATransientSingleton<RoundRobinPrefabPools>
    {
        [SerializeField, InitializationField] private PredefinedRRPoolConfig[] _configs;
        private Dictionary<GameObject, RRPrefabPool> _pools = new();

        private void Start()
        {
            foreach (PredefinedRRPoolConfig config in _configs)
            {
                if (config.Prefab == null || config.Size <= 0) continue;
                EnsureContainerFor(config);
                var pool = new RRPrefabPool(config.Container, config.Prefab, config.Size);
                foreach (Transform child in config.Container)
                {
                    if (child == null) continue;
                    pool.AddInstance(child);
                }
                pool.EnsureInstances();
                _pools.Add(config.Prefab.gameObject, pool);
            }
        }

        public T GetInstanceOf<T>(T prefab) where T : Component
        {
            if (!_pools.TryGetValue(prefab.gameObject, out RRPrefabPool pool))
            {
                throw new System.ArgumentException($"No round robin pool found for prefab {prefab.name}");
            }

            Component instance = pool.GetInstance();
            var typedInstance = instance as T;
            if (typedInstance == null) typedInstance = instance.GetComponent<T>();
            if (typedInstance == null)
            {
                throw new System.ArgumentException($"Prefab {prefab.name} is not of type {typeof(T).Name}.");
            }
            return typedInstance;
        }

        private void EnsureContainerFor(PredefinedRRPoolConfig config)
        {
            if (config.Container != null) return;
            config.Container = new GameObject($"{config.Prefab.name} Pool").transform;
            config.Container.SetParent(this.transform);
        }

#if UNITY_EDITOR
        [Button(Mode = ButtonMode.DisabledInPlayMode)]
        private void PrecreateInstances()
        {
            this.transform.DestroyAllChildren(immediate: true);
            foreach (PredefinedRRPoolConfig config in _configs)
            {
                if (config.Prefab == null || config.Size <= 0) continue;
                EnsureContainerFor(config);

                for (int i = 0; i < config.Size; i++)
                {
                    var instance = PrefabUtility.InstantiatePrefab(config.Prefab, config.Container) as Component;
                    instance.name = $"[{i}] {config.Prefab.name}";
                    instance.gameObject.SetActive(false);
                }
            }
        }
#endif
    }
}
