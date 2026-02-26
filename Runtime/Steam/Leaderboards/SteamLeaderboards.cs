using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Steamworks;
using UnityEngine;

namespace SensenToolkit
{

    // Read more on: https://partner.steamgames.com/doc/api/ISteamUserStats
    public class SteamLeaderboards : APermanentSingleton<SteamLeaderboards>
    {
        private const ELeaderboardUploadScoreMethod KEEP_BEST = ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest;
        private const ELeaderboardUploadScoreMethod FORCE_UPDATE = ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate;
        private const ELeaderboardDataRequest AROUND_USER = ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser;
        private const ELeaderboardDataRequest TOP_GLOBAL = ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal;
        private const ELeaderboardDataRequest FRIENDS_ENTRIES = ELeaderboardDataRequest.k_ELeaderboardDataRequestFriends;

        // Rate limit enforced by Steam API of 10 uploads per 10 minutes
        // From steam documentation: https://partner.steamgames.com/doc/api/ISteamUserStats#UploadLeaderboardScore
        // To control that we don't run into that limit, we use a custom rate limit system with some margin
        // and do delays between submission batches to spread out the submissions over time, even when we have slots available,
        // to avoid running out of slots too fast.
        private const float RATE_LIMIT_TIME_FRAME_MINUTES = 10.15f;
        private const int RATE_LIMIT_MAX_UPLOADS_PER_TIME_FRAME
#if UNITY_EDITOR
        = 3;
#else
        = 10;
#endif
        private const float MIN_DELAY_BETWEEN_SUBMISSION_BATCHES_SECONDS = 20f;
        private const float MAX_DELAY_BETWEEN_SUBMISSION_BATCHES_SECONDS = 120f;

        [SerializeField] private SteamLeaderboardSO[] _leaderboards;

        private const bool ACTIVATE_LOGS = false;
        private Logx _logger;
        private new Logx Logger => _logger ??= Logx.GetLogger(nameof(SteamLeaderboards), activate: ACTIVATE_LOGS);

        private RoundRobinPool<SteamCallHandler<LeaderboardScoreUploaded_t>> _uploadedCallHandlerPool = new(
            factory: () => new SteamCallHandler<LeaderboardScoreUploaded_t>(),
            minSize: 5,
            maxCreations: 10,
            prefill: true
        );
        private RoundRobinPool<SteamCallHandler<LeaderboardScoresDownloaded_t>> _downloadedCallHandlerPool = new(
            factory: () => new SteamCallHandler<LeaderboardScoresDownloaded_t>(),
            minSize: 10,
            maxCreations: 30,
            prefill: true
        );
        private HashSet<SteamLeaderboardSO> _leaderboardsEnsured = new();
        private HashSet<SteamLeaderboardSO> _leaderboardsBeingRequested = new();

        public int SubmissionsRemainingOnTimeFrame { get; private set; } = RATE_LIMIT_MAX_UPLOADS_PER_TIME_FRAME;
        private HashSet<SteamLeaderboardSO> _leaderboardsWithScheduledSubmission = new();
        [NonSerialized] private Coroutine _submitScoreLoopCoroutine;
        [NonSerialized] private WaitForSecondsRealtime _getListResultsPreDelay = new(0.2f);
        [NonSerialized] private bool _isWaitingDelayBetweenSubmissions = false;
        [NonSerialized] private Coroutine _delayBetweenSubmissionsCoroutine;
        [NonSerialized] private bool _isBooted;
        [NonSerialized] private bool _isInitialized;

        public bool IsFunctional => _isBooted && _isInitialized;

        public delegate void RankingChangedEvent(
            int newRanking,
            int oldRanking,
            int score,
            SteamLeaderboardSO leaderboard
        );
        public event RankingChangedEvent OnRankingChanged;

        public delegate void ReceivedPlayerScoreEvent(int score, SteamLeaderboardSO leaderboard);
        public event ReceivedPlayerScoreEvent OnReceivedPlayerScore;

        public delegate SteamCallHandler<LeaderboardScoresDownloaded_t> GetEntriesApiCall(
            int amount, SteamLeaderboard_t leaderboard
        );

