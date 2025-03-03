using System;
using System.Collections.Generic;

namespace SensenToolkit
{
    public class SimpleExpandablePool<T> : IReleasablePool<T>
    {
        private readonly Func<SimpleExpandablePool<T>, T> _factory;
        private readonly Queue<T> _resources = new();
        private readonly int _minSize;
        private readonly int _maxCreations;
        public HashSet<T> Creations { get; } = new();
        private bool HasAvailableResource => _resources.Count > 0;
        private bool CanCreateMore => Creations.Count < _maxCreations;
        public bool CanProvide => HasAvailableResource || CanCreateMore;

        public int MaxCreations => _maxCreations;

        public event Action<T> OnBeforeGetInstance = delegate { };
        public event Action<T> OnAfterReleaseInstance = delegate { };

        public delegate void OnInstanceCreatedAction(T instance);
        private event OnInstanceCreatedAction OnInstanceCreated = delegate { };

        public SimpleExpandablePool(
            Func<SimpleExpandablePool<T>, T> factory,
            int minSize = 20,
            int? maxCreations = null,
            bool prefill = true
        )
        {
            _factory = factory;
            _minSize = minSize;
            _maxCreations = maxCreations ?? minSize + 30;

            if (prefill) Prefill();
        }

        public void Prefill()
        {
            while (Creations.Count < _minSize)
            {
                Grow();
            }
        }

        public T Get()
        {
            if (_resources.Count == 0)
            {
                Grow();
            }
            T instance = _resources.Dequeue();
            OnBeforeGetInstance.Invoke(instance);
            return instance;
        }

        public void Release(T obj)
        {
            if (_resources.Count >= _maxCreations)
            {
                return;
            }
            _resources.Enqueue(obj);
            OnAfterReleaseInstance.Invoke(obj);
        }

        public void ExecuteOncePerInstance(OnInstanceCreatedAction action)
        {
            foreach (T instance in Creations)
            {
                action(instance);
            }
            OnInstanceCreated += action;
        }

        private void Grow()
        {
            if (Creations.Count >= _maxCreations)
            {
                throw new InvalidOperationException("Pool has reached max it should create");
            }
            T creation = _factory(this);
            OnInstanceCreated.Invoke(creation);
            Creations.Add(creation);
            _resources.Enqueue(creation);
        }
    }
}
