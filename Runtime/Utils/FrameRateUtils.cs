using UnityEngine;

namespace SensenToolkit
{
    public static class FrameRateUtils
    {
        public static int TargetFrameRate
        {
            get => Application.targetFrameRate;
            set => Application.targetFrameRate = Mathf.Clamp(value, 0, 120);
        }

        public static bool VSync
        {
            get => QualitySettings.vSyncCount > 0;
            set => QualitySettings.vSyncCount = value ? 1 : 0;
        }
    }
}
