using System.Collections;
using MyBox;
using Steamworks;
using UnityEngine;
using System;
using System.Collections.Generic;
using EasyButtons;

namespace SensenToolkit
{
    // Read more on: https://partner.steamgames.com/doc/api/ISteamUserStats
    public class SteamStatsAndAchievements : APermanentSingleton<SteamStatsAndAchievements>
    {
        [SerializeField] private AchievementSO[] _prodAchievements;
        [SerializeField] private StatSO[] _prodStats;
        [SerializeField] private AchievementSO[] _demoAchievements;
        [SerializeField] private StatSO[] _demoStats;

        private const bool ACTIVATE_LOGS = true;
        private Logx _logger;
        private new Logx Logger => _logger ??= Logx.GetLogger(nameof(SteamStatsAndAchievements), ACTIVATE_LOGS);

        private Callback<UserStatsReceived_t> _statsReceivedCallback;
        private int? _lastStatsStoreTime;

        private HashSet<AchievementSO> _achievementsSet;
        private HashSet<StatSO> _statsSet;
        public AchievementSO[] Achievements => Env.IsDemoBuild ? _demoAchievements : _prodAchievements;
        public HashSet<AchievementSO> AchievementsSet => _achievementsSet ??= new(Achievements);

        public StatSO[] Stats => Env.IsDemoBuild ? _demoStats : _prodStats;
        public HashSet<StatSO> StatsSet => _statsSet ??= new(Stats);

        public void Start()
        {
            if (Application.isEditor)
            {
                StartCoroutine(CheckStatsAndAchievements());
            }
        }

        private IEnumerator CheckStatsAndAchievements()
        {
            yield return SteamManager.WaitBooted();
            var steam = SteamManager.GetInstanceIfExists();
            if (steam == null || steam.IsDisabled) yield break;

            foreach (AchievementSO achievement in Achievements)
            {
                string achievementName = achievement.SteamName;
                if (!SteamUserStats.GetAchievement(achievementName, out bool isAchieved))
                {
                    Debug.LogWarning($"[STEAMWORKS] Achievement does NOT exist! {achievementName}");
                    continue;
                }
            }

            foreach (StatSO stat in Stats)
            {
                string statName = stat.SteamName;
                if (!SteamUserStats.GetStat(statName, out int statInt))
                {
                    if (!SteamUserStats.GetStat(statName, out float statFloat))
                    {
                        Debug.LogWarning($"[STEAMWORKS] Stat does NOT exist! {statName}");
                    }
                }
            }
        }

        // Must be called to submit stats to steam. Usually when the level is finished
        // or another significant event occurs.
        // throttle: If true, that call will be called only if the last one was more than 30 seconds ago
        // PS: Steam docs say that call should be done in the order of minutes rather than seconds
        private const int STORE_THROTTLE_TIME_SECONDS = 30;
        public bool StoreStats(bool throttle = false)
        {
            if (!CheckIsInitialized(nameof(StoreStats))) return false;
            if (throttle
                && _lastStatsStoreTime.HasValue
                && Time.time - _lastStatsStoreTime.Value < STORE_THROTTLE_TIME_SECONDS)
            {
                StartCoroutine(nameof(StoreStatsDelayed));
                return false;
            }
            bool wasSuccessful = SteamUserStats.StoreStats();
            if (wasSuccessful)
            {
                StopCoroutine(nameof(StoreStatsDelayed));
                _lastStatsStoreTime = Mathf.RoundToInt(Time.time);
            }

            return wasSuccessful;
        }

        private IEnumerator StoreStatsDelayed()
        {
            int timePast = _lastStatsStoreTime.HasValue
                ? Mathf.RoundToInt(Time.time) - _lastStatsStoreTime.Value
                : 0;
            yield return new WaitForSecondsRealtime(STORE_THROTTLE_TIME_SECONDS - timePast);
            StoreStats();
        }

        // Store stats locally, need to call afterwards StoreStats to submit to Steam
        public bool SetStatFloat(ISteamStat stat, float val)
        {
            if (!CheckValidStat(stat, nameof(SetStatFloat))) return false;
            string name = stat.SteamName;
            if (!CheckIsInitialized(nameof(SetStatFloat), name)) return false;
            if (!SteamUserStats.SetStat(name, val))
            {
                Debug.LogWarning($"[STEAMWORKS] SetStatFloat failed: {name} {val}");
                return false;
            }
            return true;
        }

