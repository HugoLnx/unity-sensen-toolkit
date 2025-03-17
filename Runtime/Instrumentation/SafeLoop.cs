using System.Diagnostics;

namespace SensenToolkit
{
    public struct SafeLoop
    {
        private const int DefaultMaxIterations = 1000;
        private int _maxIterations;
        private int _interactions;
        public bool HasReachedMax => _interactions >= _maxIterations;

        public SafeLoop(int maxIterations)
        {
            _maxIterations = maxIterations;
            _interactions = 0;
        }

        [Conditional("UNITY_EDITOR")]
        public void Count(bool throwError = true)
        {
            if (_maxIterations <= 0) _maxIterations = DefaultMaxIterations;
            _interactions++;
            if (throwError && HasReachedMax)
            {
                // UnityEngine.Debug.LogError("SafeLoop: Max iterations reached. Exiting loop.");
                // UnityEngine.Debug.Break();
                throw new System.Exception("SafeLoop: Max iterations reached. Exiting loop.");
            }
        }

        [Conditional("UNITY_EDITOR")]
        public void Reset()
        {
            _interactions = 0;
        }
    }
}
