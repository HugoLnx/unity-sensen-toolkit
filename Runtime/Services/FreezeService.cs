using System;
using System.Collections.Generic;
using MyBox;
using SensenToolkit;
using UnityEngine;

namespace Bumashuta
{
    public class FreezeService : ATransientSingleton<FreezeService>
    {
        [SerializeField] private bool _scaleTime = true;
        [SerializeField, ReadOnly] private bool _isFrozen;
        private HashSet<Component> _locks = new();

        public bool IsFrozen => _isFrozen;

        public event Action<bool> OnChanged;

        protected override void AwakeAny()
        {
            base.AwakeAny();
            SetIsFrozen(false, force: true);
        }

        public void Freeze(Component c)
        {
            if (c == null) return;
            _locks.Add(c);
            RefreshIsFrozen();
        }

        public void Unfreeze(Component c)
        {
            if (c == null) return;
            _locks.Remove(c);
            RefreshIsFrozen();
        }

        private void RefreshIsFrozen()
        {
            SetIsFrozen(_locks.Count > 0);
        }

        private void SetIsFrozen(bool turnOn, bool force = false)
        {
            if (!force && _isFrozen == turnOn) return;
            _isFrozen = turnOn;
            if (_scaleTime)
            {
                Time.timeScale = _isFrozen ? 0f : 1f;
            }
            _locks.Clear();
            OnChanged?.Invoke(_isFrozen);
        }
    }
}
