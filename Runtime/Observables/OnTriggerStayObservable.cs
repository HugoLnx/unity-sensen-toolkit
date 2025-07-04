using System;
using UnityEngine;

namespace SensenToolkit.Observables
{
    public class OnTriggerStayObservable : MonoBehaviour
    {
        public event Action<Collider> Callbacks;
        private void OnTriggerStay(Collider other)
        {
            Callbacks?.Invoke(other);
        }
    }
}