        private void OnEnable()
        {
            StartCoroutine(BootCoroutine());
        }

        private IEnumerator BootCoroutine()
        {
            var bootBlackout = AppBootBlackoutService.GetInstanceIfExists();
            bootBlackout.HoldBlackout(this);
            yield return SteamManager.WaitBooted();
            if (SteamManager.IsFunctional)
            {
                foreach (SteamLeaderboardSO leaderboardSO in _leaderboards)
                {
                    yield return EnsureLeaderboard(leaderboardSO);
                }
                _isInitialized = true;
            }
            bootBlackout.ReleaseBlackout(this);
            _isBooted = true;
        }

        private IEnumerator EnsureLeaderboard(SteamLeaderboardSO leaderboardSo)
        {
            if (_leaderboardsEnsured.Contains(leaderboardSo)) yield break;
            yield return SteamManager.WaitBooted();
            if (!SteamManager.IsFunctional) yield break;
            if (_leaderboardsBeingRequested.Contains(leaderboardSo))
            {
                yield return new WaitWhile(() => _leaderboardsBeingRequested.Contains(leaderboardSo));
                yield break;
            }
            _leaderboardsBeingRequested.Add(leaderboardSo);
            try
            {
                SteamCallHandler<LeaderboardFindResult_t> callHandler = new();
                SteamAPICall_t handle = SteamUserStats.FindOrCreateLeaderboard(
                    leaderboardSo.BoardName,
                    leaderboardSo.SortMethod,
                    leaderboardSo.DisplayType
                );
                yield return callHandler.WaitForResult(handle);
                SteamCallResult<LeaderboardFindResult_t> result = callHandler.PopResult();
                SteamLeaderboard_t leaderboard = result.Value.m_hSteamLeaderboard;
                ELeaderboardDisplayType remoteDisplayType = SteamUserStats
                    .GetLeaderboardDisplayType(leaderboard);
                ELeaderboardSortMethod remoteSortMethod = SteamUserStats
                    .GetLeaderboardSortMethod(leaderboard);
                if (remoteDisplayType != leaderboardSo.DisplayType)
                {
                    Debug.LogWarning($"Leaderboard '{leaderboardSo.BoardName}' DISPLAY TYPE mismatch: local '{leaderboardSo.DisplayType}' vs remote '{remoteDisplayType}'");
                }
                if (remoteSortMethod != leaderboardSo.SortMethod)
                {
                    Debug.LogWarning($"Leaderboard '{leaderboardSo.BoardName}' SORT METHOD mismatch: local '{leaderboardSo.SortMethod}' vs remote '{remoteSortMethod}'");
                }

                leaderboardSo.SteamRef = leaderboard;
                _leaderboardsEnsured.Add(leaderboardSo);
                Logger.Info($"Leaderboard found/created {leaderboardSo.BoardName}/{leaderboard} - Success:{result.IsSuccess}");
            }
            finally
            {
                _leaderboardsBeingRequested.Remove(leaderboardSo);
            }
        }

        public void ForceSkipDelayBetweenSubmissionsOnce(bool ensureLoop = true)
        {
            _isWaitingDelayBetweenSubmissions = false;
            Coroutinesx.KillAndNullify(this, ref _delayBetweenSubmissionsCoroutine);
            if (ensureLoop) EnsureCheckSubmitScoreLoop();
        }

        public void ScheduleValueSubmission(
            SteamLeaderboardSO leaderboard,
            int value,
            bool forceUpdate = false,
            bool delayed = true)
        {
            if (!IsFunctional)
            {
                Logger.Info("Leaderboard submission CANCELED (Steam isn't functional)");
                return;
            }
            if (!_leaderboardsEnsured.Contains(leaderboard))
            {
                throw new InvalidOperationException($"Leaderboard '{leaderboard.BoardName}' must be ensured before submitting scores");
            }

            if (forceUpdate || leaderboard.ScoreIsBestThanScheduled(value))
            {
                ScoreSubmission newSubmission = new()
                {
                    BoardName = leaderboard.BoardName,
                    UpdateMethod = forceUpdate ? FORCE_UPDATE : KEEP_BEST,
                    Value = value,
                };
                leaderboard.ScheduledToSubmit = newSubmission;
                _leaderboardsWithScheduledSubmission.Add(leaderboard);

                if (!delayed) ForceSkipDelayBetweenSubmissionsOnce(ensureLoop: false);
            }
            else
            {
                ScoreSubmission? currentSubmission = leaderboard.ScheduledToSubmit;
                int currentScheduled = currentSubmission.HasValue ? currentSubmission.Value.Value : 0;
                bool isBestThanScheduled = leaderboard.ScoreIsBestThanScheduled(value);
                Logger.Info($"Ignored score scheduling for '{leaderboard.BoardName}' value:{value} currentScheduled:{currentScheduled} isBestThanScheduled:{isBestThanScheduled}");
            }

            EnsureCheckSubmitScoreLoop();
        }

