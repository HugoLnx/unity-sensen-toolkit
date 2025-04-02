#if DOTWEEN
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SensenToolkit
{
    public class PostprocessingService : ATransientSingleton<PostprocessingService>
    {
        public Volume Volume => _volume = _volume == null
            ? GetComponent<Volume>()
            : _volume;
        private Volume _volume;

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

        private T EnsureEffect<T>() where T : VolumeComponent
        {
            if (Volume.profile.TryGet(out T effect)) return effect;
            effect = Volume.profile.Add<T>(true);
            effect.active = false;
            return effect;
        }
    }
}
#endif

