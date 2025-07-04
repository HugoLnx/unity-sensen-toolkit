using System.Collections;
using System.Collections.Generic;
using MyBox;
using SensenToolkit;
using UnityEngine;

namespace SensenToolkit
{
    [RequireComponent(typeof(PrefabPools))]
    public class OneShotParticlePools : ATransientSingleton<OneShotParticlePools>
    {
        private PrefabPools _pools;
        private HashSet<OneShotParticle> _playingParticles = new();
        protected override void AwakeSingleton()
        {
            base.AwakeSingleton();
            _pools = PrefabPools.Instance;
        }

        protected override void OnDisableSingleton()
        {
            base.OnDisableSingleton();
            foreach (OneShotParticle particle in _playingParticles)
            {
                particle.Instance.Stop();
                particle.Instance.Clear();
                _pools.ReleaseInstanceOf(particle.Prefab, particle.Instance);
            }
            _playingParticles.Clear();
        }

        public OneShotParticle GetInstanceOf(ParticleSystem prefab)
        {
            ParticleSystem instance = _pools.GetInstanceOf(prefab);
            return new OneShotParticle(prefab, instance, this);
        }

        internal void PlayOneShot(OneShotParticle particle)
        {
            StartCoroutine(PlayOneShotCoroutine(particle));
        }

        private IEnumerator PlayOneShotCoroutine(OneShotParticle particle)
        {
            ParticleSystem prefab = particle.Prefab;
            ParticleSystem instance = particle.Instance;
            _playingParticles.Add(particle);
            instance.Play();
            yield return new WaitWhile(() => instance.isPlaying);
            _playingParticles.Remove(particle);
            _pools.ReleaseInstanceOf(prefab, instance);
        }
    }
}
