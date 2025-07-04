using UnityEngine;

namespace SensenToolkit
{
    public abstract class ASliderBindingGeneric : ASliderBindingBase
    {
        protected abstract float MinValue { get; set; }
        protected abstract float MaxValue { get; set; }
        protected abstract float ToUiMultiplier { get; set; }
        protected abstract bool UiValueAsInt { get; set; }
        private bool IgnoreMultiplier => Mathf.Approximately(ToUiMultiplier, 0f) || Mathf.Approximately(ToUiMultiplier, 1f);

        protected override float FromUIValue(float uiValue)
        {
            if (UiValueAsInt)
            {
                uiValue = Mathf.Round(uiValue);
            }
            float value = IgnoreMultiplier ? uiValue : uiValue / ToUiMultiplier;
            return Mathf.Clamp(value, MinValue, MaxValue);
        }

        protected override float ToUIValue(float value)
        {
            value = Mathf.Clamp(value, MinValue, MaxValue);
            float uiValue = IgnoreMultiplier ? value : value * ToUiMultiplier;
            if (UiValueAsInt)
            {
                uiValue = Mathf.Round(uiValue);
            }
            return uiValue;
        }
    }

    // public class Slider100OnlyPositiveBinding : ASliderValueBinding
    // {
    //     private const float MinValue = 0f;
    //     private const float MaxValue = 1f;
    //     private const int MinUIValue = 0;
    //     private const int MaxUIValue = 100;
    //     private const float MultiplierToUI = 100f;

    //     protected override float FromUIValue(float uiValue)
    //     {
    //         return Mathf.Clamp(uiValue / MultiplierToUI, MinValue, MaxValue);
    //     }

    //     protected override float ToUIValue(float value)
    //     {
    //         return Mathf.Clamp(Mathf.RoundToInt(value * MultiplierToUI), MinUIValue, MaxUIValue);
    //     }
    // }

    // public class Slider100IncludeNegativeBinding : ASliderValueBinding
    // {
    //     private const float MinValue = -1f;
    //     private const float MaxValue = 1f;
    //     private const int MinUIValue = -100;
    //     private const int MaxUIValue = 100;
    //     private const float MultiplierToUI = 100f;

    //     protected override float FromUIValue(float uiValue)
    //     {
    //         return Mathf.Clamp(uiValue / MultiplierToUI, MinValue, MaxValue);
    //     }

    //     protected override float ToUIValue(float value)
    //     {
    //         return Mathf.Clamp(Mathf.RoundToInt(value * MultiplierToUI), MinUIValue, MaxUIValue);
    //     }
    // }
}
