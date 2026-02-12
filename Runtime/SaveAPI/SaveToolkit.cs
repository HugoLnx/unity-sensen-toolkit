#if SENSEN_BAYAT
using Bayat.SaveSystem;
using Cysharp.Threading.Tasks;

namespace SensenToolkit
{
    public static class SaveToolkit
    {
        public static async UniTaskVoid SaveAsync<TData>(SaveRepository<TData> repository, TData data)
        where TData : ISaveRootData
        {
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

        public static async UniTaskVoid DeleteAsync<TData>(SaveRepository<TData> repository)
        where TData : ISaveRootData
        {
            await SaveSystemAPI.DeleteAsync(repository.Key).AsUniTask();
        }
    }
}
#endif
