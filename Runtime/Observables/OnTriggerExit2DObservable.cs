using System;
using UnityEngine;

namespace SensenToolkit.Observables
{
    public class OnTriggerExit2DObservable : MonoBehaviour
    {
        public event Action<Collider2D> Callbacks;
        private void OnTriggerExit2D(Collider2D other)
        {
            Callbacks?.Invoke(other);
        }
    }
}
