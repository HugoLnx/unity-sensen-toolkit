using EasyButtons;
using MyBox;
using UnityEngine;

namespace SensenToolkit
{
    public class SpriteRendererAlpha : MonoBehaviour, ISingleAlpha
    {
        [SerializeField, AutoProperty] private SpriteRenderer _srenderer;
        [SerializeField] private float _forceInitialAlpha = -1f;
        [SerializeField, ReadOnly] private float _alpha = 1f;
        private float? _initialAlpha;
        private float InitialAlpha => EnsureInitialAlpha();

        public float Alpha
        {
            get => _alpha;
            set
            {
                _alpha = value;
                RefreshSpriteRenderer();
            }
        }

        private void OnEnable()
        {
            EnsureInitialAlpha();
            RefreshSpriteRenderer();
        }

        private void OnDisable()
        {
            _srenderer.color = _srenderer.color.WithAlpha(InitialAlpha);
        }

        private void RefreshSpriteRenderer()
        {
            if (!this.gameObject.activeInHierarchy) return;

            _srenderer.color = _srenderer.color.WithAlpha(InitialAlpha * _alpha);
        }

        [Button]
        private void SetAlpha(float alpha = 1f)
        {
            Alpha = alpha;
        }

        private float EnsureInitialAlpha()
        {
            if (_forceInitialAlpha > 0f) return _forceInitialAlpha;
            return _initialAlpha ??= _srenderer.color.a;
        }
    }
}
