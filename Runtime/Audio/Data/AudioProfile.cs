using System;
using MyBox;
using SensenToolkit;
using UnityEngine;

namespace SensenToolkit
{
    [CreateAssetMenu(fileName = "AudioProfile", menuName = "Sensen/Audio/Profile", order = 1)]
    public class AudioProfile : ScriptableObject
    {
        [field: SerializeField]
        public AudioClip[] Clips { get; private set; }
        [field: SerializeField]
        public bool RandomizeClips { get; private set; } = true;
        [field: SerializeField]
        public AudioTrack Track { get; private set; }
        [Tooltip("Volume multiplier (5x should be the max used)")]
        [field: SerializeField, Range(0f, 7f)]
        public float Volume { get; private set; } = 1f;

        [field: SerializeField]
        public AudioPlaybackProfileBase PlaybackProfile { get; private set; }
        [field: SerializeField]
        [field: Tooltip("Ignore playing this sound on the first frames of the game.")]
        public bool DontPlayOnFirstFrames { get; private set; } = false;
        [field: SerializeField]
        public bool Enable3D { get; private set; } = false;
        [field: SerializeField, ConditionalField(useMethod: true, method: nameof(Is3DEnabled))]
        public Audio3DSettings Settings3D { get; private set; }
        public AudioClip LastPlayedClip { get; private set; }

        private RandomWithVariability _random;
        private RandomWithVariability VarRandom => _random ??= new(
            optionsAmount: Clips.Length,
            percentReductionOnSelect: 0.25f,
            noSequentialRepetition: true
        );

        private int _clipIndex = 0;

        private void Awake()
        {
            _random = null;
            _clipIndex = 0;
            LastPlayedClip = null;
        }

        public AudioPlaybackCommand GetCommand(
            AudioTrack track = null,
            Vector3? position = null,
            float volumeModifier = 1f
        )
        {
            if (track == null) track = Track;
            if (track == null) track = AudioTrack.Global;
            return new AudioPlaybackCommand(
                clip: ChooseClip(),
                volume: PlaybackProfile.Volume * Volume * volumeModifier,
                loop: PlaybackProfile.Loop,
                pitch: PlaybackProfile.ChoosePitch(),
                track: track,
                settings3d: Enable3D ? Settings3D : null,
                position: position
            );
        }

        private AudioClip ChooseClip()
        {
            if (Clips.Length == 0) return null;
            AudioClip clip = null;
            if (RandomizeClips)
            {
                clip = Clips[VarRandom.Select()];
            }
            else
            {
                clip = Clips[_clipIndex];
                _clipIndex = (_clipIndex + 1) % Clips.Length;
            }
            LastPlayedClip = clip;
            return clip;
        }

        private bool Is3DEnabled() => Enable3D;
    }
}
