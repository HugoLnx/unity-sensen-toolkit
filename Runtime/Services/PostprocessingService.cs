#if DOTWEEN
using MyBox;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SensenToolkit
{
    public class PostprocessingService : ATransientSingleton<PostprocessingService>
    {
        [SerializeField, AutoProperty] private Volume _volume;
        public Volume Volume => _volume = _volume == null
            ? GetComponent<Volume>()
            : _volume;

        public ColorAdjustments ColorAdjustments => _colorAdjustments == null
            ? EnsureEffect<ColorAdjustments>()
            : _colorAdjustments;
        private ColorAdjustments _colorAdjustments;

        public DepthOfField DepthOfField => _depthOfField == null
            ? EnsureEffect<DepthOfField>()
            : _depthOfField;
        private DepthOfField _depthOfField;

        public Bloom Bloom => _bloom == null
            ? EnsureEffect<Bloom>()
            : _bloom;
        private Bloom _bloom;

        public MotionBlur MotionBlur => _motionBlur == null
            ? EnsureEffect<MotionBlur>()
            : _motionBlur;
        private MotionBlur _motionBlur;

        public LensDistortion LensDistortion => _lensDistortion == null
            ? EnsureEffect<LensDistortion>()
            : _lensDistortion;
        private LensDistortion _lensDistortion;

        public SplitToning SplitToning => _splitToning == null
            ? EnsureEffect<SplitToning>()
            : _splitToning;
        private SplitToning _splitToning;

        public LiftGammaGain LiftGammaGain => _liftGammaGain == null
            ? EnsureEffect<LiftGammaGain>()
            : _liftGammaGain;
        private LiftGammaGain _liftGammaGain;

        private T EnsureEffect<T>() where T : VolumeComponent
            => EnsureEffect<T>(_volume);

        public static T EnsureEffect<T>(Volume volume) where T : VolumeComponent
        {
            if (volume == null) return null;
            if (volume.profile.TryGet(out T effect)) return effect;
            effect = volume.profile.Add<T>(true);
            effect.active = false;
            return effect;
        }
    }
}
#endif

