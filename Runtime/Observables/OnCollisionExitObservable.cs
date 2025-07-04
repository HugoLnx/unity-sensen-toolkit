using System;
using UnityEngine;

namespace SensenToolkit.Observables
{
    public class OnCollisionExitObservable : MonoBehaviour
    {
        public event Action<Collision> Callbacks;
        private void OnCollisionExit(Collision collision)
        {
            Callbacks?.Invoke(collision);
        }
    }
}
