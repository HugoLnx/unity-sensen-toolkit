using System.Collections;
using MyBox;
using SensenToolkit;
using UnityEngine;

namespace SensenToolkit
{
    public class StingerService : APermanentSingleton<StingerService>
    {
        [SerializeField, MustBeAssigned] private AudioTrack _stingerTrack;
        [SerializeField, AutoProperty(AutoPropertyMode.Scene)]
        private SfxService _sfxService;
        [SerializeField, AutoProperty(AutoPropertyMode.Scene)]
        private MusicService _musicService;

        protected override void AwakeSingleton()
        {
            base.AwakeSingleton();
            _sfxService = SfxService.Instance;
            _musicService = MusicService.Instance;
        }

        protected override void OnDisableSingleton()
        {
            _musicService.UnlockLowVolume(this);
        }

        public void Play(AudioProfile stinger)
        {
            _musicService.UnlockLowVolume(this);
            StopAllCoroutines();
            _stingerTrack.Stop();
            StartCoroutine(PlayStinger(stinger));
        }

        private IEnumerator PlayStinger(AudioProfile stinger)
        {
            Assertx.IsNotNull(stinger, "Stinger cannot be null.");
            bool stingerHasFinished = false;
            _sfxService.Play(
                stinger,
                track: _stingerTrack,
                onFinished: () => stingerHasFinished = true
            );
            AudioClip clip = stinger.LastPlayedClip;
            Assertx.IsNotNull(clip, $"Stinger {stinger.name} has no audio clip to play.");
            yield return new WaitForSeconds(0.25f);
            _musicService.LockLowVolume(this);
            yield return new WaitForSeconds(clip.length - 0.25f);
            _musicService.UnlockLowVolume(this);
            yield return new WaitUntil(() => stingerHasFinished);
        }
    }
}
