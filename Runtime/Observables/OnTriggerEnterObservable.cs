using UnityEngine;
using System;

namespace SensenToolkit.Observables
{
    public class OnTriggerEnterObservable : MonoBehaviour
    {
        public event Action<Collider> Callbacks;
        private void OnTriggerEnter(Collider other)
        {
            Callbacks?.Invoke(other);
        }
    }
}
