using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SensenToolkit.Internal
{
    // No need to add those to the scene, as this class only contains static methods
    public class StaticRuntimeCallbacks : MonoBehaviour
    {
        // It's being called on AppCore to ensure it runs after AppCore is initialized
        // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void SubsystemRegistration()
        {
            CallStaticMethods<IAppCore_RuntimeInit_SubsystemRegistration_Internal>(
                nameof(IAppCore_RuntimeInit_SubsystemRegistration_Internal.AppCore_RuntimeInit_SubsystemRegistration_Internal)
            );
            CallStaticMethods<IAppCore_RuntimeInit_SubsystemRegistration>(
                nameof(IAppCore_RuntimeInit_SubsystemRegistration.AppCore_RuntimeInit_SubsystemRegistration)
            );
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void AfterAssembliesLoaded()
        {
            CallStaticMethods<IAppCore_RuntimeInit_AfterAssembliesLoaded>(
                nameof(IAppCore_RuntimeInit_AfterAssembliesLoaded.AppCore_RuntimeInit_AfterAssembliesLoaded)
            );
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BeforeSceneLoad()
        {
            CallStaticMethods<IAppCore_RuntimeInit_BeforeSceneLoad>(
                nameof(IAppCore_RuntimeInit_BeforeSceneLoad.AppCore_RuntimeInit_BeforeSceneLoad)
            );
        }

        public static void AppAwake()
        {
            CallStaticMethods<IAppCore_AppAwake_Internal>(
                nameof(IAppCore_AppAwake_Internal.AppCore_AppAwake_Internal)
            );
            CallStaticMethods<IAppCore_AppAwake>(
                nameof(IAppCore_AppAwake.AppCore_AppAwake)
            );
        }

        public static void AppQuit()
        {
            CallStaticMethods<IAppCore_AppQuit>(
                nameof(IAppCore_AppQuit.AppCore_AppQuit)
            );
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AfterSceneLoad()
        {
            CallStaticMethods<IAppCore_RuntimeInit_AfterSceneLoad>(
                nameof(IAppCore_RuntimeInit_AfterSceneLoad.AppCore_RuntimeInit_AfterSceneLoad)
            );
        }

        private static void CallStaticMethods<TInterface>(string methodName)
        {
            Type bootInterface = typeof(TInterface);

            // Get All non-abstract types that implement IAppCore_AppBoot
            IEnumerable<Type> appBootTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => bootInterface.IsAssignableFrom(type) && !type.IsAbstract && type.IsClass);

            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy;
            foreach (Type type in appBootTypes)
            {
                MethodInfo methodInfo = type.GetMethod(methodName, flags);
                methodInfo.Invoke(null, null);
            }
        }
    }
}