        private void EnsureCheckSubmitScoreLoop()
        {
            bool skipSubmitLoop = !SteamManager.IsFunctional
                || _leaderboardsWithScheduledSubmission.Count == 0
                || _submitScoreLoopCoroutine != null;
            // Logger.Info($"EnsureCheckSubmitScoreLoop - Skip:{skipSubmitLoop} (IsFunctional:{SteamManager.IsFunctional} | HasScheduledSubmissions:{_leaderboardsWithScheduledSubmission.Count > 0} | IsLoopRunning:{_submitScoreLoopCoroutine != null})");
            if (skipSubmitLoop) return;
            _submitScoreLoopCoroutine = StartCoroutine(CheckSubmitScoreLoop());
        }

        private IEnumerator CheckSubmitScoreLoop()
        {
            yield return new WaitUntil(() => _isBooted);
            if (!IsFunctional) yield break;
            List<SteamLeaderboardSO> toSubmit = new();
            while (true)
            {
                yield return WaitDelayBetweenSubmissions();
                toSubmit.Clear();
                foreach (SteamLeaderboardSO leaderboard in _leaderboardsWithScheduledSubmission)
                {
                    if (!leaderboard.ScheduledToSubmit.HasValue) continue;
                    toSubmit.Add(leaderboard);
                }
                bool hasSubmitted = false;
                foreach (SteamLeaderboardSO leaderboard in toSubmit)
                {
                    UniTask<bool> checkShouldSkip = ShouldSkipScoreSubmission(leaderboard);
                    bool shouldSkip = false;
                    yield return checkShouldSkip.ToCoroutine((result) => shouldSkip = result);
                    bool hasScoreToSubmit = leaderboard.ScheduledToSubmit.HasValue;
                    if (shouldSkip)
                    {
                        Logger.Info($"Skipping submission for leaderboard '{leaderboard.BoardName}' (score is not better than current or same as already submitted score)");
                        _leaderboardsWithScheduledSubmission.Remove(leaderboard);
                        continue;
                    }
                    yield return ScoreSubmissionCoroutine(leaderboard);
                    hasSubmitted = true;
                }
                if (hasSubmitted) EnsureDelayBetweenSubmissionsCoroutine();
                if (_leaderboardsWithScheduledSubmission.Count == 0)
                {
                    _submitScoreLoopCoroutine = null;
                    yield break;
                }
            }
        }

        private async UniTask<bool> ShouldSkipScoreSubmission(SteamLeaderboardSO leaderboard)
        {
            bool hasSubmission = leaderboard.ScheduledToSubmit.HasValue;
            if (!hasSubmission) return true;

            ScoreSubmission submission = leaderboard.ScheduledToSubmit.Value;
            int remoteScore = await GetRemoteCurrentScoreValue(leaderboard);
            Logger.Info($"CurrentRemoteScore: {remoteScore}");
            if (submission.Value == remoteScore) return true;
            if (submission.UpdateMethod == FORCE_UPDATE) return false;
            return submission.Value == remoteScore
                || (submission.UpdateMethod == KEEP_BEST
                    && leaderboard.ScoreIsBestThanScheduled(remoteScore));
        }

        private async UniTask<int> GetRemoteCurrentScoreValue(SteamLeaderboardSO leaderboardSo)
        {
            var result = new LeaderboardGetAllResult();
            await GetListResults(
                leaderboardSo,
                result,
                amount: 1,
                ApiCallGetEntriesAroundPlayer,
                waitSubmissionsToComplete: false
            ).ToUniTask();
            LeaderboardEntry? playerEntry = result.PlayerEntry;
            return playerEntry == null ? 0 : playerEntry.Value.Score;
        }

