using System;
using UnityEngine;

namespace SensenToolkit
{
    [CreateAssetMenu(menuName = "Sensen/Values/Int")]
    public class IntValueSO : AValueSO<int, IntValueSO>
    {
        public override Type ValueType => typeof(int);
    }
}
