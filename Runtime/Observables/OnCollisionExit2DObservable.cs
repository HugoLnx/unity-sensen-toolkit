using System;
using UnityEngine;

namespace SensenToolkit.Observables
{
    public class OnCollisionExit2DObservable : MonoBehaviour
    {
        public event Action<Collision2D> Callbacks;
        private void OnCollisionExit2D(Collision2D collision)
        {
            Callbacks?.Invoke(collision);
        }
    }
}