        private IEnumerator ScoreSubmissionCoroutine(SteamLeaderboardSO leaderboardSo)
        {
            yield return WaitAndUseSubmissionRateLimitSlot();
            ScoreSubmission submission = leaderboardSo.ScheduledToSubmit.Value;
            ELeaderboardUploadScoreMethod updateMethod = submission.UpdateMethod;
            int value = submission.Value;
            string boardName = submission.BoardName;
            SteamLeaderboard_t leaderboardRef = leaderboardSo.SteamRef.Value;
            Logger.Info($"Submiting HighScore Value {value} ({updateMethod})");
            SteamAPICall_t handle = SteamUserStats.UploadLeaderboardScore(leaderboardRef, updateMethod, value, null, 0);
            SteamCallHandler<LeaderboardScoreUploaded_t> callHandler = _uploadedCallHandlerPool.Get();
            yield return callHandler.WaitForResult(handle);
            SteamCallResult<LeaderboardScoreUploaded_t> result = callHandler.PopResult();
            if (result.IsError)
            {
                throw new Exception($"Leaderboard submission FAILED {value} ({updateMethod})");
            }

            LeaderboardScoreUploaded_t steamResult = result.Value;
            bool wasSuccessful = steamResult.m_bSuccess == 1
                && steamResult.m_bScoreChanged == 1
                && steamResult.m_nScore == value;
            // Logger.Info($"Score Upload Result: {boardName}/{value} {(wasSuccessful ? "SUCCESS" : "FAIL")}: {steamResult.m_nGlobalRankPrevious} ~> {steamResult.m_nGlobalRankNew}  (score:{steamResult.m_nScore})");
            ScoreSubmission? currentSubmission = leaderboardSo.ScheduledToSubmit;
            if (currentSubmission == null
                || (wasSuccessful && currentSubmission.Value.Equals(submission)))
            {
                leaderboardSo.ScheduledToSubmit = null;
                _leaderboardsWithScheduledSubmission.Remove(leaderboardSo);
            }
            bool hasRankingChanged = steamResult.m_nGlobalRankPrevious != steamResult.m_nGlobalRankNew;
            if (wasSuccessful && hasRankingChanged)
            {
                int newRanking = steamResult.m_nGlobalRankNew;
                int oldRanking = steamResult.m_nGlobalRankPrevious;
                Logger.Info($"[ScoreSubmit] Ranking changed {newRanking} ~> {oldRanking} ({value})");
                OnRankingChanged?.Invoke(
                    newRanking: newRanking,
                    oldRanking: oldRanking,
                    score: steamResult.m_nScore,
                    leaderboard: leaderboardSo
                );
            }
            bool isScoreZero = steamResult.m_nScore <= 0;
            if (!wasSuccessful)
            {
                Logger.Info($"[ScoreSubmit] FAILED {value} ({updateMethod})");
            }
            else if (isScoreZero)
            {
                Logger.Info($"[ScoreSubmit] IGNORED (score is zero) {value} ({updateMethod})");
            }
            else
            {
                Logger.Info($"[ScoreSubmit] SUCCESS {value} ({updateMethod})");
                OnReceivedPlayerScore?.Invoke(
                    score: steamResult.m_nScore,
                    leaderboard: leaderboardSo
                );
            }
        }

        private void EnsureDelayBetweenSubmissionsCoroutine()
        {
            if (_isWaitingDelayBetweenSubmissions) return;
            _isWaitingDelayBetweenSubmissions = true;
            Coroutinesx.KillAndNullify(this, ref _delayBetweenSubmissionsCoroutine);
            _delayBetweenSubmissionsCoroutine = StartCoroutine(CoroutineDelayBetweenSubmissions());
        }

        private IEnumerator CoroutineDelayBetweenSubmissions()
        {
            if (!_isWaitingDelayBetweenSubmissions) yield break;
            yield return Coroutinesx.TimedWaitWhile(
                () => _isWaitingDelayBetweenSubmissions,
                ChooseDelayBetweenSubmissionBatches(),
                realtime: true
            );
            _isWaitingDelayBetweenSubmissions = false;
        }

