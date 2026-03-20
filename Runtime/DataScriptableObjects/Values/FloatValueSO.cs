using System;
using EasyButtons;
using UnityEngine;

namespace SensenToolkit
{
    [CreateAssetMenu(menuName = "Sensen/Values/Float")]
    public class FloatValueSO : AValueSO<float, FloatValueSO>
    {
        public override Type ValueType => typeof(float);

        [Button(Mode = ButtonMode.EnabledInPlayMode)]
        private void SetFloat(float value) => Value = value;
    }
}
