using System;
using UnityEngine;

namespace SensenToolkit
{
    public abstract class AValueSOBase : ScriptableObject
    {
        public abstract string Name { get; }
        public abstract void ResetToDefault();
        public abstract object ValueAsObject { get; }
        public abstract Type ValueType { get; }
        public abstract bool IsUsingDefaultValue { get; }
    }
}
