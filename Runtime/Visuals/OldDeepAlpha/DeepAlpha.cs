#if DOTWEEN
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using EasyButtons;
using TMPro;

namespace SensenToolkit
{
    public class DeepAlpha : MonoBehaviour
    {
        [System.Serializable]
        private struct ImageAlphaMapping
        {
            public Image Image { get; set; }
            public float Max { get; set; }
            public float Min { get; set; }
            public bool Ignore { get; set; }
        }
        [System.Serializable]
        private struct SpriteAlphaMapping
        {
            public SpriteRenderer Sprite { get; set; }
            public float Max { get; set; }
            public float Min { get; set; }
            public bool Ignore { get; set; }
        }
        [System.Serializable]
        private struct TextAlphaMapping
        {
            public TMP_Text Text { get; set; }
            public float Max { get; set; }
            public float Min { get; set; }
            public bool Ignore { get; set; }
        }
        [System.Serializable]
        private struct ParentAlphaMapping
        {
            public Transform Parent { get; set; }
            public float Max { get; set; }
            public float Min { get; set; }
            public bool Ignore { get; set; }
        }
        [System.Serializable]
        private struct ObjectAlphaMapping
        {
            public IAlphaCompatible Obj { get; set; }
            public float Max { get; set; }
            public float Min { get; set; }
            public bool Ignore { get; set; }

            public static ObjectAlphaMapping From(ImageAlphaMapping imgMapping) => new()
            {
                Obj = new ImageAlpha(imgMapping.Image),
                Max = imgMapping.Max,
                Min = imgMapping.Min,
                Ignore = imgMapping.Ignore,
            };
            public static ObjectAlphaMapping From(SpriteAlphaMapping spriteMapping) => new()
            {
                Obj = new SpriteAlpha(spriteMapping.Sprite),
                Max = spriteMapping.Max,
                Min = spriteMapping.Min,
                Ignore = spriteMapping.Ignore,
            };
            public static ObjectAlphaMapping From(TextAlphaMapping textMapping) => new()
            {
                Obj = new TextAlpha(textMapping.Text),
                Max = textMapping.Max,
                Min = textMapping.Min,
                Ignore = textMapping.Ignore,
            };
        }
        [System.NonSerialized] private IAlphaCompatible[] _objs;
        [SerializeField] private ImageAlphaMapping[] _customImgMapping;
        [SerializeField] private SpriteAlphaMapping[] _customSpriteMapping;
        [SerializeField] private TextAlphaMapping[] _customTextMapping;
        [SerializeField] private ParentAlphaMapping[] _customParentMapping;
        [System.NonSerialized] private ObjectAlphaMapping[] _customMapping;
        private Dictionary<IAlphaCompatible, ObjectAlphaMapping> _mappings = new();
        private Dictionary<Transform, ParentAlphaMapping> _parentMappings = new();
        private Dictionary<Image, IAlphaCompatible> _cachedImageAlpha = new();
        private Dictionary<SpriteRenderer, IAlphaCompatible> _cachedSpriteAlpha = new();
        private Dictionary<TMP_Text, IAlphaCompatible> _cachedTextAlpha = new();

        public float Alpha
        {
            get
            {
                if (!Application.isPlaying) Awake();
                if (this._objs.Length > 0)
                {
                    return GetObjectRelativeAlpha(this._objs[0]);
                }

                return 0;
            }

            set
            {
                if (!Application.isPlaying) Awake();
                foreach (IAlphaCompatible obj in _objs)
                {
                    SetObjectAlpha(obj, value);
                }
            }
        }

        private void Awake()
        {
            Remap();
        }

