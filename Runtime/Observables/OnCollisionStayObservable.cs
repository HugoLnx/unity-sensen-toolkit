using System;
using UnityEngine;

namespace SensenToolkit.Observables
{
    public class OnCollisionStayObservable : MonoBehaviour
    {
        public event Action<Collision> Callbacks;
        private void OnCollisionStay(Collision collision)
        {
            Callbacks?.Invoke(collision);
        }
    }
}
