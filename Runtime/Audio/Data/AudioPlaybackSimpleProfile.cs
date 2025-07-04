using System;
using UnityEngine;

namespace SensenToolkit
{
    [CreateAssetMenu(fileName = "AudioPlaybackSimpleProfile", menuName = "Sensen/Audio/PlaybackSimpleProfile", order = 1)]
    public class AudioPlaybackSimpleProfile : AudioPlaybackProfileBase
    {
        [SerializeField]
        [Range(0f, 2f)]
        private float _pitch;

        public override float ChoosePitch() => _pitch;
    }
}
