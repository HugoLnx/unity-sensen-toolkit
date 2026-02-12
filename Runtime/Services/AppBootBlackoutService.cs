using System;
using System.Collections;
using MyBox;
using UnityEngine;

namespace SensenToolkit
{
    public class AppBootBlackoutService : APermanentSingleton<AppBootBlackoutService>
    {
        [SerializeField, AutoProperty(AutoPropertyMode.Parent)]
        private PanelFadable _panel;

        private const bool ACTIVATE_LOG = false;
        private Logx _logger;
        private new Logx Logger => _logger ??= Logx.GetLogger(nameof(AppBootBlackoutService), ACTIVATE_LOG);

        private MultiHolderHub _holdersHub;
        private bool _isBlackoutOver;
        private bool _wasStarted;

        private MultiHolderHub HoldersHub => _holdersHub ??= new(onChanged: OnHoldersChanged);
        public bool IsBlackoutActive => HoldersHub.IsHolding;

        public event Action OnBlackoutIsOver = delegate { };

        protected override void AwakeSingleton()
        {
            base.AwakeSingleton();
        }

        private IEnumerator Start()
        {
            Logger.Info("Show Blackout");
            _panel.IntantShow();
            FreezeService.Instance.HoldFreeze(this);
            _wasStarted = true;
            yield return null;
            yield return null;
            yield return null;
            TryEndBlackout();
        }

        public void HoldBlackout(object holder)
        {
            Logger.Info($"{holder} - Hold");
            HoldersHub.Hold(holder);
        }

        public void ReleaseBlackout(object holder)
        {
            Logger.Info($"{holder} - Release");
            HoldersHub.Release(holder);
        }

        private void OnHoldersChanged(bool _) => TryEndBlackout();

        private void TryEndBlackout()
        {
            if (!_wasStarted || _isBlackoutOver || IsBlackoutActive) return;
            Logger.Info("Hide Blackout");
            _isBlackoutOver = true;
            _panel.Hide();
            FreezeService.Instance.ReleaseFreeze(this);
            OnBlackoutIsOver.Invoke();
            OnBlackoutIsOver = delegate { };
        }
    }
}
