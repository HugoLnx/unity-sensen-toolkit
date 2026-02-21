using System;
using System.Collections.Generic;

namespace SensenToolkit
{
    public class RoundRobinPool<T> : ISimplePool<T>
    {
        private readonly Func<T> _factory;
        private readonly T[] _resources;
        private readonly int _minSize;
        private readonly int _maxCreations;
        private int _nextResourceIndex;
        private List<T> _creations = new();
        public HashSet<T> Creations { get; } = new();

        public RoundRobinPool(
            Func<T> factory,
            int minSize = 20,
            int? maxCreations = null,
            bool prefill = true
        )
        {
            _factory = factory;
            _minSize = minSize;
            _maxCreations = maxCreations ?? minSize + 30;
            _resources = new T[_maxCreations];

            if (prefill) Prefill();
        }

        public void Prefill()
        {
            while (_creations.Count < _minSize)
            {
                Grow();
            }
        }

        public T Get()
        {
            if (_creations.Count < _maxCreations && _resources[_nextResourceIndex] == null)
            {
                Grow();
            }
            return GetNextResourceAndUpdateIndex();
        }

        private void Grow()
        {
            if (_creations.Count >= _maxCreations)
            {
                throw new InvalidOperationException("Pool has reached max it should create");
            }
            T creation = _factory();
            _creations.Add(creation);
            Creations.Add(creation);
            _resources[_creations.Count - 1] = creation;
        }

        private T GetNextResourceAndUpdateIndex()
        {
            T resource = _resources[_nextResourceIndex];
            Assertx.IsNotNull(resource, $"Resource at index {_nextResourceIndex} is null, this should not happen");
            _nextResourceIndex = (_nextResourceIndex + 1) % _resources.Length;
            return resource;
        }
    }
}
