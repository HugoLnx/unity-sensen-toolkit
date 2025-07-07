using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using SensenToolkit;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        private bool _firstFramesHavePast = false;

        private void OnEnable()
        {
            AppCore.OnSceneUnloadStart += OnSceneUnload;
            AppCore.OnSceneLoadEnd += OnSceneLoad;
        }

        protected override void OnDisableAny()
        {
            base.OnDisableAny();
            AppCore.OnSceneUnloadStart -= OnSceneUnload;
            AppCore.OnSceneLoadEnd -= OnSceneLoad;
        }

        public AudioOutput Play(
            AudioProfile profile,
            Vector3? position = null,
            AudioTrack track = null,
            Action onFinished = null
        )
        {
            if (profile.DontPlayOnFirstFrames && !_firstFramesHavePast)
            {
                Debug.Log($"[{typeof(T)}] Not playing {profile.name} because it is set to not play on the first frames.");
                return null;
            }
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
                Debug.LogWarning($"[{typeof(T)}] No audio output available. Abort playing {command.Clip.name}");
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

        private void OnSceneLoad(Scene scene)
        {
            StartCoroutine(DelayedSetFirstFramesHavePast());
        }

        private void OnSceneUnload(Scene scene)
        {
            _firstFramesHavePast = false;
        }

        private IEnumerator DelayedSetFirstFramesHavePast()
        {
            yield return null;
            yield return null;
            yield return null;
            _firstFramesHavePast = true;
        }
    }
}
