using System;
using EasyButtons;
using UnityEngine;

namespace SensenToolkit
{
    [CreateAssetMenu(menuName = "Sensen/Values/Int")]
    public class IntValueSO : AValueSO<int, IntValueSO>
    {
        public override Type ValueType => typeof(int);

        [Button(Mode = ButtonMode.EnabledInPlayMode)]
        private void SetInt(int value) => Value = value;
    }
}
