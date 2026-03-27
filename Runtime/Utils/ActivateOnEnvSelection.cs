using System;
using System.ComponentModel;
using MyBox;
using UnityEngine;

namespace SensenToolkit
{
    public enum EnvSelection
    {
        IsDemo,
        IsBooth,
        IsEditor,
        IsDebug,
        IsFullGame,
    }

    [RequireComponent(typeof(PanelChildVisibilityEvents))]
    public class ActivateOnEnvSelection : MonoBehaviour
    {
        [SerializeField] private EnvSelection[] _envsWhitelist;
        [SerializeField] private EnvSelection[] _envsBlacklist;
        [SerializeField, AutoProperty] private PanelChildVisibilityEvents _visibilityEvents;

        private void Awake()
        {
            _visibilityEvents.OnShow += RefreshActive;
        }

        private void OnEnable()
        {
            RefreshActive();
        }

        private void RefreshActive()
        {
            gameObject.SetActive(CheckAllowedOnCurrentEnv());
        }

        private bool CheckAllowedOnCurrentEnv()
        {
            return CheckAllowedByWhitelist() && CheckAllowedByBlacklist();
        }

        private bool CheckAllowedByWhitelist()
        {
            if (_envsWhitelist == null || _envsWhitelist.Length == 0) return true;
            foreach (EnvSelection env in _envsWhitelist)
            {
                if (IsEnvSelected(env)) return true;
            }
            return false;
        }

        private bool CheckAllowedByBlacklist()
        {
            if (_envsBlacklist == null || _envsBlacklist.Length == 0) return true;
            foreach (EnvSelection env in _envsBlacklist)
            {
                if (IsEnvSelected(env)) return false;
            }
            return true;
        }

        private bool IsEnvSelected(EnvSelection env)
        {
            return env switch
            {
                EnvSelection.IsDemo => Env.IsDemoBuild,
                EnvSelection.IsFullGame => Env.IsFullGameBuild,
                EnvSelection.IsBooth => Env.IsBoothBuild,
                EnvSelection.IsEditor => Env.IsEditor,
                EnvSelection.IsDebug => Env.IsDebugBuild,
                _ => throw new ArgumentOutOfRangeException(nameof(env), env, null)
            };
        }
    }
}
