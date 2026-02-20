using System;
using Steamworks;

namespace SensenToolkit
{
    public struct ScoreSubmission
    {
        public string BoardName;
        public ELeaderboardUploadScoreMethod UpdateMethod;
        public int Value;
        public readonly bool IsNull => String.IsNullOrEmpty(BoardName);

        public override readonly bool Equals(object obj)
        {
            return obj is ScoreSubmission submission &&
                BoardName == submission.BoardName &&
                UpdateMethod == submission.UpdateMethod &&
                Value == submission.Value;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(BoardName, UpdateMethod, Value);
        }
    }
}
