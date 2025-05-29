namespace SensenToolkit
{
    public static class Env
    {
        public static bool IsDemoBuild
        {
#if SENSEN_DEMO_BUILD
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

        public static bool IsDebugBuild
        {
#if SENSEN_DEBUG_BUILD
            get => true;
#else
            get => false;
#endif
        }
    }
}
