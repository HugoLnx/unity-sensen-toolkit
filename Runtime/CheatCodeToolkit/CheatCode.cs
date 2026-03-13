using MyBox;
using UnityEngine;
using UnityEngine.Events;


namespace SensenToolkit
{
    [System.Serializable]
    public struct CheatCodeConfig
    {
        [Tooltip("Code that must be typed")]
        [SerializeField, MustBeAssigned] public string Code;
        public UnityEvent Trigger;
    }
}
