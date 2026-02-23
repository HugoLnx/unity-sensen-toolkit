using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace SensenToolkit
{
    public class Logx
    {
        private static readonly Dictionary<int, Logx> s_loggers = new();
        private static readonly HashSet<int> s_activeLoggers = new();

        private string _name;
        private int _id;

#if UNITY_EDITOR || SENSEN_DEBUG_BUILD
        // TODO: That isn't called if class doesn't inherits from MonoBehaviour,
        // so we need to find another way to initialize loggers in that case
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize()
        {
            s_activeLoggers.Clear();
            s_loggers.Clear();
            string[] activateAll = new[]{
                "", // Default
            };
            foreach (string loggerName in activateAll)
            {
                ActivateLogger(loggerName);
            }
        }
#endif

        public static Logx GetLogger(string loggerName = null, bool activate = false)
        {
#if UNITY_EDITOR || SENSEN_DEBUG_BUILD
            loggerName ??= "";
            int loggerId = loggerName.GetHashCode();
            if (activate || string.IsNullOrEmpty(loggerName))
            {
                ActivateLogger(loggerId);
            }
            if (s_loggers.TryGetValue(loggerId, out Logx logger)) return logger;
            logger = new(loggerName, loggerId);
            s_loggers.Add(logger._id, logger);
            return logger;
#else
            return null;
#endif
        }

        private Logx(string name, int? id = null)
        {
            _name = name;
            _id = id ?? name.GetHashCode();
        }

        [Conditional("UNITY_EDITOR"), Conditional("SENSEN_DEBUG_BUILD")]
        public void Info(object message, string loggerName = null, string category = null)
        {
            int loggerId;
            if (string.IsNullOrEmpty(loggerName))
            {
                loggerName = _name;
                loggerId = _id;
            }
            else
            {
                loggerId = loggerName.GetHashCode();
            }
            if (!s_activeLoggers.Contains(loggerId)) return;
            if (!string.IsNullOrEmpty(category))
            {
                loggerName = $"{loggerName}:{category}";
            }
            UnityEngine.Debug.Log($"[{loggerName}] {message}");
        }

        [Conditional("UNITY_EDITOR"), Conditional("SENSEN_DEBUG_BUILD")]
        public void Info<T>(object message)
        {
            Info(message, loggerName: typeof(T).ToString());
        }

        [Conditional("UNITY_EDITOR"), Conditional("SENSEN_DEBUG_BUILD")]
        public static void ActivateLogger<T>()
        {
            ActivateLogger(typeof(T).ToString());
        }

        [Conditional("UNITY_EDITOR"), Conditional("SENSEN_DEBUG_BUILD")]
        public static void ActivateLogger(string loggerName)
        {
            UnityEngine.Debug.Log($"Activating logger: {loggerName}");
            ActivateLogger(loggerName.GetHashCode());
        }

        [Conditional("UNITY_EDITOR"), Conditional("SENSEN_DEBUG_BUILD")]
        public static void ActivateLogger(int loggerId)
        {
            s_activeLoggers.Add(loggerId);
        }
    }
}
