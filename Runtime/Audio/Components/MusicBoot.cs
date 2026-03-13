using MyBox;
using UnityEngine;

namespace SensenToolkit
{
    public class MusicBoot : MonoBehaviour
    {
        [SerializeField] private AudioProfile _bootMusic;

        private void Start()
        {
            if (_bootMusic == null) return;
            MusicService musicService = MusicService.Instance;
            musicService.StopAllAudio();
            musicService.Play(_bootMusic);
        }
    }
}
