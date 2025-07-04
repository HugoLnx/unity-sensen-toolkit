using UnityEngine;
using System;

namespace SensenToolkit.Observables
{
    public class UpdateObservable : MonoBehaviour, IParameterlessObservable
    {
        public event Action Callbacks;

        private void Update()
        {
            Callbacks?.Invoke();
        }
    }
}
