using UnityEngine;

namespace SensenToolkit
{
    public class SliderBinding100Positive : ASliderBindingGeneric
    {
        protected override float MinValue { get; set; } = 0f;
        protected override float MaxValue { get; set; } = 1f;
        protected override float ToUiMultiplier { get; set; } = 100f;
        protected override bool UiValueAsInt { get; set; } = true;
    }
}
