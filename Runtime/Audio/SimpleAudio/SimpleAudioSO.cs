using UnityEngine;

namespace SensenToolkit
{
    [CreateAssetMenu(fileName = "SimpleAudioClip", menuName = "Sensen/SimpleAudio/Clip", order = 1)]
    public class SimpleAudioSO : ScriptableObject
    {
        [field: SerializeField] public AudioClip[] Clips { get; private set; }
        [field: SerializeField] public float PitchRange { get; private set; } = 0.15f;
        [field: SerializeField] public float VolumeModifier { get; private set; } = 1f;
        [field: SerializeField] public bool Loop { get; private set; } = false;

        public void PlayOn(AudioSource source)
        {
            if (source == null) return;
            if (source.isPlaying && Loop) return;
            source.pitch = 1 + UnityEngine.Random.Range(-PitchRange, PitchRange);
            source.volume = VolumeModifier;
            source.loop = Loop;
            AudioClip clip = Clips[UnityEngine.Random.Range(0, Clips.Length)];
            source.PlayOneShot(clip);
        }
    }
}
