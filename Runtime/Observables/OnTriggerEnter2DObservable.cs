using System;
using UnityEngine;

namespace SensenToolkit.Observables
{
    public class OnTriggerEnter2DObservable : MonoBehaviour
    {
        public event Action<Collider2D> Callbacks;
        private void OnTriggerEnter2D(Collider2D other)
        {
            Callbacks?.Invoke(other);
        }
    }
}
