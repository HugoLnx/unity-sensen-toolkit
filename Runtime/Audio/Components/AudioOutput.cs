using System;
using System.Collections;
using SensenToolkit;
using UnityEngine;

namespace SensenToolkit
{
    public class AudioOutput
    {
        public bool Mute { get; private set; }
        public float VolumeModifier { get; private set; } = 1f;
        public float Volume { get; private set; }
        public float Pitch { get => _source.pitch; set => _source.pitch = value; }
        public bool Loop { get => _source.loop; set => _source.loop = value; }
        public AudioTrack Track { get; private set; }
        public bool IsPlaying { get; private set; }
        private float TrackVolumeModifier => Track == null ? 1f : Track.VolumeModifier;
        private bool TrackMute => Track != null && Track.Mute == true;

        public string SourceName => _source.name;
        public AudioSource Source => _source;
        public bool IsValid => _source != null;

        private readonly AudioSource _source;
        private readonly MonoBehaviour _mono;

        public event Action<AudioOutput> OnFinishedPlaying;

        public AudioOutput(AudioSource source, MonoBehaviour mono)
        {
            _source = source;
            _source.Stop();
            _mono = mono;
            _mono.StartCoroutine(MonitorLoop());
        }

        public void Play(AudioPlaybackCommand command, Action onFinished = null)
        {
            if (!IsValid) return;
            IsPlaying = true;
            Volume = command.Volume;
            Loop = command.Loop;
            Pitch = command.Pitch;
            Audio3DSettings settings3d = command.Settings3D == null ? Audio3DSettings.Default : command.Settings3D;
            settings3d.ApplyToSource(_source);

            _source.transform.position = command.Position ?? Vector3.zero;
            Play(command.Clip, onFinished);
        }

        public void Play(AudioClip clip, Action onFinished = null)
        {
            Assertx.IsNotNull(clip, "AudioOutput.Play: clip cannot be null");
            if (!IsValid) return;
            IsPlaying = true;
            RefreshSource();
            if (Loop)
            {
                _source.clip = clip;
                _source.Play();

                void OnFinished(AudioOutput output)
                {
                    onFinished?.Invoke();
                    OnFinishedPlaying -= OnFinished;
                }

                OnFinishedPlaying += OnFinished;
            }
            else
            {
                _source.clip = null;
                _source.PlayOneShot(clip, Volume);
                if (onFinished != null)
                {
                    _mono.StartCoroutine(ScheduleOnFinished(onFinished, clip.length));
                }
            }
        }

        public void Stop()
        {
            if (_source != null) _source.Stop();
            IsPlaying = false;
        }

        public void UpdateVolume(float? volume = null, float? modifier = null)
        {
            VolumeModifier = modifier ?? VolumeModifier;
            Volume = volume ?? Volume;
            RefreshSource();
        }

        public void UpdateMute(bool isMute)
        {
            Mute = isMute;
            RefreshSource();
        }

        public void UpdateTrack(AudioTrack track)
        {
            if (Track != null) Track.Unregister(this);
            Track = track;
            if (Track != null) Track.Register(this);
            RefreshSource();
        }

        public void RemoveTrack() => UpdateTrack(track: null);

        internal void RefreshSource()
        {
            if (!IsValid) return;
            if (_source.loop)
            {
                _source.volume = Volume * VolumeModifier * TrackVolumeModifier;
            }
            else
            {
                _source.volume = VolumeModifier * TrackVolumeModifier;
            }
            _source.mute = Mute || TrackMute;
        }

        private IEnumerator MonitorLoop()
        {
            while (true)
            {
                yield return new WaitUntil(() => _source != null && _source.isPlaying);
                yield return new WaitWhile(() => Time.timeScale == 0 || !Application.isFocused || !Application.isPlaying || _source == null || _source.isPlaying);
                IsPlaying = false;
                OnFinishedPlaying?.Invoke(this);
            }
        }

        private IEnumerator ScheduleOnFinished(Action onFinished, float clipLength)
        {
            yield return new WaitForSeconds(clipLength);
            if (IsValid) yield break;
            onFinished?.Invoke();
        }
    }
}
