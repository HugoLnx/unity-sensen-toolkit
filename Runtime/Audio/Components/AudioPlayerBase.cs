using System;
using System.Collections.Generic;
using MyBox;
using SensenToolkit;
using UnityEngine;

namespace SensenToolkit
{
    public abstract class AudioPlayerBase<T> : APermanentSingleton<T>
    where T : APermanentSingleton<T>
    {
        [SerializeField, Range(0f, 1f)] private float _globalVolume = 1f;
        [SerializeField] private bool _isMuted;
        [SerializeField, AutoProperty(AutoPropertyMode.Scene)]
        private AudioOutputPool _outputPool;
        private float _volumeModifier = 1f;
        private float _lowVolumeModifier = 1f;

        private float GlobalVolume => _globalVolume * _volumeModifier * _lowVolumeModifier;
        public AudioOutput LastPlayedOutput { get; private set; }
        private HashSet<Component> _lowVolumeLocks = new();

        public AudioOutput Play(
            AudioProfile profile,
            Vector3? position = null,
            AudioTrack track = null,
            Action onFinished = null
        )
        {
            AudioPlaybackCommand command = profile.GetCommand(
                track: track,
                position: position
            );
            return Play(command, onFinished);
        }

        public AudioOutput Play(AudioPlaybackCommand command, Action onFinished = null)
        {
            AudioOutput output = _outputPool.Get();
            if (output == null || !output.IsValid)
            {
                Debug.LogWarning($"[{nameof(T)}] No audio output available. Abort playing {command.Clip.name}");
                return null;
            }
            LastPlayedOutput = output;
            output.UpdateVolume(modifier: GlobalVolume);
            output.UpdateTrack(command.Track);
            output.Play(command, onFinished);
            return output;
        }
        public void SetIsAudible(bool isAudible)
        {
            _isMuted = !isAudible;
            UpdateAllAudioOutputs();
        }

        // To be configured by the player via settings
        public void SetGlobalVolume(float volume)
        {
            _globalVolume = volume;
            UpdateAllAudioOutputs();
        }

        // To be used by scripts
        public void SetVolumeModifier(float modifier)
        {
            _volumeModifier = modifier;
            UpdateAllAudioOutputs();
        }

        // To put music in low volume when playing stingers
        public void LockLowVolume(Component component)
        {
            if (_lowVolumeLocks.Contains(component)) return;
            _lowVolumeLocks.Add(component);
            UpdateLowVolumeLock();
        }

        public void UnlockLowVolume(Component component)
        {
            if (!_lowVolumeLocks.Contains(component)) return;
            _lowVolumeLocks.Remove(component);
            UpdateLowVolumeLock();
        }

        private void UpdateLowVolumeLock()
        {
            float previousModifier = _lowVolumeModifier;
            _lowVolumeModifier = _lowVolumeLocks.Count == 0 ? 1f : 0.85f;
            if (previousModifier != _lowVolumeModifier) UpdateAllAudioOutputs();
        }

        private void UpdateAllAudioOutputs()
        {
            foreach (AudioOutput output in _outputPool.Creations)
            {
                if (output == null || !output.IsValid) continue;
                output.UpdateVolume(modifier: GlobalVolume);
                output.UpdateMute(isMute: _isMuted);
            }
        }
    }
}