        private float ChooseDelayBetweenSubmissionBatches()
        {
            int totalSlots = RATE_LIMIT_MAX_UPLOADS_PER_TIME_FRAME;
            int usedSlots = totalSlots - SubmissionsRemainingOnTimeFrame;
            float t = (float)usedSlots / totalSlots;
            return Mathf.Lerp(
                MIN_DELAY_BETWEEN_SUBMISSION_BATCHES_SECONDS,
                MAX_DELAY_BETWEEN_SUBMISSION_BATCHES_SECONDS,
                t);
        }

        private IEnumerator WaitDelayBetweenSubmissions()
        {
            if (!_isWaitingDelayBetweenSubmissions) yield break;
            yield return new WaitWhile(() => _isWaitingDelayBetweenSubmissions);
        }

        public LeaderboardGetAllResult DownloadEntriesAroundPlayer(SteamLeaderboardSO leaderboard, int amount = 10)
        {
            if (!IsFunctional) return null;
            var result = new LeaderboardGetAllResult();
            StartCoroutine(GetListResults(leaderboard, result, amount, ApiCallGetEntriesAroundPlayer));
            return result;
        }

        public LeaderboardGetAllResult DownloadTopGlobalEntries(SteamLeaderboardSO leaderboard, int amount = 10)
        {
            if (!IsFunctional) return null;
            var result = new LeaderboardGetAllResult();
            StartCoroutine(GetListResults(leaderboard, result, amount, ApiCallGetTopGlobalEntries));
            return result;
        }

        public LeaderboardGetAllResult DownloadFriendsEntries(SteamLeaderboardSO leaderboard, int? maxAmount = null)
        {
            if (!IsFunctional) return null;
            var result = new LeaderboardGetAllResult();
            StartCoroutine(GetListResults(leaderboard, result, maxAmount ?? -1, ApiCallGetFriendsEntries));
            return result;
        }

        private IEnumerator GetListResults(
            SteamLeaderboardSO leaderboardSo,
            LeaderboardGetAllResult result,
            int amount,
            GetEntriesApiCall getEntriesApiCall,
            bool waitSubmissionsToComplete = true
        )
        {
            yield return new WaitUntil(() => _isBooted);
            if (!IsFunctional) yield break;
            if (!_leaderboardsEnsured.Contains(leaderboardSo))
            {
                throw new InvalidOperationException($"Leaderboard '{leaderboardSo.BoardName}' must be ensured before downloading entries");
            }
            string boardName = leaderboardSo.BoardName;
            if (String.IsNullOrEmpty(boardName))
            {
                Debug.LogError("boardName is required if no default leaderboard is set");
                yield break;
            }
            if (waitSubmissionsToComplete)
            {
                if (_submitScoreLoopCoroutine != null) ForceSkipDelayBetweenSubmissionsOnce();
                yield return Coroutinesx.TimedWaitWhile(
                    () => _submitScoreLoopCoroutine != null && SubmissionsRemainingOnTimeFrame > 0,
                    1f,
                    realtime: true
                );
            }

            SteamCallHandler<LeaderboardScoresDownloaded_t> entriesCall;

            yield return _getListResultsPreDelay;

            SteamLeaderboard_t leaderboard = leaderboardSo.SteamRef.Value;
            entriesCall = getEntriesApiCall(amount, leaderboard);
            yield return entriesCall.WaitForResult();

            LeaderboardScoresDownloaded_t getAllResult = entriesCall.PopResult().Value;
            var entries = new List<LeaderboardEntry>();
            Logger.Info($"Download Leaderboard Entries Result: {getAllResult.m_cEntryCount} {getAllResult.m_hSteamLeaderboard} {getAllResult.m_hSteamLeaderboardEntries}");
            int entriesCount = getAllResult.m_cEntryCount;
            if (amount > 0) entriesCount = Mathf.Min(amount, entriesCount);

            for (int i = 0; i < entriesCount; i++)
            {
                SteamUserStats.GetDownloadedLeaderboardEntry(getAllResult.m_hSteamLeaderboardEntries, i, out LeaderboardEntry_t leaderboardEntry, null, 0);
                var entry = new LeaderboardEntry
                {
                    UserId = leaderboardEntry.m_steamIDUser,
                    Ranking = leaderboardEntry.m_nGlobalRank,
                    Score = leaderboardEntry.m_nScore,
                    Nickname = SteamFriends.GetFriendPersonaName(leaderboardEntry.m_steamIDUser),
                    IsPlayerEntry = leaderboardEntry.m_steamIDUser.Equals(SteamManager.UserId),
                    DisplayType = leaderboardSo.DisplayType,
                };
                entries.Add(entry);
            }
            result.Entries = entries;
            CSteamID currentUserId = SteamManager.UserId;
            foreach (LeaderboardEntry entry in entries)
            {
                if (entry.IsPlayerEntry)
                {
                    result.PlayerEntry = entry;
                    break;
                }
            }
            if (result.PlayerEntry.HasValue)
            {
                OnReceivedPlayerScore?.Invoke(
                    score: result.PlayerEntry.Value.Score,
                    leaderboard: leaderboardSo
                );
            }
        }

