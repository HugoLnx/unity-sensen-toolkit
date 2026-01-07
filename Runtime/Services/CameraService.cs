using UnityEngine;

namespace SensenToolkit
{
    public class CameraService : ATransientSingleton<CameraService>
    {
        [SerializeField]
        private TransparencySortMode _transparencySortMode = TransparencySortMode.Default;
        public Camera MainCamera => _mainCamera = _mainCamera != null ? _mainCamera : Camera.main;
        private Camera _mainCamera;

        private void Start()
        {
            MainCamera.transparencySortMode = _transparencySortMode;
        }
    }
}
