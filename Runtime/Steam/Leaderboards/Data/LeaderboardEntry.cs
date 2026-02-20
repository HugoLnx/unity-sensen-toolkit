using Steamworks;

namespace SensenToolkit
{
    public struct LeaderboardEntry
    {
        public CSteamID UserId;
        public int Ranking;
        public string Nickname;
        public int Score;
        public bool IsPlayerEntry;
        public ELeaderboardDisplayType DisplayType;

        public string FormatScore()
        {
            return DisplayType switch
            {
                ELeaderboardDisplayType.k_ELeaderboardDisplayTypeTimeSeconds => $"{Score / 60:00}:{Score % 60:00}",
                ELeaderboardDisplayType.k_ELeaderboardDisplayTypeTimeMilliSeconds => FormatScoreWithMillis(Score),
                _ => Score.ToString()
            };
        }

        private static string FormatScoreWithMillis(int score)
        {
            // Take the last three digits of the score
            int milliSeconds = score % 1000;
            //Padding with 0s and taking the first two digits
            string milliSecondsToDisplay = milliSeconds.ToString("D3").Substring(0, 2);
            int seconds = (score % 60000) / 1000;

            int minutes = score / 60000;
            return $"{minutes:00}:{seconds:00}.{milliSecondsToDisplay}";
        }
    }
}
