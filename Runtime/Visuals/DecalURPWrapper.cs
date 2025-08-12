using System.Diagnostics;
using DG.Tweening;
using EasyButtons;
using MyBox;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SensenToolkit
{
    [RequireComponent(typeof(DecalProjector))]
    public class DecalURPWrapper : MonoBehaviour
    {
        [SerializeField] private int _gridCellsXCount = 4;
        [SerializeField] private int _gridCellsYCount = 4;
        [SerializeField, AutoProperty] private DecalProjector _decal;
        private Vector2 CellSize => new(1f / _gridCellsXCount, 1f / _gridCellsYCount);
        public Vector3 Size3D => _decal.size;
        public Vector2 Size
        {
            get => Size3D.XY();
            set => SetSize(value);
        }
        public DecalProjector Projector => _decal;
        private Tween _fadeTween;
        public Vector2 OriginalSize { get; private set; }

        public float Opacity
        {
            get => _decal.fadeFactor;
            set => _decal.fadeFactor = value;
        }

        private void Awake()
        {
            OriginalSize = Size;
        }

        private void Start()
        {
            _decal.uvScale = CellSize;
        }

        public void EnsureVisibleForDuration(float duration, float fadeOutDuration, Ease fadeOutEase)
        {
            Tweenx.KillAndNullify(ref _fadeTween);
            _fadeTween = DOTween.Sequence()
                .AppendInterval(duration)
                .Append(Tweenx.FromOneToZero(
                    action: v => this.Opacity = v,
                    duration: fadeOutDuration,
                    setImmediately: true
                ).SetEase(fadeOutEase))
                .Play();
        }

        [Button]
        private void SetSize(Vector2 size)
        {
            _decal.size = _decal.size.With(x: size.x, y: size.y);
        }

        [Button]
        public void SetTile(Vector2Int coords)
        {
            EditorPrepare();
            Vector2 size = CellSize;
            Vector2 offset = new(coords.x * size.x, coords.y * size.y);
            _decal.uvBias = offset;
        }

        [Button]
        public void SetRandomTile()
        {
            Vector2Int randomCellCoords = new(
                Random.Range(0, _gridCellsXCount),
                Random.Range(0, _gridCellsYCount)
            );
            SetTile(randomCellCoords);
        }

        [Conditional("UNITY_EDITOR")]
        private void EditorPrepare()
        {
#if UNITY_EDITOR
            if (Application.isPlaying) return;
            Start();
#endif
        }
    }
}
