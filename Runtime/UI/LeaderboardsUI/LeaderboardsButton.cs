using System;
using System.Collections;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace SensenToolkit
{
    public class LeaderboardsButton : MonoBehaviour
    {
        [SerializeField] private SteamLeaderboardSO _customLeaderboardToShow;
        [SerializeField, AutoProperty] private Button _button;
        [SerializeField, AutoProperty]
        private PanelChildVisibilityEvents _visibilityEvents;

        private void Awake()
        {
            _visibilityEvents.OnShow += OnShow;
            _button.onClick.AddListener(OnButtonClicked);
        }

        private IEnumerator Start()
        {
            yield return SteamManager.WaitBooted();
            RefreshInteractable();
        }

        private void OnShow() => RefreshInteractable();

        private void OnButtonClicked()
        {
            LeaderboardsPanel panel = LeaderboardsPanel.Instance;
            panel.ShowLeaderboard(_customLeaderboardToShow);
        }

        private void RefreshInteractable()
        {
            _button.interactable = CheckShouldBeInteractable();
        }

        private bool CheckShouldBeInteractable() => SteamManager.IsFunctional;
    }
}
