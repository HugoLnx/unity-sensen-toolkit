using MyBox;
using UnityEngine;

namespace SensenComponents
{
    [RequireComponent(typeof(Animator))]
    public class AnimatorPlayer : MonoBehaviour
    {
        #region Properties
        [SerializeField, AutoProperty] private Animator _animator;
        #endregion

        #region Public
        public AnimatorPlayback Play(
            int stateHash,
            int layer = -1,
            float normalizedStartAt = float.NegativeInfinity,
            float normalizedTransitionDuration = 0f
        )
        {
            if (normalizedTransitionDuration == 0f)
            {
                _animator.Play(stateHash, layer, normalizedStartAt);
            }
            else
            {
                _animator.CrossFade(
                    stateHash,
                    normalizedTransitionDuration,
                    layer,
                    normalizedStartAt
                );
            }
            return AfterPlay(stateHash, layer);
        }
        public AnimatorPlayback PlayInFixedTime(
            int stateHash,
            int layer = -1,
            float fixedStartAt = float.NegativeInfinity,
            float fixedTransitionDuration = 0f
        )
        {
            if (fixedTransitionDuration == 0f)
            {
                _animator.PlayInFixedTime(stateHash, layer, fixedStartAt);
            }
            else
            {
                _animator.CrossFadeInFixedTime(
                    stateHash,
                    fixedTransitionDuration,
                    layer,
                    fixedStartAt
                );
            }
            return AfterPlay(stateHash, layer);
        }

        public AnimatorPlayback Play(
            string stateName,
            int layer = -1,
            float normalizedStartAt = float.NegativeInfinity,
            float normalizedTransitionDuration = 0f
        ) => Play(Animator.StringToHash(stateName), layer, normalizedStartAt, normalizedTransitionDuration);

        public AnimatorPlayback PlayInFixedTime(
            string stateName,
            int layer = -1,
            float fixedStartAt = float.NegativeInfinity,
            float fixedTransitionDuration = 0f
        ) => PlayInFixedTime(Animator.StringToHash(stateName), layer, fixedStartAt, fixedTransitionDuration);

        public bool IsPlaying(int stateHash, int layer = -1)
        {
            int layerFixed = FixLayer(layer);
            AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(layerFixed);
            AnimatorStateInfo nextState = _animator.GetNextAnimatorStateInfo(layerFixed);
            return IsValidStateOfHash(currentState, stateHash) || IsValidStateOfHash(nextState, stateHash);
        }
        #endregion

        #region Private
        private AnimatorPlayback AfterPlay(int stateHash, int layer)
        {
            EnsureAnimatorStateWasUpdated();
            return AnimatorPlayback.BuildForHash(_animator, stateHash, layer);
        }

        private void EnsureAnimatorStateWasUpdated()
        {
            // Ensure GetNextAnimatorStateInfo and GetCurrentAnimatorStateInfo are updated
            _animator.Update(0f);
        }
        #endregion

        internal static bool IsValidStateOfHash(AnimatorStateInfo state, int hash)
        {
            bool isBlankState = state.fullPathHash == 0;
            return !isBlankState && IsMatchingHash(state, hash);
        }

        private static bool IsMatchingHash(AnimatorStateInfo state, int hash)
        {
            return state.fullPathHash == hash || state.shortNameHash == hash;
        }

        internal static int FixLayer(int layer) => Mathf.Max(0, layer);
    }
}
