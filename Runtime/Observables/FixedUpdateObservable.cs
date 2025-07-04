using UnityEngine;
using System;

namespace SensenToolkit.Observables
{
    public class FixedUpdateObservable : MonoBehaviour, IParameterlessObservable
    {
        public event Action Callbacks;

        private void FixedUpdate()
        {
            Callbacks?.Invoke();
        }
    }
}
