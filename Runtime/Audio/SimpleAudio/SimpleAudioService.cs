using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace SensenToolkit
{
    public class SimpleAudioService : APermanentSingleton<SimpleAudioService>
    {
        [SerializeField, MustBeAssigned] private AudioSource _musicSource;
        private readonly Dictionary<SimpleAudioSO, AudioSource> _sources = new();

        public void PlaySFX(SimpleAudioSO sfx)
        {
            AudioSource source = GetAudioSourceFor(sfx);
            sfx.PlayOn(source);
        }

        public void StopSFX(SimpleAudioSO sfx)
        {
            if (_sources.TryGetValue(sfx, out AudioSource source))
            {
                source.Stop();
            }
        }

        public void PlayMusic(AudioClip music)
        {
            if (_musicSource.isPlaying) return;
            _musicSource.clip = music;
            _musicSource.Play();
        }

        public void StopMusic()
        {
            _musicSource.Stop();
        }

        private AudioSource GetAudioSourceFor(SimpleAudioSO sfx)
        {
            if (_sources.TryGetValue(sfx, out AudioSource source))
            {
                return source;
            }

            source = gameObject.AddComponent<AudioSource>();
            _sources.Add(sfx, source);
            return source;
        }
    }
}
