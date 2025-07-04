using MyBox;
using UnityEngine;

namespace SensenToolkit
{
    public class AudioOutputWrapper : MonoBehaviour
    {
        public AudioOutput Output { get; private set; }
        [SerializeField, AutoProperty] private AudioSource _source;
        private void Awake()
        {
            Output = new AudioOutput(_source, this);
        }
    }
}
