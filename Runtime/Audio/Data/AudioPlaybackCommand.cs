using UnityEngine;

namespace SensenToolkit
{
    public struct AudioPlaybackCommand
    {
        public AudioClip Clip { get; }
        public float Volume { get; }
        public bool Loop { get; }
        public float Pitch { get; }
        public AudioTrack Track { get; }
        public bool UseGlobalTrack => Track.IsGlobal;
        public Audio3DSettings Settings3D { get; }
        public Vector3? Position { get; }

        public AudioPlaybackCommand(
            AudioClip clip,
            float volume,
            bool loop,
            float pitch,
            AudioTrack track,
            Audio3DSettings settings3d,
            Vector3? position
        )
        {
            Clip = clip;
            Volume = volume;
            Loop = loop;
            Pitch = pitch;
            Track = track;
            Settings3D = settings3d;
            Position = position;
        }

        public AudioPlaybackCommand WithDefaultTrack(AudioTrack track)
        {
            if (this.Track != null) return this;
            return new AudioPlaybackCommand(Clip, Volume, Loop, Pitch, track, null, null);
        }
    }
}
