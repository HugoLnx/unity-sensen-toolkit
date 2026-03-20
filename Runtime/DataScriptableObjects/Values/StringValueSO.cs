using System;
using UnityEngine;

namespace SensenToolkit
{
    [CreateAssetMenu(menuName = "Sensen/Values/String")]
    public class StringValueSO : AValueSO<string, StringValueSO>
    {
        public override Type ValueType => typeof(string);
        private void SetString(string value) => Value = value;
    }
}
