using System;
using System.Collections;
using System.Collections.Generic;
using EasyButtons;
using MyBox;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SensenToolkit
{
    public class LeaderboardsPanel : ATransientSingleton<LeaderboardsPanel>
    {
        [Header("Config")]
        [SerializeField] private int _maxEntries = 30;
        [SerializeField] private SteamLeaderboardSO _defaultLeaderboard;
        [SerializeField] private bool _shouldLoadDebugEntries = false;
        [Header("References")]
        [SerializeField, MustBeAssigned] private LeaderboardEntryUI _leaderboardEntryPrefab;
        [Tooltip("Where the entries will be instantiated as children of. Should have a VerticalLayoutGroup.")]
        [SerializeField, MustBeAssigned] private VerticalLayoutGroup _entryRowsContainer;
        [Tooltip("ScrollRect that will be hidden/shown when loading/after loading, parent of the entries container.")]
        [SerializeField, MustBeAssigned] private ScrollRect _entriesScrollRect;
        [SerializeField, MustBeAssigned] private RectTransform _loadingIndicator;
        [SerializeField, MustBeAssigned] private RectTransform _noEntriesIndicator;
        [Header("Filter Toggles")]
        [SerializeField, MustBeAssigned] private RadioButtonGroup _toggleGroup;
        [SerializeField, MustBeAssigned] private RadioButton _topBtnToggle;
        [SerializeField, MustBeAssigned] private RadioButton _aroundPlayerBtnToggle;
        [SerializeField, MustBeAssigned] private RadioButton _friendsBtnToggle;
        [Header("Auto References")]
        [SerializeField, AutoProperty] private PanelFadable _panel;
        [SerializeField, AutoProperty(AutoPropertyMode.Children)]
        private FadableContent _fadableContent;
        [SerializeField, AutoProperty]
        private PanelChildVisibilityEvents _visibilityEvents;

        private RadioButton _lastActiveToggle;

        private SteamLeaderboardSO _currentLeaderboard;
        private SteamLeaderboardSO CurrentLeaderboard => _currentLeaderboard != null
            ? _currentLeaderboard
            : _defaultLeaderboard;
        public bool HasLeaderboard => CurrentLeaderboard != null;
        private SteamLeaderboards Leaderboards => SteamLeaderboards.Instance;

        private bool IsReady => SteamManager.IsFunctional && HasLeaderboard;
        private bool ShouldLoadDebugEntries => !Env.IsProductionBuild
            && (_shouldLoadDebugEntries || !SteamManager.IsFunctional);

        protected override void AwakeSingleton()
        {
            _toggleGroup.OnStateChanged += OnToggleChanged;
            _visibilityEvents.OnShow += LoadEntriesOfActiveToggle;
        }

        public void ShowLeaderboard(SteamLeaderboardSO leaderboard = null)
        {
            _currentLeaderboard = leaderboard != null ? leaderboard : null;
            if (_defaultLeaderboard == null && _currentLeaderboard == null)
            {
                Debug.LogError("No default leaderboard set for LeaderboardsPanel, and no leaderboard provided to ShowLeaderboard.");
                return;
            }
            _panel.Show();
        }


        public void Hide()
        {
            _currentLeaderboard = null;
            _panel.Hide();
        }

        [Button]
        public void LoadTopGlobalEntries()
        {
            if (ShouldLoadDebugEntries)
            {
                LoadDebugEntries();
                return;
            }
            LeaderboardGetAllResult result = CheckIsReady()
                ? Leaderboards.DownloadTopGlobalEntries(
                    CurrentLeaderboard,
                    amount: _maxEntries
                )
                : null;
            StopAllCoroutines();
            StartCoroutine(LoadResults(result));
        }

        [Button]
        public void LoadEntriesAroundPlayer()
        {
            if (ShouldLoadDebugEntries)
            {
                LoadDebugEntries();
                return;
            }
            LeaderboardGetAllResult result = CheckIsReady()
                ? Leaderboards.DownloadEntriesAroundPlayer(
                    CurrentLeaderboard,
                    amount: _maxEntries
                )
                : null;
            StopAllCoroutines();
            StartCoroutine(LoadResults(result));
        }

        [Button]
        public void LoadFriendsEntries()
        {
            if (ShouldLoadDebugEntries)
            {
                LoadDebugEntries();
                return;
            }
            LeaderboardGetAllResult result = CheckIsReady()
                ? Leaderboards.DownloadFriendsEntries(
                    CurrentLeaderboard,
                    maxAmount: _maxEntries
                )
                : null;
            StopAllCoroutines();
            StartCoroutine(LoadResults(result));
        }

        private void LoadDebugEntries()
        {
            StopAllCoroutines();
            LeaderboardGetAllResult result = LeaderboardEntriesGenerator.Instance.GenerateResult(
                CurrentLeaderboard,
                _maxEntries
            );
            StartCoroutine(LoadResults(result));
        }

        private IEnumerator LoadResults(LeaderboardGetAllResult result)
        {
            SetupLoading();
            yield return new WaitWhile(() => _fadableContent.HasChangesToApply);
            bool hasEntries = false;
            if (result != null)
            {
                yield return result.WaitResult();
                List<LeaderboardEntry> entries = result.Entries ?? new List<LeaderboardEntry>();
                foreach (LeaderboardEntry entry in entries)
                {
                    var entryUI = LeaderboardEntryUI.Instantiate(
                        prefab: _leaderboardEntryPrefab,
                        parent: _entryRowsContainer.transform as RectTransform,
                        entry: entry
                    );
                    hasEntries = true;
                }
            }

            if (hasEntries) ShowContent();
            else ShowNoEntries();
        }

        private void OnToggleChanged()
        {
            RadioButton button = _toggleGroup.ActiveButton;
            if (_lastActiveToggle == button) return;
            LoadEntriesOfActiveToggle();
        }

        private void LoadEntriesOfActiveToggle()
        {
            RadioButton button = _toggleGroup.ActiveButton;
            if (button == null) return;

            if (button == _topBtnToggle) LoadTopGlobalEntries();
            else if (button == _aroundPlayerBtnToggle) LoadEntriesAroundPlayer();
            else if (button == _friendsBtnToggle) LoadFriendsEntries();
            else throw new Exception($"Unknown toggle: {button.name}");
            _lastActiveToggle = button;
        }

        private void SetupLoading()
        {
            _fadableContent.ApplyWhenHidden(() =>
            {
                ShowLoading();
                ClearContent();
            });
            _fadableContent.HideAndReshowFading();
        }

        private void ShowLoading()
        {
            _fadableContent.ApplyWhenHidden(() =>
            {
                _entriesScrollRect.gameObject.SetActive(false);
                _noEntriesIndicator.gameObject.SetActive(false);
                _loadingIndicator.gameObject.SetActive(true);
            });
            _fadableContent.HideAndReshowFading();
        }

        private void ShowContent()
        {
            _fadableContent.ApplyWhenHidden(() =>
            {
                _loadingIndicator.gameObject.SetActive(false);
                _noEntriesIndicator.gameObject.SetActive(false);
                _entriesScrollRect.gameObject.SetActive(true);
            });
            _fadableContent.HideAndReshowFading();
        }

        private void ShowNoEntries()
        {
            _fadableContent.ApplyWhenHidden(() =>
            {
                _loadingIndicator.gameObject.SetActive(false);
                _noEntriesIndicator.gameObject.SetActive(true);
                _entriesScrollRect.gameObject.SetActive(false);
            });
            _fadableContent.HideAndReshowFading();
        }

        private void ClearContent()
        {
            _entryRowsContainer
                .transform.DestroyAllChildren();
        }

        private bool CheckIsReady()
        {
            if (!IsReady)
            {
                Debug.LogWarning("Trying to load leaderboards UI before initialization.");
                return false;
            }
            return IsReady;
        }
    }
}
