using SensenToolkit;
using UnityEngine;

namespace SensenToolkit
{
    public abstract class APrefabSimplePool<TPrefab> : APrefabPoolBase<TPrefab, TPrefab>
    where TPrefab : Component
    {
        protected override TPrefab InstantiateNew(SimpleExpandablePool<TPrefab> pool)
        {
            TPrefab instance = Instantiate(_prefab, this.transform);
            instance.name = $"[{pool.Creations.Count + 1}] {_prefab.name}";
            return instance;
        }
    }
}
