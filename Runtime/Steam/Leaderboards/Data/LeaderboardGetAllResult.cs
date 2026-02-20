using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SensenToolkit
{
    public class LeaderboardGetAllResult
    {
        public List<LeaderboardEntry> Entries = null;
        public LeaderboardEntry? PlayerEntry = null;

        public IEnumerator WaitResult()
        {
            yield return new WaitUntil(() => Entries != null);
        }
    }
}
