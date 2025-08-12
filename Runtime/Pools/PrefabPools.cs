using System;
using System.Collections.Generic;
using EasyButtons;
using MyBox;
using SensenToolkit;
using UnityEditor;
using UnityEngine;

namespace SensenToolkit
{
    [Serializable]
    public struct PredefinedPoolConfig
    {
        public Component Prefab;
        public PoolConfig Config;
    }

    public struct PoolNode
    {
        public Component Prefab;
        public SimpleExpandablePool<Component> Pool;
    }

    public class PrefabPools : ATransientSingleton<PrefabPools>
    {
        [SerializeField]
        private PoolConfig _defaultConfig = new()
        {
            MinSize = 15,
            MaxCreations = 50,
            Prefill = true,
            AutoDeactivate = true
        };
        [SerializeField, InitializationField] private PredefinedPoolConfig[] _predefinedPoolsConfig;
        private Dictionary<GameObject, PoolNode> _pools = new();

        public delegate void OnPoolCreatedAction(Component prefab, SimpleExpandablePool<Component> pool);
        private event OnPoolCreatedAction OnPoolCreated = delegate { };

        protected override void AwakeSingleton()
        {
            base.AwakeSingleton();
            foreach (PredefinedPoolConfig c in _predefinedPoolsConfig)
            {
                AddPool(c.Prefab, c.Config);
            }
        }

        public void ExecuteOncePerPool(OnPoolCreatedAction action)
        {
            foreach ((GameObject objKey, PoolNode poolNode) in _pools)
            {
                action(poolNode.Prefab, poolNode.Pool);
            }
            OnPoolCreated += action;
        }

        public T GetInstanceOf<T>(T prefab) where T : Component
        {
            Component instance = EnsurePool(prefab).Get();
            var typedInstance = instance as T;
            if (typedInstance == null) typedInstance = instance.GetComponent<T>();
            if (typedInstance == null)
            {
                throw new ArgumentException($"Prefab {prefab.name} is not of type {typeof(T).Name}.");
            }
            return typedInstance;
        }

        public void ReleaseInstanceOf<T>(T prefab, T instance) where T : Component
        {
            EnsurePool(prefab).Release(instance);
        }

        private SimpleExpandablePool<Component> EnsurePool(Component prefab)
        {
            GameObject objKey = prefab.gameObject;
            if (_pools.TryGetValue(objKey, out PoolNode poolNode))
            {
                return poolNode.Pool;
            }

            return AddPool(prefab, _defaultConfig);
        }

        private SimpleExpandablePool<Component> AddPool(Component prefab, PoolConfig config)
        {
            GameObject objKey = prefab.gameObject;
            if (_pools.ContainsKey(objKey))
            {
                throw new ArgumentException($"Pool for {prefab.name} was added twice.");
            }
            GameObject container = EnsureContainer(prefab, config);

            List<Component> initialResources = new();
            foreach (Transform child in container.transform)
            {
                initialResources.Add(child);
            }

            SimpleExpandablePool<Component> pool = new(
                factory: (pool) => CreateResource(prefab, config, pool.Creations.Count + 1),
                minSize: config.MinSize,
                maxCreations: config.MaxCreations,
                prefill: config.Prefill
            );
            if (config.AutoDeactivate)
            {
                pool.OnBeforeGetInstance += (instance) => instance.gameObject.SetActive(true);
                pool.OnAfterReleaseInstance += (instance) => instance.gameObject.SetActive(false);
            }
            _pools.Add(objKey, new PoolNode { Prefab = prefab, Pool = pool });
            OnPoolCreated.Invoke(prefab, pool);
            return pool;
        }

        private static Component CreateResource(Component prefab, PoolConfig config, int id)
        {
            GameObject container = config.Container;
            Assertx.IsNotNull(container);
            Component instance = Instantiate(prefab, container.transform);
            InitInstance(instance, prefab, config, id);
            return instance;
        }

        private static void InitInstance(Component instance, Component prefab, PoolConfig config, int id)
        {
            instance.name = $"[{id}] {prefab.name}";
            if (config.AutoDeactivate) instance.gameObject.SetActive(false);
        }

        private GameObject EnsureContainer(Component prefab, PoolConfig config)
        {
            if (config.Container == null) config.Container = new GameObject($"{prefab.name} Pool Container");
            GameObject container = config.Container;
            container.transform.SetParent(this.transform);
            return container;
        }

#if UNITY_EDITOR
        [Button(Mode = ButtonMode.DisabledInPlayMode)]
        private void PrecreateInstances()
        {
            this.transform.DestroyAllChildren(immediate: true);
            foreach (PredefinedPoolConfig config in _predefinedPoolsConfig)
            {
                if (config.Prefab == null || config.Config.MinSize <= 0) continue;
                GameObject container = EnsureContainer(config.Prefab, config.Config);

                for (int i = 0; i < config.Config.MinSize; i++)
                {
                    var instance = PrefabUtility.InstantiatePrefab(config.Prefab, container.transform) as Component;
                    InitInstance(instance, config.Prefab, config.Config, i + 1);
                }
            }
        }
#endif
    }
}
