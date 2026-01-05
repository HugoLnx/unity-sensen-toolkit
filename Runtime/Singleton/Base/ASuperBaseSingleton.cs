using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace SensenToolkit.Internal
{
    [DefaultExecutionOrder(-1)]
    public abstract class ASuperBaseSingleton<T> : MonoBehaviour
    where T : ASuperBaseSingleton<T>
    {
        private static bool? s_isTransient;
        protected static bool IsTransient => s_isTransient ??= typeof(T).IsDefined(typeof(TransientSingletonAttribute), true);
        protected static bool IsPermanent => !IsTransient;
        protected abstract bool IsAlreadyInstanced { get; }
        public abstract bool IsActualSingleton { get; }
        public abstract bool IsExcessSingleton { get; }
        protected abstract string DescriptiveKey { get; }
        protected abstract bool ToSingletonInstance();

        private const bool ACTIVATE_LOGGER = false;
        private const string LOGGER_ID = "Singleton";
        private static Logx s_logger;
        protected static Logx Logger => s_logger ??= Logx.GetLogger(LOGGER_ID, activate: ACTIVATE_LOGGER);

        public Scene MyScene { get; private set; }

        private static object s_resetStaticsId;

        protected void InitializeAsSingleton()
        {
            LogInfo($"[{typeof(T).Name}] Initialize as Singleton", this as T);
            if (IsPermanent)
            {
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
            MyScene = gameObject.scene;
            AppCore.OnScenesBatchUnloadEnd += OnSceneBatchUnloadEnd;
        }

        private void OnSceneBatchUnloadEnd(HashSet<Scene> unloadedScenes)
        {
            if (!unloadedScenes.Contains(MyScene)) return;
            OnDestroyCleanup();
        }


        // Needed callbacks to avoid calling them on the excess instances.
        protected virtual void AwakeSingleton() { }
        protected virtual void OnDestroySingleton() { }
        protected virtual void OnDisableSingleton() { }
        protected virtual void OnApplicationQuitSingleton() { }

        protected virtual void AwakeExcess() { }
        protected virtual void OnDestroyExcess() { }
        protected virtual void OnDisableExcess() { }
        protected virtual void OnApplicationQuitExcess() { }

        protected virtual void AwakeAny() { }
        protected virtual void OnDestroyAny() { }
        protected virtual void OnDisableAny() { }
        protected virtual void OnApplicationQuitAny() { }
        protected virtual void OnDestroyCleanup()
        {
            AppCore.OnScenesBatchUnloadEnd -= OnSceneBatchUnloadEnd;
        }


        protected void Awake()
        {
            if (!IsAlreadyInstanced && (this as T).ToSingletonInstance())
            {
                LogInfo($"[{DescriptiveKey}] Binded awake instance", this as T);
            }

            AwakeAny();
            bool isQuitting = AppCore.IsAppQuitting || (IsTransient && AppCore.IsActiveSceneUnloading);
            if (IsActualSingleton && !isQuitting) AwakeSingleton();
            else
            {
                if (isQuitting)
                {
                    Debug.LogWarning($"{nameof(T)} Was awaken while scene is unloading or application was quitting");
                }
                Destroy(gameObject);
                AwakeExcess();
            }
        }

        protected void OnDestroy()
        {
            OnDestroyAny();
            if (IsActualSingleton)
            {
                OnDestroySingleton();
                TryDestroyCleanup();
            }
            else OnDestroyExcess();
        }

        protected void OnApplicationQuit()
        {
            OnApplicationQuitAny();
            if (IsActualSingleton)
            {
                OnApplicationQuitSingleton();
            }
            else OnApplicationQuitExcess();
        }

        private void TryDestroyCleanup()
        {
            if (
                IsPermanent // Doesn't need to cleanup on destroy if permanent
                || AppCore.IsAppQuitting // Doesn't need to cleanup if app is quitting
                || AppCore.IsSceneUnloading(gameObject.scene) // It'll be cleaned up with the scene unloading
            ) return;
            OnDestroyCleanup();
        }

        protected void OnDisable()
        {
            OnDisableAny();
            if (IsActualSingleton) OnDisableSingleton();
            else OnDisableExcess();
        }

        protected static void StaticSuperBaseResetStatics()
        {
            s_isTransient = null;
            s_logger = null;
        }

        protected static bool IsValidInstance(T instance)
        {
            return AppCore.IsAnySceneActive
                && instance != null
                && instance.gameObject != null
                && instance.gameObject.scene != null
                && AppCore.IsSceneActive(instance.gameObject.scene);
        }

        protected static string InstanceValidityDescription(T instance)
        {
            if (instance == null) return "instance is null";
            if (instance.gameObject == null) return "gameObject is null";

            Scene scene = instance.gameObject.scene;
            if (scene == null) return "scene is null";
            if (!AppCore.IsSceneActive(scene))
            {
                return $"{Scenex.DescribeScene(scene)} is NOT ACTIVE (active:{AppCore.ActiveScenes.Count})";
            }
            return "instance is valid";
        }

        protected static void LogInfo(string message, T instance = null)
        {
            Logger.Info($"[{(instance != null ? instance.DescriptiveKey : typeof(T).Name)}] {message}");
        }
    }
}
