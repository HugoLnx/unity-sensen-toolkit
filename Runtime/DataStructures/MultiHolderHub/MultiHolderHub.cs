using System;
using System.Collections.Generic;

namespace SensenToolkit
{
    public class MultiHolderHub
    {
        private HashSet<object> _holders = new();
        public bool IsHolding => _holders.Count > 0;
        public event Action<bool> OnChanged = delegate { };

        public MultiHolderHub(Action<bool> onChanged = null)
        {
            if (onChanged != null)
            {
                OnChanged += onChanged;
            }
        }

        public void Hold(object holder)
        {
            if (holder == null) return;
            bool wasHolding = IsHolding;
            _holders.Add(holder);
            if (!wasHolding && IsHolding) OnChanged.Invoke(true);
        }

        public void Release(object holder)
        {
            if (holder == null) return;
            bool wasHolding = IsHolding;
            _holders.Remove(holder);
            if (wasHolding && !IsHolding) OnChanged.Invoke(false);
        }

        public void ReleaseAllHolders()
        {
            bool wasHolding = IsHolding;
            _holders.Clear();
            if (wasHolding && !IsHolding) OnChanged.Invoke(false);
        }
    }
}
