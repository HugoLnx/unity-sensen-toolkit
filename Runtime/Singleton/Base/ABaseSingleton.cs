using System;
using UnityEngine;

namespace SensenToolkit.Internal
{
    public abstract class ABaseSingleton<T> : ASuperBaseSingleton<T>, IAppCore_RuntimeInit_SubsystemRegistration_Internal
    where T : ABaseSingleton<T>
    {
        private static string s_singletonTypeName;
        private static string SingletonTypeName => s_singletonTypeName ??= IsPermanent ? "Permanent" : "Transient";
        protected sealed override string DescriptiveKey => $"{typeof(T).Name}|{(InstanceValidityDescription(this as T))}|{SingletonTypeName}|{name}";

        private static T s_instance;
        public static bool HasInstance => IsValidInstance(s_instance);
        protected sealed override bool IsAlreadyInstanced => HasInstance;
        public override bool IsActualSingleton => s_instance == this;
        public override bool IsExcessSingleton => s_instance != null && s_instance != this;
        public static T Instance => GetOrSetInstance();

        public static SingletonAssignCallbacks<T> OnSetSingletonCallbacks = new();

        public static T GetInstanceIfExists()
        {
            if (HasInstance) return s_instance;

            if (AppCore.IsAppBooted && !AppCore.IsAnySceneLoading) return null;

            if (IsTransient && AppCore.IsActiveSceneUnloading)
            {
                // Scene is unloading, should not create new instance
                LogInfo($"[{typeof(T).Name}] Scene is unloading, should not create new instance");
                return null;
            }

            if (AppCore.IsAppQuitting)
            {
                // Application is quitting, should not create new instance
                LogInfo($"[{typeof(T).Name}] Application is quitting, should not create new instance");
                return null;
            }

            T foundInstance = FindFirstObjectByType<T>();
            if (TrySetAndInitializeInstance(foundInstance))
            {

                Assertx.IsNotNull(s_instance);
                LogInfo($"[{s_instance.DescriptiveKey}] Binded found instance");
                return s_instance;
            }

            return null;
        }

        private static T GetOrSetInstance()
        {
            T availableInstance = GetInstanceIfExists();
            if (availableInstance != null) return availableInstance;

            var obj = new GameObject();
            obj.SetActive(false);
            if (TrySetAndInitializeInstance(obj.AddComponent<T>()))
            {
                obj.name = $"{typeof(T).Name}:AutoCreated:{SingletonTypeName}";
                obj.SetActive(true);

                Assertx.IsNotNull(s_instance);
                LogInfo($"[{obj.name}] Binded created instance");
                return s_instance;
            }

            Destroy(obj);
            throw new Exception($"[{typeof(T).Name}] Failed to setup instance");
        }

        protected sealed override bool ToSingletonInstance() => TrySetAndInitializeInstance(this as T);
        private static bool TrySetAndInitializeInstance(T instance)
        {
            if (instance == null) return false;
            if (HasInstance) throw new Exception($"[{instance.DescriptiveKey}] Instance already set");

            if (!IsValidInstance(instance)) throw new Exception($"[{instance.DescriptiveKey}] Cant initialize an invalid instance. ({InstanceValidityDescription(instance)})");
            s_instance = instance;
            instance.InitializeAsSingleton();

            OnSetSingletonCallbacks.InvokeCallbacksIfChanged(instance);
            return true;
        }

        public static void AddAssignListener(
            object key,
            Action<T> assign = null,
            Action<T> unassign = null,
            bool forceInstance = false)
        {
            OnSetSingletonCallbacks.AddAssignListeners(key, assign, unassign);
            if (HasInstance)
            {
                assign?.Invoke(s_instance);
                return;
            }


            if (forceInstance)
            {
                T _ = Instance;
            }
        }

        public static void UnassignAndRemoveListener(object key)
        {
            OnSetSingletonCallbacks.UnassignAndRemoveAssignListener(key);
        }

        protected sealed override void OnDestroyCleanup()
        {
            base.OnDestroyCleanup();
            s_instance = null;
        }

        public static void AppCore_RuntimeInit_SubsystemRegistration_Internal()
        {
            StaticBaseResetStatics();
            StaticSuperBaseResetStatics();
        }

        protected static void StaticBaseResetStatics()
        {
            s_instance = null;
            OnSetSingletonCallbacks.Reset();
        }
    }
}
