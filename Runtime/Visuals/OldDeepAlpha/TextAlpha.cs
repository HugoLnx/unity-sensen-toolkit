using TMPro;
using UnityEngine;

namespace SensenToolkit
{
    public class TextAlpha : IAlphaCompatible
    {
        private TMP_Text text;

        public float Alpha { get => text.color.a; set => text.color = text.color.WithAlpha(value); }
        public Transform Transform => text.transform;

        public TextAlpha(TMP_Text text)
        {
            this.text = text;
        }
    }
}
