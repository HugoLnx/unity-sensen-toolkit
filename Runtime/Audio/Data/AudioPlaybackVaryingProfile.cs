using System;
using SensenToolkit;
using UnityEngine;

namespace SensenToolkit
{
    [CreateAssetMenu(fileName = "AudioPlaybackVaryingProfile", menuName = "Sensen/Audio/PlaybackVaryingProfile", order = 1)]
    public class AudioPlaybackVaryingProfile : AudioPlaybackProfileBase
    {
        [SerializeField, Range(0f, 2f)] private float _minPitch = .5f;
        [SerializeField, Range(0f, 2f)] private float _maxPitch = 1.5f;

        [Tooltip("Pitch will have those steps between min and max")]
        [SerializeField] private int _pitchSteps = 5;

        [Tooltip("Changes pitch randomly or chooses circularly (Round-Robin)")]
        [SerializeField] private bool _randomPitch = true;

        [Tooltip("To automatic reset the circular getting pitch after a delay")]
        [SerializeField] private float _delayToResetPitch = 0f;

        private int _step;
        private float _getPitchTime;

        private RandomWithVariability _random;
        private RandomWithVariability VarRandom => _random ??= new(
            optionsAmount: _pitchSteps,
            percentReductionOnSelect: 0.25f,
            noSequentialRepetition: true
        );
        public override float ChoosePitch()
        {
            if (_randomPitch) SetRandomStep();
            else SetCircularStep();
            return Mathf.Lerp(_minPitch, _maxPitch, _step / (_pitchSteps - 1f));
        }

        private void SetRandomStep()
        {
            _step = VarRandom.Select();
        }

        private void SetCircularStep()
        {
            float timeSinceLastGetPitch = Time.time - _getPitchTime;
            _step = (_step + 1) % _pitchSteps;
            if (_delayToResetPitch > 0f && timeSinceLastGetPitch > _delayToResetPitch)
            {
                _step = 0;
            }
            _getPitchTime = Time.time;
        }
    }
}
