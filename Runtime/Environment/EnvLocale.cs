
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace SensenToolkit.EnvInternal
{
    public static class EnvLocale
    {
        private static Dictionary<string, string> s_localeEnvVars;
        private static readonly HashSet<string> s_envVarPrefixes = new()
        {
            "LOCALE_", "LOC_", "LANG_"
        };

        public static Dictionary<string, string> GetLocaleEnvVars()
        {
            if (s_localeEnvVars != null) return s_localeEnvVars;
            s_localeEnvVars = new();
            foreach (Locale locale in LocalizationSettings.AvailableLocales.Locales)
            {
                foreach (string prefix in s_envVarPrefixes)
                {
                    string langCode = locale.Identifier.Code;
                    string envVarName1 = $"{prefix}{langCode.ToUpper().Replace('-', '_')}";
                    string envVarName2 = $"{prefix}{langCode.ToUpper().Replace("-", "")}";
                    s_localeEnvVars[envVarName1] = langCode;
                    s_localeEnvVars[envVarName2] = langCode;
                }
            }

            return s_localeEnvVars;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            s_localeEnvVars = null;
        }
    }
}
