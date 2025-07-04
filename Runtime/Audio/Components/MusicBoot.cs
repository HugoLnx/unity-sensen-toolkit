using MyBox;
using UnityEngine;

namespace SensenToolkit
{
    public class MusicBoot : MonoBehaviour
    {
        [SerializeField] private AudioProfile _bootMusic;

        private void Start()
        {
            _bootMusic.Track.Stop();
            MusicService.Instance.Play(_bootMusic);
        }
    }
}
