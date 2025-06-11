#if DOTWEEN
using DG.Tweening;
using EasyButtons;
using UnityEngine;

namespace SensenToolkit
{
    public class ScreenShakeService : APermanentSingleton<ScreenShakeService>
    {
        private const float MIN_DURATION = 0.3f;
        private const float MAX_DURATION = 0.6f;
        private const float MIN_DISTANCE = 0.01f;
        private const float MAX_DISTANCE = 0.04f;

        [SerializeField] private bool _isEnabled = true;
        public bool IsEnabled { get => _isEnabled; set => _isEnabled = value; }
        private Vector3? _cameraOriginalPosition;
        private Tween _tween;

        [Button]
        public void Shake(float violence = 0f, float? durationOverride = null, float? distanceOverride = null)
        {
            if (!IsEnabled) return;
            Camera camera = CameraService.Instance.MainCamera;
            if (_tween == null || !_tween.IsActive())
            {
                _cameraOriginalPosition = camera.transform.localPosition;
            }
            else
            {
                _tween.Kill();
                _tween = null;
            }

            float duration = durationOverride ?? Mathf.Lerp(MIN_DURATION, MAX_DURATION, violence);
            _tween = DOTween.Sequence()
                .Append(ScreenShake(camera, violence, durationOverride, distanceOverride))
                .Append(camera.transform.DOLocalMove(_cameraOriginalPosition.Value, duration * 0.3f))
                .OnKill(() => _tween = null);
        }

        private static Tween ScreenShake(Camera camera, float violence = 0f, float? durationOverride = null, float? distanceOverride = null)
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

