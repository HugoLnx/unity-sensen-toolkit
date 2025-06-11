using System;
using UnityEngine;

namespace SensenToolkit
{
    [CreateAssetMenu(menuName = "Sensen/Values/Float")]
    public class FloatValueSO : AValueSO<float, FloatValueSO>
    {
        public override Type ValueType => typeof(float);
    }
}
