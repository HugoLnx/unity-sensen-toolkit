using UnityEngine;

namespace SensenToolkit
{
    public class SpriteAlpha : IAlphaCompatible
    {
        private SpriteRenderer sprite;

        public float Alpha { get => sprite.color.a; set => sprite.color = sprite.color.WithAlpha(value); }
        public Transform Transform => sprite.transform;

        public SpriteAlpha(SpriteRenderer sprite)
        {
            this.sprite = sprite;
        }
    }
}