        // Store stats locally, need to call afterwards StoreStats to submit to Steam
        public bool SetStatInt(ISteamStat stat, int val, bool decrementOnly = false)
        {
            if (!CheckValidStat(stat, nameof(SetStatInt))) return false;
            string name = stat.SteamName;
            if (!CheckIsInitialized(nameof(SetStatInt), name)) return false;
            if (decrementOnly)
            {
                if (!SteamUserStats.GetStat(name, out int currentVal))
                {
                    Debug.LogWarning($"[STEAMWORKS] SetStatInt decrement failed: {name} {val}");
                    return false;
                }
                if (currentVal <= val) return true;
            }
            if (!SteamUserStats.SetStat(name, val))
            {
                Debug.LogWarning($"[STEAMWORKS] SetStatInt failed: {name} {val}");
                return false;
            }
            return true;
        }

        public bool AddStatInt(ISteamStat stat, int val)
        {
            if (!CheckValidStat(stat, nameof(AddStatInt))) return false;
            string name = stat.SteamName;
            if (!CheckIsInitialized(nameof(AddStatInt), name)) return false;
            if (!SteamUserStats.GetStat(name, out int currentVal))
            {
                Debug.LogWarning($"[STEAMWORKS] AddStatInt failed: {name} {val}");
                return false;
            }

            Debug.Log($"[STEAMWORKS] AddStatInt {name} {currentVal + val}");

            return SteamUserStats.SetStat(name, currentVal + val);
        }

        public bool AddStatFloat(ISteamStat stat, float val)
        {
            if (!CheckValidStat(stat, nameof(AddStatFloat))) return false;
            string name = stat.SteamName;
            if (!CheckIsInitialized(nameof(AddStatFloat), name)) return false;
            if (!SteamUserStats.GetStat(name, out float currentVal))
            {
                Debug.LogWarning($"[STEAMWORKS] AddStatFloat failed: {name} {val}");
                return false;
            }

            return SteamUserStats.SetStat(name, currentVal + val);
        }

        public bool TryGetStatInt(ISteamStat stat, out int val)
        {
            val = default;
            if (!CheckValidStat(stat, nameof(TryGetStatInt))) return false;
            string name = stat.SteamName;
            if (!CheckIsInitialized(nameof(TryGetStatInt), name)) return false;

            if (SteamUserStats.GetStat(name, out int intVal))
            {
                val = intVal;
                return true;
            }

            return false;
        }

        public bool TryGetStatFloat(ISteamStat stat, out float val)
        {
            val = default;
            if (!CheckValidStat(stat, nameof(TryGetStatFloat))) return false;
            string name = stat.SteamName;
            if (!CheckIsInitialized(nameof(TryGetStatFloat), name)) return false;

            if (SteamUserStats.GetStat(name, out float floatVal))
            {
                val = floatVal;
                return true;
            }

            return false;
        }

        // Set the achievement locally, need to call afterwards StoreStats to submit to Steam
        public void UnlockAchievement(ISteamAchievement achievement, bool storeStats = true)
        {
            if (!CheckValidAchievement(achievement, nameof(UnlockAchievement))) return;
            string name = achievement.SteamName;
            if (!CheckIsInitialized(nameof(UnlockAchievement), name)) return;
            if (!SteamUserStats.SetAchievement(name))
            {
                Debug.LogWarning($"[STEAMWORKS] SetAchievement failed: {name}");
                return;
            }

            if (storeStats)
            {
                StoreStats(throttle: true);
            }
        }

        // Used for debugging only, resets all stats and achievements of the current account
        [Button]
        public void ResetStatsAndAchievements()
        {
            var steam = SteamManager.GetInstanceIfExists();
            if (steam.IsInitialized)
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

        private bool CheckIsInitialized(string operationName, string itemName = "")
        {
            var steam = SteamManager.GetInstanceIfExists();
            if (steam != null && steam.IsInitialized) return true;
            Logger.Info($"[Steam:{operationName}]{itemName} Canceled ({"!SteamBooted ".If(!steam.IsBooted)} {"SteamDisabled".If(steam.IsDisabled)})");
            return false;
        }

        private bool CheckValidStat(ISteamStat stat, string operation)
        {
            bool isRegistered = stat != null && StatsSet.Contains(stat as StatSO);
            if (isRegistered) return true;
            string statName = (stat != null ? stat.SteamName : null) ?? "null";
            Logger.Info($"[Steam:{operation}] Invalid Stat {statName} {" isNull".If(stat == null)} {" isNotRegistered".If(!isRegistered)}");
            return false;
        }

        private bool CheckValidAchievement(ISteamAchievement achievement, string operation)
        {
            bool isRegistered = achievement != null && AchievementsSet.Contains(achievement as AchievementSO);
            if (isRegistered) return true;
            string achievementName = (achievement != null ? achievement.SteamName : null) ?? "null";
            Logger.Info($"[Steam:{operation}] Invalid Achievement {achievementName} {" isNull".If(achievement == null)} {" isNotRegistered".If(!isRegistered)}");
            return false;
        }
    }
}
