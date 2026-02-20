using System;
using MyBox;
using Steamworks;
using UnityEngine;

namespace SensenToolkit
{
    [CreateAssetMenu(fileName = "SteamLeaderboard", menuName = "Sensen/SteamLeaderboard")]
    public class SteamLeaderboardSO : ScriptableObject, IScriptableCallbackSubscriber_OnAppAwake
    {
        [Header("Naming")]
        [SerializeField] private string _baseBoardName;

        [SerializeField] private bool _useEnvSuffix = true;
        [SerializeField] private bool _forceBoardName = false;
        [ConditionalField(nameof(_forceBoardName))]
        [SerializeField] private string _forcedBoardName;
        [Tooltip("The type of data to display in the leaderboard, used to create leaderboards dynamically and define default sort method.")]

        [Header("Steam Config")]
        [SerializeField] private ELeaderboardDisplayType _displayTypeCfg = ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric;
        [Tooltip("If none, it will use ascending for time display type and descending for others.")]
        [SerializeField] private ELeaderboardSortMethod _sortMethodCfg = ELeaderboardSortMethod.k_ELeaderboardSortMethodNone;

        [Header("Preview")]
        [SerializeField, ReadOnly] private string _boardNamePreview;
        [SerializeField, ReadOnly] private ELeaderboardDisplayType _displayTypePreview;
        [SerializeField, ReadOnly] private ELeaderboardSortMethod _sortMethodPreview;

        [NonSerialized] private string _boardName;
        [NonSerialized] private ELeaderboardDisplayType? _displayType;
        [NonSerialized] private ELeaderboardSortMethod? _sortMethod;

        public string BoardName => _boardName ??= ResolveBoardName();
        public ELeaderboardDisplayType DisplayType => _displayType ??= ResolveDisplayType();
        public ELeaderboardSortMethod SortMethod => _sortMethod ??= ResolveSortMethod();
        public SteamLeaderboard_t? SteamRef { get; set; } = null;
        public ScoreSubmission? ScheduledToSubmit { get; set; } = null;

        private bool HigherIsBest => _sortMethodCfg == ELeaderboardSortMethod.k_ELeaderboardSortMethodDescending;

        private void Setup()
        {
            ResolveBoardName(force: true);
            ResolveDisplayType(force: true);
            ResolveSortMethod(force: true);
        }

        private void OnValidate()
        {
            Setup();
            _boardNamePreview = BoardName;
            _displayTypePreview = DisplayType;
            _sortMethodPreview = SortMethod;
        }

        public void ScriptableCallback_OnAppAwake()
        {
            Setup();
            SteamRef = null;
            ScheduledToSubmit = null;
        }

        private string ResolveBoardName(bool force = false)
        {
            if (!string.IsNullOrEmpty(_boardName) && !force) return _boardName;
            _boardName = _forceBoardName ? _forcedBoardName : BuildBoardName();
            return _boardName;
        }

        private ELeaderboardDisplayType ResolveDisplayType(bool force = false)
        {
            if (_displayType.HasValue && !force) return _displayType.Value;
            _displayType = _displayTypeCfg;
            return _displayType.Value;
        }

        private ELeaderboardSortMethod ResolveSortMethod(bool force = false)
        {
            if (_sortMethod.HasValue && !force) return _sortMethod.Value;
            _sortMethod = _sortMethodCfg == ELeaderboardSortMethod.k_ELeaderboardSortMethodNone
                ? ChooseDefaultSortMethod(DisplayType)
                : _sortMethodCfg;
            return _sortMethod.Value;
        }

        private string BuildBoardName()
        {
            if (!_useEnvSuffix) return _baseBoardName;
            string envSuffix = Env.GetEnvId();
            return $"{_baseBoardName}-{envSuffix}";
        }

        private static ELeaderboardSortMethod ChooseDefaultSortMethod(ELeaderboardDisplayType displayType)
        {
            switch (displayType)
            {
                case ELeaderboardDisplayType.k_ELeaderboardDisplayTypeTimeSeconds:
                case ELeaderboardDisplayType.k_ELeaderboardDisplayTypeTimeMilliSeconds:
                    return ELeaderboardSortMethod.k_ELeaderboardSortMethodAscending;
                default:
                    return ELeaderboardSortMethod.k_ELeaderboardSortMethodDescending;
            }
        }

        public bool ScoreIsBestThanScheduled(int score)
        {
            if (!ScheduledToSubmit.HasValue) return true;
            int scheduledScore = ScheduledToSubmit.Value.Value;
            return HigherIsBest ? score > scheduledScore : score < scheduledScore;
        }
    }
}
