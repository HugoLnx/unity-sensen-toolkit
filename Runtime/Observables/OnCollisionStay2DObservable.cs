using System;
using UnityEngine;

namespace SensenToolkit.Observables
{
    public class OnCollisionStay2DObservable : MonoBehaviour
    {
        public event Action<Collision2D> Callbacks;
        private void OnCollisionStay2D(Collision2D collision)
        {
            Callbacks?.Invoke(collision);
        }
    }
}
