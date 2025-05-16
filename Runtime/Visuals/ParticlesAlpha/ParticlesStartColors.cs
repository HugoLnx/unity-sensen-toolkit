using System;
using UnityEngine;

namespace SensenToolkit
{
    public struct ParticlesStartColors
    {
        public Color Color;
        public Color MinColor;
        public Color MaxColor;
        public Gradient Gradient;
        public Gradient MinGradient;
        public Gradient MaxGradient;

        public static ParticlesStartColors FromParticles(ParticleSystem particles)
        {
            return new ParticlesStartColors
            {
                Color = particles.main.startColor.color,
                MinColor = particles.main.startColor.colorMin,
                MaxColor = particles.main.startColor.colorMax,
                Gradient = particles.main.startColor.gradient,
                MinGradient = particles.main.startColor.gradientMin,
                MaxGradient = particles.main.startColor.gradientMax
            };
        }

        public ParticlesStartColors ScaleByAlpha(float alpha)
        {
            this.Color.a *= alpha;
            this.MinColor.a *= alpha;
            this.MaxColor.a *= alpha;
            ScaleGradientAlpha(ref this.Gradient, alpha);
            ScaleGradientAlpha(ref this.MinGradient, alpha);
            ScaleGradientAlpha(ref this.MaxGradient, alpha);
            return this;
        }

        public ParticlesStartColors CopyFrom(ParticlesStartColors other)
        {
            Color = other.Color;
            MinColor = other.MinColor;
            MaxColor = other.MaxColor;
            if (other.Gradient == null) Gradient = null;
            else
            {
                Gradient ??= new Gradient();
                CopyGradient(other.Gradient, Gradient);
            }

            if (other.MinGradient == null) MinGradient = null;
            else
            {
                MinGradient ??= new Gradient();
                CopyGradient(other.MinGradient, MinGradient);
            }

            if (other.MaxGradient == null) MaxGradient = null;
            else
            {
                MaxGradient ??= new Gradient();
                CopyGradient(other.MaxGradient, MaxGradient);
            }
            return this;
        }

        public void ApplyTo(ParticleSystem particles)
        {
            ParticleSystem.MainModule main = particles.main;
            ParticleSystem.MinMaxGradient startColor = main.startColor;
            startColor.color = Color;
            startColor.colorMin = MinColor;
            startColor.colorMax = MaxColor;

            if (Gradient == null) startColor.gradient = null;
            else
            {
                startColor.gradient ??= new Gradient();
                CopyGradient(Gradient, startColor.gradient);
            }

            if (MinGradient == null) startColor.gradientMin = null;
            else
            {
                startColor.gradientMin ??= new Gradient();
                CopyGradient(MinGradient, startColor.gradientMin);
            }

            if (MaxGradient == null) startColor.gradientMax = null;
            else
            {
                startColor.gradientMax ??= new Gradient();
                CopyGradient(MaxGradient, startColor.gradientMax);
            }

            main.startColor = startColor;
        }

        public ParticlesStartColors Clone()
        {
            return new ParticlesStartColors().CopyFrom(this);
        }

        private void ScaleGradientAlpha(ref Gradient gradient, float alpha)
        {
            if (gradient == null) return;
            GradientAlphaKey[] keys = gradient.alphaKeys;
            if (keys.Length == 0) throw new Exception("Gradient has no alpha keys");

            for (int i = 0; i < keys.Length; i++)
            {
                GradientAlphaKey key = keys[i];
                key.alpha *= alpha;
                keys[i] = key;
            }
        }

        private static Gradient CopyGradient(Gradient source, Gradient dest)
        {
            GradientColorKey[] colorKeys = source.colorKeys;
            GradientAlphaKey[] alphaKeys = source.alphaKeys;
            var newColorKeys = new GradientColorKey[colorKeys.Length];
            var newAlphaKeys = new GradientAlphaKey[alphaKeys.Length];
            for (int i = 0; i < colorKeys.Length; i++)
            {
                newColorKeys[i] = colorKeys[i];
            }
            for (int i = 0; i < alphaKeys.Length; i++)
            {
                newAlphaKeys[i] = alphaKeys[i];
            }
            dest.SetKeys(newColorKeys, newAlphaKeys);
            dest.mode = source.mode;
            dest.colorSpace = source.colorSpace;
            return dest;
        }
    }
}
