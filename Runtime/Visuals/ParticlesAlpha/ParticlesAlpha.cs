using System;
using EasyButtons;
using MyBox;
using UnityEngine;

namespace SensenToolkit
{
    public class ParticlesAlpha : MonoBehaviour, ISingleAlpha
    {
        [SerializeField, AutoProperty] private ParticleSystem _particles;
        [SerializeField, ReadOnly] private float _alpha = 1f;
        private ParticlesStartColors? _initialColor;
        private ParticlesStartColors _tmpColor;
        private ParticlesStartColors InitialColor => EnsureInitialColor();

        public float Alpha
        {
            get => _alpha;
            set
            {
                _alpha = value;
                RefreshParticles();
            }
        }

        private void OnEnable()
        {
            EnsureInitialColor();
            RefreshParticles();
        }

        private void OnDisable()
        {
            _initialColor?.ApplyTo(_particles);
        }

        private void RefreshParticles()
        {
            if (!this.gameObject.activeInHierarchy) return;

            _tmpColor
                .CopyFrom(InitialColor)
                .ScaleByAlpha(_alpha)
                .ApplyTo(_particles);
        }

        [Button]
        private void SetAlpha(float alpha = 1f)
        {
            Alpha = alpha;
        }

        private ParticlesStartColors EnsureInitialColor()
        {
            return _initialColor ??= ParticlesStartColors.FromParticles(_particles);
        }
    }
}
