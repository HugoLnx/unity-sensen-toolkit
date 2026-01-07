using System.Collections.Generic;
using System.Linq;
using SensenToolkit.EnvInternal;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
#endif
using UnityEngine;

namespace SensenToolkit
{
    public static class Env
    {
        private static (bool cached, string value) s_buildLocaleCode = (false, null);
        public static bool IsDemoBuild => RawIsDemoBuild || IsBoothBuild;
        public static bool IsDebugBuild => RawIsDebugBuild || IsEditor;
        public static bool IsProductionBuild => !(IsDebugBuild || IsBoothBuild || IsTrailerBuild);
        public static string BuildLocaleCode => ResolveBuildLocaleCode();

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

        public static IEnumerable<string> GetSymbols()
        {
#if UNITY_EDITOR
            BuildTargetGroup buildTarget = EditorUserBuildSettings.selectedBuildTargetGroup;
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(buildTarget);
            return PlayerSettings
                .GetScriptingDefineSymbols(namedBuildTarget)
                .Split(";")
                .Where(s => !string.IsNullOrWhiteSpace(s));
#else
            // TODO: Solve it in other way
            return new List<string>();
#endif
        }

        private static string ResolveBuildLocaleCode()
        {
            if (!s_buildLocaleCode.cached)
            {
                s_buildLocaleCode.value = FindBuildLocaleCode();
                s_buildLocaleCode.cached = true;
            }
            return s_buildLocaleCode.value;
        }

        private static string FindBuildLocaleCode()
        {
            Dictionary<string, string> localeEnvVars = EnvLocale.GetLocaleEnvVars();
            foreach (string symbol in GetSymbols())
            {
                if (localeEnvVars.TryGetValue(symbol, out string localeCode))
                {
                    return localeCode;
                }
            }
            return null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            s_buildLocaleCode = (false, null);
        }

    }
}
