namespace SensenToolkit
{
    public static class Env
    {
        public static bool IsDemoBuild => RawIsDemoBuild || IsBoothBuild;
        public static bool IsDebugBuild => RawIsDebugBuild || IsEditor;
        public static bool IsProductionBuild => !(IsDebugBuild || IsBoothBuild || IsTrailerBuild);

        public static bool IsBoothBuild
        {
#if SENSEN_BOOTH_BUILD
            get => true;
#else
            get => false;
#endif
        }

        public static bool IsTrailerBuild
        {
#if SENSEN_TRAILER_BUILD
            get => true;
#else
            get => false;
#endif
        }

        public static bool IsEditor
        {
#if UNITY_EDITOR
            get => true;
#else
            get => false;
#endif
        }
        private static bool RawIsDemoBuild
        {
#if SENSEN_DEMO_BUILD
            get => true;
#else
            get => false;
#endif
        }

        private static bool RawIsDebugBuild
        {
#if SENSEN_DEBUG_BUILD
            get => true;
#else
            get => false;
#endif
        }
    }
}
