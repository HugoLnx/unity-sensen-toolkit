#if SENSEN_BAYAT
using System;
using Bayat.SaveSystem;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SensenToolkit
{
    public static class SaveToolkit
    {
        public static async UniTask SaveAsync<TData>(SaveRepository<TData> repository, TData data)
        where TData : ISaveRootData
        {
            data.Timestamp = DateTime.UtcNow;
            data.UserId = SteamManager.ResolvedUserId;
            data.EnvId = Env.GetEnvId();
            string msg = $"Saving data to key '{repository.Key}' with timestamp:'{data.Timestamp}' envId:'{data.EnvId}'";
            if (Env.IsDebugBuild) msg += $" userId:'{data.UserId}' {JsonUtility.ToJson(data, prettyPrint: true)}";
            UnityEngine.Debug.Log(msg);
            await SaveSystemAPI.SaveAsync(repository.Key, data).AsUniTask();
        }

        public static async UniTask<TData> LoadAsync<TData>(SaveRepository<TData> repository, System.Action<TData> onDataLoaded = null)
        where TData : ISaveRootData
        {
            if (!await SaveSystemAPI.ExistsAsync(repository.Key))
            {
                onDataLoaded?.Invoke(default);
                return default;
            }
            try
            {
                TData data = await SaveSystemAPI.LoadAsync<TData>(repository.Key).AsUniTask();
                if (Env.IsDebugBuild)
                {
                    string json = JsonUtility.ToJson(data, prettyPrint: true);
                    Debug.Log($"Loaded data for key '{repository.Key}':\n{json}");
                }
                if (!IsValidData(data, out string denyReason))
                {
                    string message = $"It's invalid because '{denyReason}'.";
                    throw new InvalidOperationException(message);
                }
                onDataLoaded?.Invoke(data);
                return data;
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogWarning($"Failed to load save data for key '{repository.Key}'. Exception: {ex}");
                await SaveSystemAPI.CreateBackupAsync(repository.Key).AsUniTask();
                await SaveSystemAPI.DeleteAsync(repository.Key).AsUniTask();
                onDataLoaded?.Invoke(default);
                return default;
            }
        }

        public static async UniTask DeleteAsync<TData>(SaveRepository<TData> repository)
        where TData : ISaveRootData
        {
            await SaveSystemAPI.DeleteAsync(repository.Key).AsUniTask();
        }

        public static async UniTask BackupAsync<TData>(SaveRepository<TData> repository)
        where TData : ISaveRootData
        {
            await SaveSystemAPI.CreateBackupAsync(repository.Key).AsUniTask();
        }

        private static bool IsValidData<TData>(TData data, out string denyReason) where TData : ISaveRootData
        {
            denyReason = null;
            if (data == null)
            {
                denyReason = "Data is null.";
                return false;
            }
            if (data.Timestamp == default || data.Timestamp == DateTime.MinValue)
            {
                denyReason = "Timestamp is empty.";
                return false;
            }
            if (string.IsNullOrEmpty(data.UserId))
            {
                denyReason = "UserId is empty.";
                return false;
            }
            if (string.IsNullOrEmpty(data.EnvId))
            {
                denyReason = "EnvId is empty.";
                return false;
            }

            string expectedUserId = SteamManager.ResolvedUserId;
            if (data.UserId != expectedUserId)
            {
                denyReason = "UserId does not match the current user.";
                if (Env.IsDebugBuild)
                {
                    denyReason += $" (expected: '{expectedUserId}', actual: '{data.UserId}')";
                }
                return false;
            }
            string expectedEnvId = Env.GetEnvId();
            if (data.EnvId != expectedEnvId)
            {
                denyReason = "EnvId does not match the current environment.";
                if (Env.IsDebugBuild)
                {
                    denyReason += $" (expected: '{expectedEnvId}', actual: '{data.EnvId}')";
                }
                return false;
            }
            return true;
        }
    }
}
#endif
