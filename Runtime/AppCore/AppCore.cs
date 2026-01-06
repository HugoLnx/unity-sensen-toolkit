using System;
using System.Collections.Generic;
using EasyButtons;
using SensenToolkit.Internal;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SensenToolkit
{
    [DefaultExecutionOrder(AppCore.CORE_ORDER)]
    public class AppCore : MonoBehaviour
    {
        /*
        # Callbacks Order
        * OnAppBootingStart (once per app lifetime)
        * OnScenesBatchLoadStart (when loading multiple scenes)
        * OnSceneLoadStart (before all awakes/enables of the scene)
        * OnSceneLoadEnd (before all starts of the scene)
        * OnScenesBatchLoadEnd (after all scenes were loaded)
        * OnAppBootingEnd (once per app lifetime)

        * OnAppQuittingStart (once per app lifetime)
        * OnScenesBatchUnloadStart (when unloading multiple scenes)
        * OnSceneUnloadStart (before all disables of the scene)
        * OnSceneUnloadEnd (after all destroys of the scene)
        * OnScenesBatchUnloadEnd (after all scenes were unloaded)
        * OnAppQuittingEnd (once per app lifetime)
        */
        private const int CORE_ORDER = -999999;
        public const int BEFORE_ORDER = -999998;
        public const int AFTER_ORDER = 999999;
        private const bool ACTIVATE_LOGGER = false;
        private const string LOGGER_ID = "AppCore";
        private static Logx s_logger;
        protected static Logx Logger => s_logger ??= Logx.GetLogger(LOGGER_ID, activate: ACTIVATE_LOGGER);
        private static AppCore s_instance;
        private static readonly HashSet<int> s_lifetimeActions = new();
        private static readonly HashSet<int> s_sceneActions = new();
        private static readonly HashSet<Scene> s_activeScenes = new();
        private static readonly HashSet<Scene> s_loadingScenes = new();
        private static readonly HashSet<Scene> s_unloadingScenes = new();
        private static readonly HashSet<Scene> s_scenesBatchLoaded = new();
        private static readonly HashSet<Scene> s_scenesBatchUnloaded = new();
        private static readonly HashSet<GameObject> s_appCoreBooted = new();

        public static event Action<Scene> OnSceneLoadStart = delegate { };
        public static event Action<Scene> OnSceneLoadEnd = delegate { };
        public static event Action<Scene> OnSceneUnloadStart = delegate { };
        public static event Action<Scene> OnSceneUnloadEnd = delegate { };
        public static event Action OnScenesBatchUnloadStart = delegate { };
        public static event Action<HashSet<Scene>> OnScenesBatchUnloadEnd = delegate { };
        public static event Action OnScenesBatchLoadStart = delegate { };
        public static event Action<HashSet<Scene>> OnScenesBatchLoadEnd = delegate { };
        public static event Action OnAppQuittingStart = delegate { };
        public static event Action OnAppQuittingEnd = delegate { };
        public static event Action OnAppBootingStart = delegate { };
        public static event Action OnAppBootingEnd = delegate { };

        public static bool IsAnySceneLoading => s_loadingScenes.Count > 0;
        public static bool IsAnySceneUnloading => s_unloadingScenes.Count > 0;
        public static bool IsAnySceneActive => s_activeScenes.Count > 0;
        public static bool IsActiveSceneLoading => IsSceneLoading(SceneManager.GetActiveScene());
        public static bool IsActiveSceneUnloading => IsSceneUnloading(SceneManager.GetActiveScene());
        public static bool IsAppBooting => IsScenesBatchLoading && !IsAppBooted;
        public static bool IsAppBooted { get; private set; }
        public static bool IsAppQuitting { get; private set; }
        public static bool IsScenesBatchLoading { get; private set; }
        public static bool IsScenesBatchUnloading { get; private set; }
        private static bool s_rawIsRuntime = false;
        public static bool IsRuntime => Application.isPlaying && s_rawIsRuntime;
        public static IReadOnlyCollection<Scene> ActiveScenes => s_activeScenes;

        [SerializeField] private Transform _callbacksContainer;

        public static void RunOnlyOnce(object id, Action action)
        {
            if (action == null) return;
            int hash = id.GetHashCode();
            if (s_lifetimeActions.Contains(hash)) return;
            s_lifetimeActions.Add(hash);
            action();
        }

        /// <summary>
        /// Run the action only once in the game lifetime. Don't pass a instance method directly, use a lambda or a static method.
        /// </summary>
        public static void RunOnlyOnce(System.Action action)
        {
            RunOnlyOnce(action, action);
        }

        public static void RunOncePerScene(object id, System.Action action)
        {
            if (action == null) return;
            int hash = id.GetHashCode();
            if (s_sceneActions.Contains(hash)) return;
            s_sceneActions.Add(hash);
            action();
        }

        public static void RunOncePerScene(System.Action action)
        {
            RunOncePerScene(action, action);
        }

        public static bool IsSceneActive(Scene scene)
        {
            if (Scenex.IsEmptyScene(scene)) return false;
            return s_activeScenes.Contains(scene) || (
                s_activeScenes.Count > 0
                && Scenex.IsDontDestroyOnLoadScene(scene)
            );
        }

        public static bool IsSceneLoading(Scene scene)
        {
            if (scene == null) return false;
            return s_loadingScenes.Contains(scene);
        }

        public static bool IsSceneUnloading(Scene scene)
        {
            if (scene == null) return false;
            return s_unloadingScenes.Contains(scene);
        }

        // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        // private static void BeforeSceneLoad()
        // {
        //     Debug.Log("RuntimeInitialize: BeforeSceneLoad");
        // }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize()
        {
            s_logger = null;
            s_lifetimeActions.Clear();
            s_sceneActions.Clear();
            s_activeScenes.Clear();
            s_loadingScenes.Clear();
            s_unloadingScenes.Clear();
            s_scenesBatchLoaded.Clear();
            s_scenesBatchUnloaded.Clear();
            s_appCoreBooted.Clear();
            OnSceneLoadStart = delegate { };
            OnSceneLoadEnd = delegate { };
            OnSceneUnloadStart = delegate { };
            OnSceneUnloadEnd = delegate { };
            OnAppBootingStart = delegate { };
            OnAppBootingEnd = delegate { };
            OnAppQuittingStart = delegate { };
            OnAppQuittingEnd = delegate { };
            OnScenesBatchLoadStart = delegate { };
            OnScenesBatchLoadEnd = delegate { };
            OnScenesBatchUnloadStart = delegate { };
            OnScenesBatchUnloadEnd = delegate { };
            IsAppBooted = false;
            IsAppQuitting = false;
            IsScenesBatchLoading = false;
            IsScenesBatchUnloading = false;
            s_rawIsRuntime = true;

            OnScenesBatchLoadStart += () =>
            {
                s_sceneActions.Clear();
            };

            // This is needed to avoid the event being added multiple times when
            // domain reload is disabled on Editor
            Application.quitting -= ApplicationQuittingCallback;
            Application.quitting += ApplicationQuittingCallback;
            SceneManager.sceneLoaded -= SceneLoadedCallback;
            SceneManager.sceneLoaded += SceneLoadedCallback;
            SceneManager.activeSceneChanged -= ActiveSceneChangedCallback;
            SceneManager.activeSceneChanged += ActiveSceneChangedCallback;

            StaticRuntimeCallbacks.SubsystemRegistration();
        }

        private void OnValidate() => TryBootCore();
        private void Awake() => TryBootCore();
        private void OnEnable() => TryBootCore();
        private void Start() => SetupCoreOrDestroyIt();

        private void TryBootCore()
        {
            bool hasAlreadyBooted = s_appCoreBooted.Contains(gameObject);
            if (
                !Application.isPlaying
                || !gameObject.activeInHierarchy
                || hasAlreadyBooted
            )
            {
                LogInfo("Skip Boot"
                    + " isNotPlaying".If(!Application.isPlaying)
                    + " isNotActiveInHierarchy".If(!gameObject.activeInHierarchy)
                    + " hasAlreadyBooted".If(hasAlreadyBooted));
                return;
            }
            LogInfo("Entered TryBootCore");
            s_appCoreBooted.Add(gameObject);
            if (s_instance == null)
            {
                s_instance = this;
            }

            SetupSceneCallbackObjects();
            // SetupCoreOrDestroyIt();
        }

        private void SetupSceneCallbackObjects()
        {
            if (_callbacksContainer == null)
            {
                Debug.LogError("[AppCore] _callbacksContainer can't be null");
                return;
            }
            if (_callbacksContainer.gameObject == this.gameObject)
            {
                Debug.LogError("[AppCore] _callbacksContainer can't be in the same GameObject as AppCore");
                return;
            }

            Scene scene = _callbacksContainer.gameObject.scene;
            LogInfo($"SceneSetup {Scenex.DescribeScene(scene)}");

            TryCallOnSceneLoadStart(scene);

            AppCallbacksAfter after = _callbacksContainer.GetComponentInChildren<AppCallbacksAfter>();
            if (after == null)
            {
                Debug.LogError("[AppCore] AppCallbacksAfter not found in _callbacksContainer");
                return;
            }
            AppCallbacksBefore before = _callbacksContainer.GetComponentInChildren<AppCallbacksBefore>();
            if (before == null)
            {
                Debug.LogError("[AppCore] AppCallbacksBefore not found in _callbacksContainer");
                return;
            }

            after.AfterDisable += () =>
            {
                LogInfo("AfterDisable");
                TryCallOnSceneUnloadStart(scene);
            };
            after.AfterQuit += () =>
            {
                LogInfo("AfterQuit");
                TryCallQuittingStart();
            };
            after.AfterDestroy += () =>
            {
                LogInfo("AfterDestroy");

                // When entering play mode SceneManager.sceneUnloaded is called,
                // so we need to bind it right after destroy, so it doesn't get
                // called at the beginning of the first scene load
                SceneManager.sceneUnloaded -= SceneUnloadedCallback;
                SceneManager.sceneUnloaded += SceneUnloadedCallback;

                // When app is quitting SceneManager.sceneUnloaded is not called,
                // so we need to call it manually
                if (IsAppQuitting)
                {
                    TryCallOnSceneUnloadEnd(scene, after.gameObject);
                }
            };
            before.BeforeStart += () =>
            {
                LogInfo("BeforeStart");
                TryCallOnSceneLoadEnd(scene);
            };
            before.BeforeDisable += () =>
            {
                LogInfo("BeforeDisable");
                TryCallOnSceneUnloadStart(scene);
            };
            before.BeforeQuit += () =>
            {
                LogInfo("BeforeQuit");
                TryCallQuittingStart();
            };
        }

        private void SetupCoreOrDestroyIt()
        {
            _callbacksContainer.SetParent(null);
            _callbacksContainer.SetAsFirstSibling();

            if (s_instance != this)
            {
                Destroy(gameObject);
                return;
            }


            transform.SetParent(null);
            transform.SetAsFirstSibling();
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            s_appCoreBooted.Remove(gameObject);
        }

        private static void ApplicationQuittingCallback()
        {
            LogInfo("Application.quitting");
            TryCallQuittingStart();
        }

        private static void SceneLoadedCallback(Scene scene, LoadSceneMode mode)
        {
            LogInfo($"SceneManager.sceneLoaded {Scenex.DescribeScene(scene)} {mode}");
            // TryCallOnSceneLoadEnd(scene);
        }

        private static void SceneUnloadedCallback(Scene scene)
        {
            LogInfo($"SceneManager.sceneUnloaded {Scenex.DescribeScene(scene)}");
            TryCallOnSceneUnloadEnd(scene);
        }

        private static void ActiveSceneChangedCallback(Scene current, Scene nextScene)
        {
            LogInfo($"SceneManager.activeSceneChanged from {Scenex.DescribeScene(current)} to {Scenex.DescribeScene(nextScene)}");
        }

        private static void TryCallQuittingStart()
        {
            if (IsAppQuitting) return;
            IsAppQuitting = true;

            LogInfo("Event:OnQuittingStart");
            OnAppQuittingStart.Invoke();
            TryCallOnSceneUnloadStart();
        }

        private static void TryCallOnSceneLoadStart(Scene? sceneOverride = null)
        {
            Scene scene = sceneOverride ?? SceneManager.GetActiveScene();
            Assertx.IsNotNull(scene);
            if (s_loadingScenes.Contains(scene)) return;
            s_loadingScenes.Add(scene);
            LogInfo($"[AppCore:Scene] Loading {Scenex.DescribeScene(scene)}");

            s_activeScenes.Add(scene);
            LogInfo($"[AppCore:Scene] Activated {Scenex.DescribeScene(scene)}");


            if (!IsScenesBatchLoading)
            {
                IsScenesBatchLoading = true;

                if (!IsAppBooted)
                {
                    StaticRuntimeCallbacks.BootAwake();

                    LogInfo($"Event:{nameof(OnAppBootingStart)}");
                    OnAppBootingStart.Invoke();
                }

                LogInfo($"Event:{nameof(OnScenesBatchLoadStart)}");
                OnScenesBatchLoadStart.Invoke();
            }

            LogInfo($"Event:{nameof(OnSceneLoadStart)} {Scenex.DescribeScene(scene)}");
            OnSceneLoadStart.Invoke(scene);
        }

        private static void TryCallOnSceneLoadEnd(Scene? sceneOverride = null)
        {
            Scene scene = sceneOverride ?? SceneManager.GetActiveScene();
            Assertx.IsNotNull(scene);
            if (!s_loadingScenes.Contains(scene)) return;

            LogInfo($"Event:{nameof(OnSceneLoadEnd)} {Scenex.DescribeScene(scene)}");
            OnSceneLoadEnd.Invoke(scene);
            s_loadingScenes.Remove(scene);
            s_scenesBatchLoaded.Add(scene);

            if (IsScenesBatchLoading && s_loadingScenes.Count == 0)
            {
                IsScenesBatchLoading = false;
                LogInfo($"Event:{nameof(OnScenesBatchLoadEnd)}");
                OnScenesBatchLoadEnd.Invoke(s_scenesBatchLoaded);
                s_scenesBatchLoaded.Clear();

                if (!IsAppBooted)
                {
                    LogInfo($"Event:{nameof(OnAppBootingEnd)}");
                    OnAppBootingEnd.Invoke();

                    IsAppBooted = true;
                }
            }
        }

        private static void TryCallOnSceneUnloadStart(Scene? sceneOverride = null)
        {
            Scene scene = sceneOverride ?? SceneManager.GetActiveScene();
            Assertx.IsNotNull(scene);
            if (s_unloadingScenes.Contains(scene)) return;
            s_unloadingScenes.Add(scene);

            if (!IsScenesBatchUnloading)
            {
                IsScenesBatchUnloading = true;
                LogInfo($"Event:{nameof(OnScenesBatchUnloadStart)}");
                OnScenesBatchUnloadStart.Invoke();
            }

            LogInfo($"Event:{nameof(OnSceneUnloadStart)} {Scenex.DescribeScene(scene)}");
            OnSceneUnloadStart.Invoke(scene);
        }

        private static void TryCallOnSceneUnloadEnd(Scene? sceneOverride = null, GameObject objBeingDestroyed = null)
        {
            Scene scene = sceneOverride ?? SceneManager.GetActiveScene();
            Assertx.IsNotNull(scene);

            if (!s_unloadingScenes.Contains(scene)) return;
            s_unloadingScenes.Remove(scene);
            s_scenesBatchUnloaded.Add(scene);

            LogInfo($"Event:{nameof(OnSceneUnloadEnd)} {Scenex.DescribeScene(scene)}");
            OnSceneUnloadEnd.Invoke(scene);

            s_activeScenes.Remove(scene);
            LogInfo($"[AppCore:Scene] Deactivated {Scenex.DescribeScene(scene)}");

            if (s_activeScenes.Count == 0)
            {
                if (IsScenesBatchUnloading)
                {
                    IsScenesBatchUnloading = false;
                    LogInfo($"Event:{nameof(OnScenesBatchUnloadEnd)}");
                    OnScenesBatchUnloadEnd.Invoke(s_scenesBatchUnloaded);
                    s_scenesBatchUnloaded.Clear();
                }

                if (IsAppQuitting)
                {
                    LogInfo("Event:OnQuittingEnd");
                    OnAppQuittingEnd.Invoke();

                    ExecuteObjectsCleanup(objBeingDestroyed);
                    s_rawIsRuntime = false;
                }
            }
        }

        private static void ExecuteObjectsCleanup(GameObject objBeingDestroyed)
        {
            try
            {
                UnsafeExecuteObjectsCleanup(objBeingDestroyed);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AppCore:Cleanup] Exception during cleanup: {ex}");
            }
        }

        private static void UnsafeExecuteObjectsCleanup(GameObject objBeingDestroyed)
        {
            GameObject[] allObjects = FindObjectsByType<GameObject>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            int countNulls = 0;
            int countFails = 0;
            int countSuccesses = 0;
            int countSkips = 0;
            List<string> destroyedObjectDescriptions = new();
            List<string> skippedObjectDescriptions = new();
            foreach (GameObject obj in allObjects)
            {
                if (obj == null)
                {
                    countNulls++;
                    continue;
                }
                string objDescription = $"'{obj.name}' {Scenex.DescribeScene(obj.scene)}";
                bool isObjBeingDestroyed = objBeingDestroyed != null && obj == objBeingDestroyed;
                bool isInactive = !obj.activeInHierarchy;
                if (isObjBeingDestroyed || isInactive)
                {
                    objDescription += " IsInactive".If(!obj.activeInHierarchy);
                    objDescription += " IsCurrentDestroyed".If(isObjBeingDestroyed);
                    skippedObjectDescriptions.Add(objDescription);
                    countSkips++;
                    continue;
                }
                destroyedObjectDescriptions.Add(objDescription);
                try
                {
                    DestroyImmediate(obj);
                    countSuccesses++;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[AppCore:Cleanup] Object Cleanup FAILED: {objDescription}: {ex}");
                    countFails++;
                }
            }

            Debug.Log($"[AppCore:Cleanup] All Objects Cleanup ENDED: Detected {allObjects.Length} objects."
                + $"\n{countSuccesses} objects destroyed successfully."
                + $"\n{countFails} objects failed to be destroyed."
                + $"\n{countSkips} objects skipped."
                + $"\n{countNulls} null references found."
            );
            Debug.Log($"[AppCore:Cleanup] Skipped Objects ({countSkips}):\n- {string.Join("\n- ", skippedObjectDescriptions)}");
            Debug.Log($"[AppCore:Cleanup] Destroyed Objects ({countSuccesses}):\n- {string.Join("\n- ", destroyedObjectDescriptions)}");
        }

        [Button]
        private void Setup()
        {
            AppCallbacksBefore before = GetComponentInChildren<AppCallbacksBefore>();
            AppCallbacksAfter after = GetComponentInChildren<AppCallbacksAfter>();
            Transform container = _callbacksContainer;
            if (container == null && before != null) container = before.transform;
            if (container == null && after != null) container = after.transform;
            if (container == null) container = new GameObject("AppCore:Callbacks").transform;
            container.SetParent(this.transform);

            if (before == null || before.transform != container)
            {
                before = container.gameObject.AddComponent<AppCallbacksBefore>();
            }

            if (after == null || after.transform != container)
            {
                after = container.gameObject.AddComponent<AppCallbacksAfter>();
            }
            _callbacksContainer = container;

            foreach (AppCallbacksAfter otherAfter in GetComponentsInChildren<AppCallbacksAfter>())
            {
                if (otherAfter != after)
                {
                    DestroyImmediate(otherAfter);
                }
            }

            foreach (AppCallbacksBefore otherBefore in GetComponentsInChildren<AppCallbacksBefore>())
            {
                if (otherBefore != before)
                {
                    DestroyImmediate(otherBefore);
                }
            }
        }

        private static void LogInfo(string message)
        {
            Logger.Info(message);
        }
    }
}
