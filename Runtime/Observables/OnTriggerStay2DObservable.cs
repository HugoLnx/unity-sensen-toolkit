using System;
using UnityEngine;

namespace SensenToolkit.Observables
{
    public class OnTriggerStay2DObservable : MonoBehaviour
    {
        public event Action<Collider2D> Callbacks;
        private void OnTriggerStay2D(Collider2D other)
        {
            Callbacks?.Invoke(other);
        }
    }
}
