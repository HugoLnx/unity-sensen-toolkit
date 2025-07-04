using System;
using UnityEngine;

namespace SensenToolkit.Observables
{
    public class OnCollisionEnterObservable : MonoBehaviour
    {
        public event Action<Collision> Callbacks;
        private void OnCollisionEnter(Collision collision)
        {
            Callbacks?.Invoke(collision);
        }
    }
}
