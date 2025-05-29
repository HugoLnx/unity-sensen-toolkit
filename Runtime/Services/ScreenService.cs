using SensenToolkit;
using UnityEngine;

namespace Bumashuta
{
    public class ScreenService : APermanentSingleton<ScreenService>
    {
        [SerializeField] private FullScreenMode _mode;
        [SerializeField] private int _width = 640;
        [SerializeField] private int _height = 480;
        [SerializeField] private bool _autoDetectResolution = true;

        protected override void AwakeSingleton()
        {
            base.AwakeSingleton();
            if (_autoDetectResolution)
            {
                _width = Screen.currentResolution.width;
                _height = Screen.currentResolution.height;
            }
            Screen.SetResolution(_width, _height, _mode);
        }
    }
}
