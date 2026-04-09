// The SteamManager is designed to work with Steamworks.NET
// This file is released into the public domain.
// Where that dedication is not recognized you are granted a perpetual,
// irrevocable license to copy and modify this file as you see fit.
//
// Version: 1.0.12
#if STEAMWORKS_NET

#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

#if DISABLESTEAMWORKS || SENSEN_BOOTH_BUILD || SENSEN_NOSTEAM
#define STEAM_BLOCKINIT
#endif

using UnityEngine;
#if !DISABLESTEAMWORKS
using System.Collections;
using Steamworks;
using System;
using EasyButtons;
#endif

namespace SensenToolkit
{
    //
    // The SteamManager provides a base implementation of Steamworks.NET on which you can build upon.
    // It handles the basics of starting up and shutting down the SteamAPI for use.
    //
    [DisallowMultipleComponent]
    public class SteamManager : APermanentSingleton<SteamManager>
    {
        [SerializeField] private uint _prodAppId = 0;
        [SerializeField] private uint _demoAppId = 0;

        public bool IsBooted => IsInitialized || IsDisabled;
        public AppId_t AppId => (AppId_t)(Env.IsDemoBuild ? _demoAppId : _prodAppId);

        public static CSteamID UserId => GetIfInitialized(s_userId);
        public static CGameID GameId => GetIfInitialized(s_gameId);
        public static string ResolvedUserName => GetResolved(s_resolvedUserName);
        public static string ResolvedUserId => GetResolved(s_resolvedUserId);
        public static string ResolvedUserIdHash => GetResolved(s_resolvedUserIdHash);
        private static CSteamID? s_userId = null;
        private static CGameID? s_gameId = null;
        private static string s_resolvedUserId = null;
        private static string s_resolvedUserName = null;
        private static string s_resolvedUserIdHash = null;

        public static bool IsFunctional
        {
            get
            {
                SteamManager steam = GetInstanceIfExists();
                return steam != null && steam.IsBooted && steam.IsInitialized;
            }
        }

        public delegate void SteamManagerBooted(bool initialized);
        private event SteamManagerBooted OnBooted = delegate { };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void InitOnPlayMode()
        {
#if !STEAM_BLOCKINIT
            s_everInitialized = false;
#endif
            s_resolvedUserId = null;
            s_resolvedUserName = null;
            s_resolvedUserIdHash = null;
            s_userId = null;
            s_gameId = null;
        }


#if !STEAM_BLOCKINIT
        private static bool s_everInitialized = false;
        private bool _isInitialized = false;
        public bool IsInitialized => _isInitialized;

        private bool _isDisabled = false;
        public bool IsDisabled => _isDisabled;

        protected SteamAPIWarningMessageHook_t SteamAPIWarningMessageHook;

        [AOT.MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
        protected static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText)
        {
            Debug.LogWarning(pchDebugText);
        }

        protected override void AwakeSingleton()
        {
            OnBooted += (initialized) =>
            {
                if (initialized)
                {
                    string msg = $"SteamAPI successfully initialized! appid:'{AppId}'";
                    if (Env.IsDebugBuild) msg += $" username:'{ResolvedUserName}' userid:'{ResolveUserId()}' useridhash:'{ResolveUserIdHash()}'";
                    Debug.Log(msg, this);
                }
                else
                {
                    Debug.LogWarning("SteamAPI failed to initialize, Steamworks features will not be available.", this);
                }
            };

            if (s_everInitialized)
            {
                // This is almost always an error.
                // The most common case where this happens is when SteamManager gets destroyed because of Application.Quit(),
                // and then some Steamworks code in some other OnDestroy gets called afterwards, creating a new SteamManager.
                // You should never call Steamworks functions in OnDestroy, always prefer OnDisable if possible.
                throw new System.Exception("Tried to Initialize the SteamAPI twice in one session!");
            }

            if (!Packsize.Test())
            {
                Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", this);
            }

            if (!DllCheck.Test())
            {
                Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", this);
            }

            var bootBlackout = AppBootBlackoutService.GetInstanceIfExists();
            if (bootBlackout != null)
            {
                bootBlackout.HoldBlackout(this);
                OnBooted += (initialized) => bootBlackout.ReleaseBlackout(this);
            }

            try
            {
                // If Steam is not running or the game wasn't started through Steam, SteamAPI_RestartAppIfNecessary starts the
                // Steam client and also launches this game again if the User owns it. This can act as a rudimentary form of DRM.

                // Once you get a Steam AppID assigned by Valve, you need to replace AppId_t.Invalid with it and
                // remove steam_appid.txt from the game depot. eg: "(AppId_t)480" or "new AppId_t(480)".
                // See the Valve documentation for more information: https://partner.steamgames.com/doc/sdk/api#initialization_and_shutdown
                if (SteamAPI.RestartAppIfNecessary((AppId_t)AppId))
                {
                    Application.Quit();
                    return;
                }
            }
            catch (System.DllNotFoundException e)
            { // We catch this exception here, as it will be the first occurrence of it.
                Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + e, this);

                Application.Quit();
                return;
            }

