using System;

namespace SensenToolkit
{
    public abstract class AMultiHolderHubService<TService, TKey> : ATransientSingleton<TService>
    where TService : AMultiHolderHubService<TService, TKey>
    {
        protected bool IsHeld => Hub.IsHolding;
        public event Action<bool> OnChanged = delegate { };
        private MultiHolderHub Hub => _hub ??= CreateHub();
        private MultiHolderHub _hub;

        public void Hold(TKey holder) => Hub.Hold(holder);
        public void Release(TKey holder) => Hub.Release(holder);
        public void ReleaseAllHolders() => Hub.ReleaseAllHolders();

        private MultiHolderHub CreateHub()
        {
            MultiHolderHub hub = new();
            hub.OnChanged += (isHeld) => OnChanged.Invoke(isHeld);
            return hub;
        }
    }
}
