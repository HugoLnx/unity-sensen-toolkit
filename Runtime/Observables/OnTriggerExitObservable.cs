using System;
using UnityEngine;

namespace SensenToolkit.Observables
{
    public class OnTriggerExitObservable : MonoBehaviour
    {
        public event Action<Collider> Callbacks;
        private void OnTriggerExit(Collider other)
        {
            Callbacks?.Invoke(other);
        }
    }
}
