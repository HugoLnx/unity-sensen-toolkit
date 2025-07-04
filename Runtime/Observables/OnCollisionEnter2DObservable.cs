using System;
using UnityEngine;

namespace SensenToolkit.Observables
{
    public class OnCollisionEnter2DObservable : MonoBehaviour
    {
        public event Action<Collision2D> Callbacks;
        private void OnCollisionEnter2D(Collision2D collision)
        {
            Callbacks?.Invoke(collision);
        }
    }
}