            // Initializes the Steamworks API.
            // If this returns false then this indicates one of the following conditions:
            // [*] The Steam client isn't running. A running Steam client is required to provide implementations of the various Steamworks interfaces.
            // [*] The Steam client couldn't determine the App ID of game. If you're running your application from the executable or debugger directly then you must have a [code-inline]steam_appid.txt[/code-inline] in your game directory next to the executable, with your app ID in it and nothing else. Steam will look for this file in the current working directory. If you are running your executable from a different directory you may need to relocate the [code-inline]steam_appid.txt[/code-inline] file.
            // [*] Your application is not running under the same OS user context as the Steam client, such as a different user or administration access level.
            // [*] Ensure that you own a license for the App ID on the currently active Steam account. Your game must show up in your Steam library.
            // [*] Your App ID is not completely set up, i.e. in Release State: Unavailable, or it's missing default packages.
            // Valve's documentation for this is located here:
            // https://partner.steamgames.com/doc/sdk/api#initialization_and_shutdown
            _isInitialized = SteamAPI.Init();
            if (_isInitialized)
            {
                s_userId = SteamUser.GetSteamID();
                s_gameId = new CGameID(SteamUtils.GetAppID());
                s_resolvedUserId = s_userId.ToString();
                s_resolvedUserName = SteamFriends.GetPersonaName();
                s_resolvedUserIdHash = SimpleHashing.Instance.SHA1Short(s_resolvedUserId);
            }
            else
            {
                Debug.LogWarning("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.", this);
                if (Env.IsProductionBuild)
                {
                    Application.Quit();
                    return;
                }
                SetupGuestUser();
                _isDisabled = true;
                InvokeBooted();

                return;
            }

            s_everInitialized = true;
            InvokeBooted();
        }

        // This should only ever get called on first load and after an Assembly reload, You should never Disable the Steamworks Manager yourself.
        private void OnEnable()
        {
            if (!_isInitialized) return;

            if (SteamAPIWarningMessageHook == null)
            {
                // Set up our callback to receive warning messages from Steam.
                // You must launch with "-debug_steamapi" in the launch args to receive warnings.
                SteamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamAPIDebugTextHook);
                SteamClient.SetWarningMessageHook(SteamAPIWarningMessageHook);
            }
        }

        // OnApplicationQuit gets called too early to shutdown the SteamAPI.
        // Because the SteamManager should be persistent and never disabled or destroyed we can shutdown the SteamAPI here.
        // Thus it is not recommended to perform any Steamworks work in other OnDestroy functions as the order of execution can not be garenteed upon Shutdown. Prefer OnDisable().
        protected override void OnDestroySingleton()
        {
            if (!_isInitialized) return;

            SteamAPI.Shutdown();
        }

        protected virtual void Update()
        {
            if (!_isInitialized)
            {
                return;
            }

            // Run Steam client callbacks
            SteamAPI.RunCallbacks();
        }

#else
        public bool IsInitialized => false;
        public bool IsDisabled => true;

        protected override void AwakeSingleton()
        {
            base.AwakeSingleton();
            SetupGuestUser();
            InvokeBooted();
        }
#endif // !DISABLESTEAMWORKS
        private static string ResolveUserIdHash()
        {
            var steamManager = SteamManager.GetInstanceIfExists();
            if (steamManager != null && !steamManager.IsBooted)
            {
                throw new System.InvalidOperationException("SteamManager exists but is not booted.");
            }

            if (steamManager == null || steamManager.IsDisabled)
            {
                return "guest";
            }

            SimpleHashing hashing = SimpleHashing.Instance;
            return hashing.SHA1Short(UserId.ToString());
        }

        private static string ResolveUserId()
        {
            var steamManager = SteamManager.GetInstanceIfExists();
            if (steamManager != null && !steamManager.IsBooted)
            {
                throw new System.InvalidOperationException("SteamManager exists but is not booted.");
            }

            if (steamManager == null || steamManager.IsDisabled)
            {
                return "guest";
            }

            return UserId.ToString();
        }

        public void AddBootListener(SteamManagerBooted listener)
        {
            if (IsBooted)
            {
                listener.Invoke(IsInitialized);
                return;
            }

            OnBooted += listener;
        }

        private void InvokeBooted()
        {
            OnBooted.Invoke(IsInitialized);
            OnBooted = delegate { };
        }

        private static string GetResolved(string resolvedValue)
        {
            if (string.IsNullOrEmpty(resolvedValue)
                && SteamManager.GetInstanceIfExists() != null)
            {
                throw new System.InvalidOperationException("Resolved value is not set. You should call it only after SteamManager is booted.");
            }
            return resolvedValue;
        }

        private static void SetupGuestUser()
        {
            s_userId = null;
            s_gameId = null;
            s_resolvedUserId = "guest";
            s_resolvedUserName = "Guest";
            s_resolvedUserIdHash = "guest";
        }

        public static IEnumerator WaitBooted()
        {
            var steamManager = SteamManager.GetInstanceIfExists();
            if (steamManager == null) yield break;

            yield return new WaitUntil(() => steamManager.IsBooted);
        }

        private static T GetIfInitialized<T>(T? val) where T : struct
        {
            if (val == null)
            {
                throw new System.InvalidOperationException("Value is not set. You should get it only after SteamManager is initialized.");
            }
            SteamManager steam = GetInstanceIfExists();
            if (steam == null || !steam.IsInitialized)
            {
                throw new System.InvalidOperationException("SteamManager is not initialized.");
            }
            return val.Value;
        }

        // Used for debugging only, resets all stats and achievements of the current account
        [Button]
        public void ResetStatsAndAchievementsButton() => ResetStatsAndAchievements();

        public static void ResetStatsAndAchievements()
        {
            if (IsFunctional)
            {
                SteamUserStats.ResetAllStats(bAchievementsToo: true);
                SteamUserStats.StoreStats();
                Debug.Log("Steam Stats and Achievements Reseted");
            }
            else
            {
                Debug.LogWarning("Steam Manager isn't initialized");
            }
        }
    }
}

#endif // STEAMWORKS_NET
