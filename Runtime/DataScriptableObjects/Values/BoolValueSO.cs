using System;
using UnityEngine;

namespace SensenToolkit
{
    [CreateAssetMenu(menuName = "Sensen/Values/Bool")]
    public class BoolValueSO : AValueSO<bool, BoolValueSO>
    {
        public override Type ValueType => typeof(bool);
    }
}
