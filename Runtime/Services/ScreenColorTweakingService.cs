using System;
using MyBox;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SensenToolkit
{
    public class ScreenColorTweakingService : APermanentSingleton<ScreenColorTweakingService>
    {
        private readonly int _brightnessPropertyId = Shader.PropertyToID("_Brightness");
        private readonly int _contrastPropertyId = Shader.PropertyToID("_Contrast");
        [SerializeField, Range(-1f, 1f)] private float _brightness = 0f;
        [SerializeField, Range(-1f, 1f)] private float _contrast = 0f;
        [SerializeField, Range(-1f, 1f)] private float _gamma = 0f;

        [SerializeField, AutoProperty(AutoPropertyMode.Scene)]
        private PostprocessingService _pp;
        [SerializeField, MustBeAssigned] private Material _fullscreenMaterial;

        public float Brightness { get => _brightness; set => SetBrightness(value); }
        public float Contrast { get => _contrast; set => SetContrast(value); }
        public float Gamma { get => _gamma; set => SetGamma(value); }

        private void OnEnable()
        {
            PostprocessingService.AddAssignListener(OnNewPostProcessingService);
        }

        protected override void OnDisableSingleton()
        {
            PostprocessingService.RemoveAssignListener(OnNewPostProcessingService);
        }

        private void OnNewPostProcessingService(PostprocessingService service)
        {
            if (service == null) return;
            _pp = service;
            RefreshAllEffects();
        }

        private void OnValidate()
        {
            RefreshPostProcessingService();
            RefreshAllEffects();
        }

        private void SetBrightness(float value)
        {
            _brightness = value;
            RefreshBrightness();
        }

        private void SetContrast(float value)
        {
            _contrast = value;
            RefreshContrast();
        }

        private void SetGamma(float value)
        {
            _gamma = value;
            RefreshGamma();
        }

        private void RefreshGamma()
        {
            RefreshPostProcessingService();
            if (_pp == null) return;
            _gamma = Mathf.Clamp(_gamma, -1f, 1f);
            bool isActive = _gamma >= 0.01f || _gamma <= -0.01f;
            LiftGammaGain gammaObj = _pp.LiftGammaGain;
            gammaObj.active = isActive;
            if (!isActive) return;

            float val = _gamma;
            gammaObj.gamma.value = new Vector4(val, val, val, val);
        }

        private void RefreshBrightness()
        {
            _brightness = Mathf.Clamp(_brightness, -1f, 1f);
            if (_fullscreenMaterial == null) return;
            _fullscreenMaterial.SetFloat(_brightnessPropertyId, _brightness);
        }

        private void RefreshContrast()
        {
            _contrast = Mathf.Clamp(_contrast, -1f, 1f);
            if (_fullscreenMaterial == null) return;
            float val = 1f + _contrast;
            _fullscreenMaterial.SetFloat(_contrastPropertyId, val);
        }

        private void RefreshPostProcessingService()
        {
            if (!Application.isPlaying)
            {
                _pp = FindFirstObjectByType<PostprocessingService>();
                return;
            }
            _pp = PostprocessingService.Instance;
        }

        private void RefreshAllEffects()
        {
            RefreshGamma();
            RefreshBrightness();
            RefreshContrast();
        }
    }
}
