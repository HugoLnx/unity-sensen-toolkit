using System;
using UnityEngine;

namespace SensenToolkit
{
    public abstract class AudioPlaybackProfileBase : ScriptableObject
    {
        [field: SerializeField, Range(0f, 2f)]
        public float Volume { get; protected set; } = 1f;

        [field: SerializeField]
        public bool Loop { get; protected set; } = false;

        public abstract float ChoosePitch();
    }
}
