#if DOTWEEN
using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;

namespace SensenToolkit
{
    public static class Tweenx
    {
        public static Tween FromTo(
            System.Action<float> action,
            float duration,
            float? startValue = null,
            float endValue = 1f,
            bool setImmediately = false
        )
        {
            float gv = startValue ?? 0f;
            TweenerCore<float, float, FloatOptions> tween = DOTween.To(
                getter: () => gv,
                setter: v =>
                {
                    gv = v;
                    action(gv);
                },
                endValue: endValue,
                duration: duration
            );
            if (startValue.HasValue)
            {
                tween = tween.From(startValue.Value, setImmediately: setImmediately);
            }
            return tween;
        }

        public static Tween FromZeroToOne(System.Action<float> action, float duration, bool setImmediately = false)
        {
            return FromTo(action, duration, startValue: 0f, endValue: 1f, setImmediately: setImmediately);
        }

        public static Sequence Flash(
            System.Action<float> action,
            float durationIn = 0.1f,
            float durationOut = 0.1f,
            float lowValue = 0f,
            float highValue = 1f,
            float? startValue = null,
            Ease easeIn = Ease.OutSine,
            Ease easeOut = Ease.InSine
        )
        {
            startValue ??= lowValue;
            return DOTween.Sequence()
                .Append(FromTo(action, duration: durationIn, endValue: highValue, startValue: startValue).SetEase(easeIn))
                .Append(FromTo(action, duration: durationOut, endValue: lowValue, startValue: highValue).SetEase(easeOut))
                .Play();
        }

        public static void KillAndNullify(ref Tween tween)
        {
            if (tween != null && tween.IsActive())
            {
                tween.Kill();
            }
            tween = null;
        }
    }
}
#endif
