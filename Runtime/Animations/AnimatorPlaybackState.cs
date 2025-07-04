using UnityEngine;

namespace SensenComponents
{
    public readonly struct AnimatorPlaybackState
    {
        public static readonly AnimatorPlaybackState None = new();

        public AnimatorStateInfo? StateInfo { get; }
        public AnimationClip Clip { get; }
        public bool IsTransitioningIn { get; }
        public bool IsTransitioningOut { get; }
        public bool IsTransitioning => IsTransitioningIn || IsTransitioningOut;
        public bool IsPlaying => StateInfo.HasValue
            && (StateInfo?.loop == true || StateInfo?.normalizedTime < 1f);
        public bool IsPlayingUninterrupted => IsPlaying && !IsTransitioningOut;
        public bool IsPlayingAlone => IsPlaying && !IsTransitioning;
        public float NormalizedTime => StateInfo?.normalizedTime ?? -1f;
        public float FixedTimePlayed => StateInfo?.length * NormalizedTime ?? -1f;
        public float FixedTimeToFinish => StateInfo?.length * Mathf.Max(0f, 1f - NormalizedTime) ?? -1f;
        public int CurrentFrame => Mathf.RoundToInt(FixedTimePlayed * Clip.frameRate);
        public int FramesToFinish => Mathf.RoundToInt(FixedTimeToFinish * Clip.frameRate);

        public AnimatorPlaybackState(AnimatorStateInfo? state = null, AnimationClip clip = null, bool isTransitioningIn = false, bool isTransitioningOut = false)
        {
            StateInfo = state;
            Clip = clip;
            IsTransitioningIn = isTransitioningIn;
            IsTransitioningOut = isTransitioningOut;
        }

        public override string ToString()
        {
            return $"AnimatorPlaybackState: hash:{StateInfo?.fullPathHash} isPlaying:{IsPlaying} timePlayed:{FixedTimePlayed} normalizedTime: {NormalizedTime} isTransitioningIn:{IsTransitioningIn} isTransitioningOut:{IsTransitioningOut}";
        }
    }
}
