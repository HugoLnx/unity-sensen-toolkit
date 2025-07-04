using System.Collections.Generic;
using UnityEngine;
using SensenToolkit;
using MyBox;

namespace SensenToolkit
{
    public abstract class APermanentPrefabPoolBase<T, TPooled, TPrefab> : APermanentSingleton<T>, IReleasablePool<TPooled>
    where TPrefab : Component
    where T : APermanentPrefabPoolBase<T, TPooled, TPrefab>
    {
        [SerializeField, InitializationField] protected TPrefab _prefab;
        [SerializeField, InitializationField] protected int _minSize = 20;
        [SerializeField, InitializationField] protected int _maxCreations = 50;
        private IReleasablePool<TPooled> _pool;
        public HashSet<TPooled> Creations => _pool?.Creations;

        protected override void AwakeSingleton()
        {
            SimpleExpandablePool<TPooled> pool = new(
                factory: InstantiateNew,
                minSize: _minSize,
                maxCreations: _maxCreations,
                prefill: false
            );
            _pool = pool;
            pool.Prefill();
        }

        public TPooled Get()
        {
            return _pool.Get();
        }

        public virtual void Release(TPooled resource)
        {
            _pool.Release(resource);
        }

        protected abstract TPooled InstantiateNew(SimpleExpandablePool<TPooled> pool);
    }
}
