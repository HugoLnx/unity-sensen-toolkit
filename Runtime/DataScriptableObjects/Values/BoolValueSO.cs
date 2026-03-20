using System;
using EasyButtons;
using UnityEngine;

namespace SensenToolkit
{
    [CreateAssetMenu(menuName = "Sensen/Values/Bool")]
    public class BoolValueSO : AValueSO<bool, BoolValueSO>
    {
        public override Type ValueType => typeof(bool);

        [Button(Mode = ButtonMode.EnabledInPlayMode)]
        public void Toggle() => Value = !Value;
    }
}
