using UnityEngine;

namespace SensenToolkit
{
    public readonly struct OneShotParticle
    {
        internal readonly ParticleSystem Prefab;
        internal readonly ParticleSystem Instance;
        private readonly OneShotParticlePools _pools;

        public OneShotParticle(
            ParticleSystem prefab,
            ParticleSystem instance,
            OneShotParticlePools pools
        )
        {
            Prefab = prefab;
            Instance = instance;
            _pools = pools;
        }

        public readonly Transform Transform => Instance.transform;
        public void PlayOneShot() => _pools.PlayOneShot(this);
    }
}
