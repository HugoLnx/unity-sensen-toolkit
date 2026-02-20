using System.Collections.Generic;
using System.Linq;
using Steamworks;
using UnityEngine;

namespace SensenToolkit
{
    public class LeaderboardEntriesGenerator
    {
        private const int MIN_SCORE_BASE = 75;
        private const int MAX_SCORE_BASE = 100000;
        private const int SCORE_BASE_MULTIPLIER = 5;
        private const ELeaderboardDisplayType DEFAULT_DISPLAY_TYPE = ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric;
        private const ELeaderboardSortMethod DEFAULT_SORT_METHOD = ELeaderboardSortMethod.k_ELeaderboardSortMethodDescending;
        private readonly string[] _fakeNames = new[] {
            "John", "Jane", "Alice", "Bob", "Charlie", "David", "Eve", "Frank", "Grace",
            "Heidi", "Ivan", "Jack", "Kate", "Liam", "Mia", "Nina", "Oliver", "Pam",
            "Quinn", "Riley", "Sara", "Tom", "Ursula", "Violet", "Wendy", "Xander", "Yara",
            "Zoe", "Adam", "Eva", "Noah", "Emma", "Luna", "Leo", "Mila", "Owen", "Ava",
            "Ryan", "Isla", "Zara", "Ella", "Liam", "Mia", "Nina", "Oliver", "Pam",
        };

        private readonly string[] _fakeSurnames = new[] {
            "Smith", "Johnson", "Williams", "Jones", "Brown", "Davis", "Miller", "Wilson", "Moore",
            "Taylor", "Anderson", "Thomas", "Jackson", "White", "Harris", "Martin", "Thompson", "Garcia",
            "Martinez", "Robinson", "Clark", "Rodriguez", "Lewis", "Lee", "Walker", "Hall", "Allen",
            "Young", "Hernandez", "King", "Wright", "Lopez", "Hill", "Scott", "Green", "Adams",
            "Baker", "Gonzalez", "Nelson", "Carter", "Mitchell", "Perez", "Roberts", "Turner", "Phillips",
            "Campbell", "Parker", "Evans", "Edwards", "Collins", "Stewart", "Sanchez", "Morris", "Rogers",
        };
        public static LeaderboardEntriesGenerator Instance => s_instance ??= new LeaderboardEntriesGenerator();
        private static LeaderboardEntriesGenerator s_instance;
        private LeaderboardEntriesGenerator() { }
        public LeaderboardGetAllResult GenerateResult(
            int amount,
            ELeaderboardDisplayType displayType = default,
            ELeaderboardSortMethod sortMethod = default)
        {
            List<LeaderboardEntry> entries = GenerateEntries(amount, displayType, sortMethod);
            LeaderboardEntry playerEntry = entries.FirstOrDefault(entry => entry.IsPlayerEntry);
            return new LeaderboardGetAllResult
            {
                Entries = entries,
                PlayerEntry = playerEntry,
            };
        }

        public List<LeaderboardEntry> GenerateEntries(
            int maxAmount,
            ELeaderboardDisplayType displayType = default,
            ELeaderboardSortMethod sortMethod = default)
        {
            displayType = displayType == default ? DEFAULT_DISPLAY_TYPE : displayType;
            sortMethod = sortMethod == default ? DEFAULT_SORT_METHOD : sortMethod;
            bool isEmpty = Random.value < 1f / 3f;
            if (isEmpty) return new List<LeaderboardEntry>();
            int amount = Random.Range(5, maxAmount);
            int playerInx = Random.Range(0, Mathf.Min(amount, 10));
            IEnumerable<LeaderboardEntry> entries = EnumerateNicknames(amount)
            .Select((nickname, i) => new LeaderboardEntry
            {
                UserId = new Steamworks.CSteamID((ulong)i),
                Nickname = nickname,
                Score = GenerateScoreForDisplayType(displayType),
                DisplayType = displayType,
            });

            switch (sortMethod)
            {
                case ELeaderboardSortMethod.k_ELeaderboardSortMethodAscending:
                    entries = entries.OrderBy(entry => entry.Score);
                    break;
                case ELeaderboardSortMethod.k_ELeaderboardSortMethodDescending:
                    entries = entries.OrderByDescending(entry => entry.Score);
                    break;
            }

            return entries.Select((entry, i) =>
            {
                entry.Ranking = i + 1;
                entry.IsPlayerEntry = i == playerInx;
                return entry;
            }).ToList();
        }

        private IEnumerable<string> EnumerateNicknames(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                string name = _fakeNames[Random.Range(0, _fakeNames.Length)];
                string surname = _fakeSurnames[Random.Range(0, _fakeSurnames.Length)];
                yield return $"{name} {surname}";
            }
        }

        private int GenerateScoreForDisplayType(ELeaderboardDisplayType displayType)
        {
            return displayType switch
            {
                ELeaderboardDisplayType.k_ELeaderboardDisplayTypeTimeSeconds => Random.Range(15, 59 * 60),
                ELeaderboardDisplayType.k_ELeaderboardDisplayTypeTimeMilliSeconds => Random.Range(15 * 1000, 59 * 60 * 1000),
                _ => Random.Range(MIN_SCORE_BASE, MAX_SCORE_BASE) * SCORE_BASE_MULTIPLIER,
            };
        }
    }
}
