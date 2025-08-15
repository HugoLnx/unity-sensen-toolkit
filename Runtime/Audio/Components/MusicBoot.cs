using MyBox;
using UnityEngine;

namespace SensenToolkit
{
    public class MusicBoot : MonoBehaviour
    {
        [SerializeField] private AudioProfile _bootMusic;

        private void Start()
        {
            MusicService musicService = MusicService.Instance;
            musicService.StopAllAudio();
            musicService.Play(_bootMusic);
        }
    }
}
