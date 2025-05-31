#if DOTWEEN
using DG.Tweening;
using EasyButtons;
using UnityEngine;

namespace SensenToolkit
{
    public class CameraService : ATransientSingleton<CameraService>
    {
        private const float MIN_DURATION = 0.3f;
        private const float MAX_DURATION = 0.6f;
        private const float MIN_DISTANCE = 0.01f;
        private const float MAX_DISTANCE = 0.04f;
        [SerializeField]
        private TransparencySortMode _transparencySortMode = TransparencySortMode.Default;
        public Camera MainCamera => _mainCamera = _mainCamera != null ? _mainCamera : Camera.main;
        private Camera _mainCamera;
        private Vector3? _originalPosition;
        private Tween _tween;

        private void Start()
        {
            MainCamera.transparencySortMode = _transparencySortMode;
        }

        [Button]
        public void Shake(float violence = 0f, float? durationOverride = null, float? distanceOverride = null)
        {
            if (_tween == null || !_tween.IsActive())
            {
                _originalPosition = MainCamera.transform.localPosition;
            }
            else
            {
                _tween.Kill();
                _tween = null;
            }

            float duration = durationOverride ?? Mathf.Lerp(MIN_DURATION, MAX_DURATION, violence);
            _tween = DOTween.Sequence()
                .Append(ScreenShake(MainCamera, violence, durationOverride, distanceOverride))
                .Append(MainCamera.transform.DOLocalMove(_originalPosition.Value, duration * 0.3f))
                .OnKill(() => _tween = null);
        }

        public static Tween ScreenShake(Camera camera, float violence = 0f, float? durationOverride = null, float? distanceOverride = null)
        {
            float duration = durationOverride ?? Mathf.Lerp(MIN_DURATION, MAX_DURATION, violence);
            float distance = distanceOverride ?? Mathf.Lerp(MIN_DISTANCE, MAX_DISTANCE, violence);
            return DOTween.Shake(
                getter: () => camera.transform.localPosition,
                setter: pos => camera.transform.localPosition = pos,
                duration: duration,
                strength: Vector2.one * distance,
                vibrato: 10,
                randomness: 90,
                fadeOut: true
            );
        }
    }
}
#endif

