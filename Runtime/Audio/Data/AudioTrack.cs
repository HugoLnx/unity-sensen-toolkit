using System;
using System.Collections.Generic;
using UnityEngine;

namespace SensenToolkit
{
    [CreateAssetMenu(fileName = "AudioTrack", menuName = "Sensen/Audio/Track", order = 1)]
    public class AudioTrack : ScriptableObject
    {
        [field: SerializeField] public bool Mute { get; private set; } = false;
        [field: SerializeField, Range(0f, 1f)]
        public float TrackVolumeModifier { get; private set; } = 0.7f;

        [Tooltip("Volume reduction so individual tracks can up to 4x the volume")]
        [SerializeField, Range(0f, 1f)] private float _volumeBaseReduction = 0.25f;
        private float _tmpVolumeModifier = 1f;
        public float VolumeModifier => TrackVolumeModifier * _volumeBaseReduction * _tmpVolumeModifier;
        public virtual bool IsGlobal => false;
        public static AudioTrack Global => EnsureGlobalTrack();

        private static AudioTrack s_globalTrack;
        private readonly HashSet<AudioOutput> _outputs = new();

        private void OnEnable()
        {
            _tmpVolumeModifier = 1f;
        }

        internal void Register(AudioOutput output)
        {
            _outputs.Add(output);
        }

        internal void Unregister(AudioOutput output)
        {
            _outputs.Remove(output);
        }

        public void SetTmpVolumeModifier(float modifier)
        {
            _tmpVolumeModifier = Mathf.Clamp01(modifier);
            RefreshOutputs();
        }

        public void SetTrackVolumeModifier(float modifier)
        {
            TrackVolumeModifier = Mathf.Clamp01(modifier);
            RefreshOutputs();
        }

        public void SetMute(bool mute)
        {
            Mute = mute;
            RefreshOutputs();
        }

        public void Stop()
        {
            foreach (AudioOutput output in _outputs)
            {
                if (output == null) continue;
                output.Stop();
            }
            _outputs.Clear();
        }

        private void RefreshOutputs()
        {
            foreach (AudioOutput output in _outputs)
            {
                if (output == null) continue;
                output.RefreshSource();
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitializeStatics()
        {
            EnsureGlobalTrack(forceRecreate: true);
        }
        private static AudioTrack EnsureGlobalTrack(bool forceRecreate = false)
        {
            if (forceRecreate || s_globalTrack != null) return s_globalTrack;
            s_globalTrack = CreateInstance<GlobalAudioTrack>();
            return s_globalTrack;
        }
    }
}