        public void Remap()
        {
            if (_customParentMapping != null)
            {
                foreach (ParentAlphaMapping parentMapping in _customParentMapping)
                {
                    this._parentMappings[parentMapping.Parent] = parentMapping;
                }
            }
            if (_customImgMapping != null)
            {
                foreach (ImageAlphaMapping imgMapping in _customImgMapping)
                {
                    var objMapping = ObjectAlphaMapping.From(imgMapping);
                    _mappings[objMapping.Obj] = objMapping;
                    _cachedImageAlpha[imgMapping.Image] = objMapping.Obj;
                }
            }

            if (_customSpriteMapping != null)
            {
                foreach (SpriteAlphaMapping spriteMapping in _customSpriteMapping)
                {
                    var objMapping = ObjectAlphaMapping.From(spriteMapping);
                    _mappings[objMapping.Obj] = objMapping;
                    _cachedSpriteAlpha[spriteMapping.Sprite] = objMapping.Obj;
                }
            }

            if (_customTextMapping != null)
            {
                foreach (TextAlphaMapping textMapping in _customTextMapping)
                {
                    var objMapping = ObjectAlphaMapping.From(textMapping);
                    _mappings[objMapping.Obj] = objMapping;
                    _cachedTextAlpha[textMapping.Text] = objMapping.Obj;
                }
            }

            Image[] images = GetComponentsInChildren<Image>();
            SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();
            TMP_Text[] texts = GetComponentsInChildren<TMP_Text>();
            this._objs = new IAlphaCompatible[images.Length + sprites.Length + texts.Length];
            int i = 0;
            foreach (Image img in images)
            {
                this._objs[i++] = _cachedImageAlpha
                    .GetValueOrDefault(img, new ImageAlpha(img));
            }
            foreach (SpriteRenderer sprite in sprites)
            {
                this._objs[i++] = _cachedSpriteAlpha
                    .GetValueOrDefault(sprite, new SpriteAlpha(sprite));
            }
            foreach (TMP_Text text in texts)
            {
                this._objs[i++] = _cachedTextAlpha
                    .GetValueOrDefault(text, new TextAlpha(text));
            }
        }

        [Button]
        private void ManualSetAlpha(float alpha)
        {
            this.Alpha = alpha;
        }

        private void SetObjectAlpha(IAlphaCompatible obj, float alpha)
        {
            float absAlpha;
            Transform parent = obj.Transform.parent;
            if (_mappings.ContainsKey(obj))
            {
                ObjectAlphaMapping map = _mappings[obj];
                if (map.Ignore) return;
                absAlpha = Mathf.Lerp(map.Min, map.Max, alpha);
            }
            else if (_parentMappings.ContainsKey(parent) || _parentMappings.ContainsKey(obj.Transform))
            {
                ParentAlphaMapping map = _parentMappings.ContainsKey(parent)
                    ? _parentMappings[parent]
                    : _parentMappings[obj.Transform];
                if (map.Ignore) return;
                absAlpha = Mathf.Lerp(map.Min, map.Max, alpha);
            }
            else
            {
                absAlpha = alpha;
            }
            obj.Alpha = absAlpha;
        }

        private float GetObjectRelativeAlpha(IAlphaCompatible obj)
        {
            Transform parent = obj.Transform.parent;
            if (_mappings.ContainsKey(obj))
            {
                ObjectAlphaMapping map = _mappings[obj];
                return Mathf.InverseLerp(map.Min, map.Max, obj.Alpha); //(obj.Alpha - map.min) / (map.max - map.min);
            }
            else if (_parentMappings.ContainsKey(parent) || _parentMappings.ContainsKey(obj.Transform))
            {
                ParentAlphaMapping map = _parentMappings.ContainsKey(parent)
                    ? _parentMappings[parent]
                    : _parentMappings[obj.Transform];
                return Mathf.InverseLerp(map.Min, map.Max, obj.Alpha); //(obj.Alpha - map.min) / (map.max - map.min);
            }
            else
            {
                return obj.Alpha;
            }
        }

        public Tween FadeIn(float duration, Ease ease = Ease.InSine)
            => Tweenx
                .FromTo(
                    v => Alpha = v,
                    duration: duration,
                    startValue: Alpha,
                    endValue: 1f,
                    setImmediately: true
                )
                .SetEase(ease);

        public Tween FadeOut(float duration, Ease ease = Ease.OutSine)
            => Tweenx.FromTo(
                v => Alpha = v,
                duration: duration,
                startValue: Alpha,
                endValue: 0f,
                setImmediately: true
            )
            .SetEase(ease);
    }
}
#endif
