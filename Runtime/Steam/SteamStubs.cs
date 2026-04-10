#if DISABLESTEAMWORKS
namespace Steamworks
{
    public enum ELeaderboardUploadScoreMethod
    {
        k_ELeaderboardUploadScoreMethodNone,
        k_ELeaderboardUploadScoreMethodKeepBest,
        k_ELeaderboardUploadScoreMethodForceUpdate
    }

    public enum ELeaderboardDataRequest
    {
        k_ELeaderboardDataRequestGlobal,
        k_ELeaderboardDataRequestGlobalAroundUser,
        k_ELeaderboardDataRequestFriends,
        k_ELeaderboardDataRequestUsers
    }

    public enum ELeaderboardDisplayType
    {
        k_ELeaderboardDisplayTypeNone = 0,
        k_ELeaderboardDisplayTypeNumeric = 1,
        k_ELeaderboardDisplayTypeTimeSeconds = 2,
        k_ELeaderboardDisplayTypeTimeMilliSeconds = 3
    }

    public enum ELeaderboardSortMethod
    {
        k_ELeaderboardSortMethodNone = 0,
        k_ELeaderboardSortMethodAscending = 1,
        k_ELeaderboardSortMethodDescending = 2
    }

    public struct SteamLeaderboard_t
    {
        public ulong m_SteamLeaderboard;
    }

    public struct LeaderboardEntry_t
    {
        public ulong m_steamIDUser;
        public int m_nGlobalRank;
        public int m_nScore;
    }

    public struct CSteamID
    {
        public ulong m_SteamID;
    }

    public struct CGameID
    {
        public ulong m_GameID;
    }

    public struct LeaderboardScoresDownloaded_t
    {
        public SteamLeaderboard_t m_hSteamLeaderboard;
        public int m_cEntryCount;
        public int m_cEntryMax;
    }

    public struct LeaderboardScoreUploaded_t
    {
        public SteamLeaderboard_t m_hSteamLeaderboard;
        public int m_nScore;
        public bool m_bSuccess;
    }

    public struct SteamAPICall_t
    {
        public ulong m_SteamAPICall;
    }

    public struct UserStatsReceived_t
    {
        public CGameID m_nGameID;
        public CSteamID m_steamIDUser;
        public int m_eResult;
    }

    public struct CallResult<T>
    {
        public void Set(SteamAPICall_t apiCall, object callback)
        {
        }

        public static CallResult<T> Create(object callback)
        {
            return new CallResult<T>();
        }
    }

    public struct Callback<T>
    {
        public void Register(object callback)
        {
        }
    }
}
#endif
