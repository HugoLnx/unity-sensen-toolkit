using System;
using MyBox;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SensenToolkit
{
    public abstract class ASliderBindingBase : AUiBindingBase<FloatValueSO, float, float>
    {
        [SerializeField, AutoProperty] protected Slider Slider;

        protected override void BindUiChanges()
        {
            Slider.onValueChanged.AddListener(OnUiValueChanged);
        }

        protected override void UnbindUiChanges()
        {
            Slider.onValueChanged.RemoveListener(OnUiValueChanged);
        }

        protected override void SetUiValue(float value)
        {
            Slider.value = value;
        }

        protected override float GetUiValue()
        {
            return Slider.value;
        }
    }
}
