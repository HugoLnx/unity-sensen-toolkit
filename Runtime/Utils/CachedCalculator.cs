using System;
using UnityEngine;

namespace SensenToolkit
{
    public class CachedCalculator<T>
    {
        private T _cachedValue;
        private bool _expired = true;
        private Func<T> _calculation;
#if UNITY_EDITOR
        // Never caches when in editor mode unless the game is running.
        public bool IsExpired => !Application.isPlaying || _expired;
#else
        public bool IsExpired => _expired;
#endif

        public CachedCalculator(Func<T> calculation)
        {
            _calculation = calculation;
        }

        public void ForceRecalculate()
        {
            _cachedValue = _calculation();
            _expired = false;
        }

        public void Expire()
        {
            _expired = true;
        }

        public T Value
        {
            get
            {
                if (!_expired) return _cachedValue;
                ForceRecalculate();
                return _cachedValue;
            }
        }
    }
}
