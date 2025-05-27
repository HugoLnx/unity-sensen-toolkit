using System;
using System.Collections;
using EasyButtons;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SensenToolkit
{
    public class ButtonTextStyling : MonoBehaviour
    {
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _disabledColor = Color.red;

        [SerializeField, AutoProperty(AutoPropertyMode.Parent)]
        private Button _button;

        [SerializeField, AutoProperty(AutoPropertyMode.Children)]
        private TMP_Text _text;

        private IEnumerator Start()
        {
            yield return null;
            yield return null;
            RefreshStyling();
        }

        [Button]
        private void RefreshStyling()
        {
            _text.color = _button.interactable ? _normalColor : _disabledColor;
        }
    }
}
