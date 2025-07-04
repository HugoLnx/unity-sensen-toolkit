using System;
using System.Collections;
using System.Collections.Generic;
using SensenToolkit;
using UnityEngine;

namespace SensenComponents
{
    public class AnimatorPlayback

    {
        private readonly Animator _animator;
        private readonly int _hash;
        private readonly int _layer;
        private List<AnimatorClipInfo> _clipsBuffer = new();

        public AnimatorPlayback(Animator animator, int hash, int layer = -1)
        {
            Assertx.IsNotZero(hash, "Hash must not be zero.");

            _animator = animator;
            _hash = hash;
            _layer = AnimatorPlayer.FixLayer(layer);
        }

        public static AnimatorPlayback BuildForHash(Animator animator, int hash, int layer = -1)
        {
            return new AnimatorPlayback(
                animator: animator,
                hash: hash,
                layer: AnimatorPlayer.FixLayer(layer)
            );
        }

        public AnimatorPlaybackState GetCurrentState()
        {
            AnimatorStateInfo? currentState = ReturnSameIfValidState(_animator.GetCurrentAnimatorStateInfo(_layer));
            AnimatorStateInfo? nextState = ReturnSameIfValidState(_animator.GetNextAnimatorStateInfo(_layer));

            if (nextState.HasValue)
            {
                return new AnimatorPlaybackState(
                    state: nextState.Value,
                    clip: GetNextFirstClip(_layer),
                    isTransitioningIn: true
                );
            }
            else if (currentState.HasValue)
            {
                return new AnimatorPlaybackState(
                    state: currentState.Value,
                    clip: GetCurrentFirstClip(_layer),
                    isTransitioningOut: _animator.IsInTransition(_layer)
                );
            }
            else
            {
                return AnimatorPlaybackState.None;
            }
        }

        public IEnumerator WaitForPlaybackEnd(float delay = 0.15f)
        {
            WaitForSeconds wait = new(delay);
            while (GetCurrentState().IsPlaying)
            {
                yield return wait;
            }
        }

        private AnimationClip GetCurrentFirstClip(int? layer = null)
        {
            _clipsBuffer.Clear();
            _animator.GetCurrentAnimatorClipInfo(AnimatorPlayer.FixLayer(layer ?? _layer), _clipsBuffer);
            if (_clipsBuffer.Count == 0)
            {
                return null;
            }
            return _clipsBuffer[0].clip;
        }

        private AnimationClip GetNextFirstClip(int? layer = null)
        {
            _clipsBuffer.Clear();
            _animator.GetNextAnimatorClipInfo(AnimatorPlayer.FixLayer(layer ?? _layer), _clipsBuffer);
            if (_clipsBuffer.Count == 0)
            {
                return null;
            }
            return _clipsBuffer[0].clip;
        }

        private AnimatorStateInfo? ReturnSameIfValidState(AnimatorStateInfo state)
        {
            if (AnimatorPlayer.IsValidStateOfHash(state, _hash)) return state;
            else return null;
        }
    }
}
