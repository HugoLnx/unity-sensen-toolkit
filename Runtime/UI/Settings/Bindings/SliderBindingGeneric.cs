using UnityEngine;

namespace SensenToolkit
{
    public class SliderBindingGeneric : ASliderBindingGeneric
    {
        [field: SerializeField] protected override float MinValue { get; set; } = 0f;
        [field: SerializeField] protected override float MaxValue { get; set; } = 1f;
        [field: SerializeField] protected override float ToUiMultiplier { get; set; } = 1f;
        [field: SerializeField] protected override bool UiValueAsInt { get; set; } = false;
    }
}
