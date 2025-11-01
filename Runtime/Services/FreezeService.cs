using System;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace SensenToolkit
{
    public class FreezeService : ATransientSingleton<FreezeService>
    {
        [SerializeField] private bool _scaleTime = true;
        [SerializeField] private bool _autoFreezeOnAppFocusLost = true;
        [SerializeField] private bool _autobindKeyShortcutsOnEditor = true;
        [SerializeField, ReadOnly] private bool _isFrozen;
        private MultiHolderHub _freezeHoldersHub;
        private MultiHolderHub FreezeHoldersHub => _freezeHoldersHub ??= CreateFreezeHoldersHub();

        public bool IsFrozen => _isFrozen;

        public event Action<bool> OnChanged;

        protected override void AwakeAny()
        {
            base.AwakeAny();
            SetIsFrozen(false, force: true);
        }

        public void HoldFreeze(object holder)
        {
            FreezeHoldersHub.Hold(holder);
        }

        public void ReleaseFreeze(object holder)
        {
            FreezeHoldersHub.Release(holder);
        }

        private void RefreshIsFrozen()
        {
            SetIsFrozen(FreezeHoldersHub.IsHolding);
        }

        private void SetIsFrozen(bool isFrozen, bool force = false)
        {
            bool previousIsFrozen = IsFrozen;
            if (!force && _isFrozen == isFrozen) return;
            _isFrozen = isFrozen;
            if (_scaleTime)
            {
                Time.timeScale = IsFrozen ? 0f : 1f;
            }
            if (!_isFrozen) FreezeHoldersHub.ReleaseAllHolders();
            if (previousIsFrozen != IsFrozen)
            {
                OnChanged?.Invoke(IsFrozen);
            }
        }

        private const string APPLICATION_FOCUS_FREEZE_LOCK = "APPLICATION_FOCUS_FREEZE_LOCK";
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!_autoFreezeOnAppFocusLost) return;
            if (hasFocus)
            {
                ReleaseFreeze(APPLICATION_FOCUS_FREEZE_LOCK);
            }
            else
            {
                HoldFreeze(APPLICATION_FOCUS_FREEZE_LOCK);
            }
        }

        private MultiHolderHub CreateFreezeHoldersHub()
            => new(onChanged: OnHoldersChanged);

        private void OnHoldersChanged(bool _)
        {
            RefreshIsFrozen();
        }

    }
}