        private IEnumerator WaitAndUseSubmissionRateLimitSlot()
        {
            if (this.SubmissionsRemainingOnTimeFrame <= 0)
            {
                yield return new WaitWhile(() => this.SubmissionsRemainingOnTimeFrame <= 0);
            }
            StartCoroutine(UseSubmissionRateLimitSlot());
        }

        private IEnumerator UseSubmissionRateLimitSlot()
        {
            SubmissionsRemainingOnTimeFrame--;
            Logger.Info($"Used a submission slot, remaining slots: {SubmissionsRemainingOnTimeFrame}");
            if (SubmissionsRemainingOnTimeFrame <= 2)
            {
                Debug.LogWarning($"[SteamLeaderboards] Approaching submission rate limit, remaining slots: {SubmissionsRemainingOnTimeFrame}");
            }
            yield return new WaitForSecondsRealtime(RATE_LIMIT_TIME_FRAME_MINUTES * 60f);
            SubmissionsRemainingOnTimeFrame++;
        }

        private SteamCallHandler<LeaderboardScoresDownloaded_t> ApiCallGetEntriesAroundPlayer(
            int amount, SteamLeaderboard_t leaderboard
        )
        {
            int amountBesidesPlayer = amount - 1;
            const int AMOUNT_BEFORE_PLAYER = 10;
            int amountAfterPlayer = amountBesidesPlayer - AMOUNT_BEFORE_PLAYER;
            SteamAPICall_t handle = SteamUserStats.DownloadLeaderboardEntries(leaderboard, AROUND_USER, -AMOUNT_BEFORE_PLAYER, amountAfterPlayer);
            SteamCallHandler<LeaderboardScoresDownloaded_t> callHandler = _downloadedCallHandlerPool.Get();
            callHandler.SetHandle(handle);
            return callHandler;
        }

        private SteamCallHandler<LeaderboardScoresDownloaded_t> ApiCallGetTopGlobalEntries(
            int amount, SteamLeaderboard_t leaderboard
        )
        {
            const int TOP_GLOBAL_RANGE_START = 1;
            SteamAPICall_t handle = SteamUserStats.DownloadLeaderboardEntries(leaderboard, TOP_GLOBAL, TOP_GLOBAL_RANGE_START, amount);
            SteamCallHandler<LeaderboardScoresDownloaded_t> callHandler = _downloadedCallHandlerPool.Get();
            callHandler.SetHandle(handle);
            return callHandler;
        }

        private SteamCallHandler<LeaderboardScoresDownloaded_t> ApiCallGetFriendsEntries(
            int ignoredAmount, SteamLeaderboard_t leaderboard
        )
        {
            SteamAPICall_t handle = SteamUserStats.DownloadLeaderboardEntries(leaderboard, FRIENDS_ENTRIES, 0, 0);
            SteamCallHandler<LeaderboardScoresDownloaded_t> callHandler = _downloadedCallHandlerPool.Get();
            callHandler.SetHandle(handle);
            return callHandler;
        }
    }
}
