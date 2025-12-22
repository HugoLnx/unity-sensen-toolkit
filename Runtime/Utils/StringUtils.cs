using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace SensenToolkit
{
    public static class StringUtils
    {
        private static readonly HashSet<char> s_commonLetters = new("aeiosr0-_ ,.;".ToArray());
        public static string FindMostSimilar(IEnumerable<string> strs, string target)
        {
            if (target == null) return null;

            HashSet<char> targetLetters = HashSetPool<char>.Get();
            int totalTargetCommons = 0;
            int totalTargetRares = 0;

            foreach (char c in target.ToLowerInvariant())
            {
                if (targetLetters.Contains(c)) continue;
                targetLetters.Add(c);

                if (s_commonLetters.Contains(c)) totalTargetCommons++;
                else totalTargetRares++;
            }

            float SimilarityScoreToTarget(string s)
            {
                if (s == null) return 0f;

                int totalStrCommons = 0;
                int totalStrRares = 0;
                int sharedCommons = 0;
                int sharedRares = 0;
                HashSet<char> strLetters = HashSetPool<char>.Get();

                foreach (char c in s.ToLowerInvariant())
                {
                    if (strLetters.Contains(c)) continue;
                    strLetters.Add(c);

                    bool isCommon = s_commonLetters.Contains(c);
                    bool isInTarget = targetLetters.Contains(c);

                    if (isCommon)
                    {
                        totalStrCommons++;
                        if (isInTarget) sharedCommons++;
                    }
                    else
                    {
                        totalStrRares++;
                        if (isInTarget) sharedRares++;
                    }
                }

                HashSetPool<char>.Release(strLetters);

                float targetRaresScore = totalTargetRares > 0 ? (float)sharedRares / totalTargetRares : 0f;
                float targetCommonsScore = totalTargetCommons > 0 ? (float)sharedCommons / totalTargetCommons : 0f;
                float strRaresScore = totalStrRares > 0 ? (float)sharedRares / totalStrRares : 0f;
                float strCommonsScore = totalStrCommons > 0 ? (float)sharedCommons / totalStrCommons : 0f;

                int maxLength = Math.Max(s.Length, target.Length);
                int minLength = Math.Min(s.Length, target.Length);
                float lengthScore = maxLength > 0 ? (float)minLength / maxLength : 0f;

                const float TARGET_RARES_WEIGHT = 0.45f;
                const float TARGET_COMMONS_WEIGHT = 0.15f;
                const float STR_RARES_WEIGHT = 0.15f;
                const float STR_COMMONS_WEIGHT = 0.1f;
                const float LENGTH_WEIGHT = 0.15f;
                float finalScore = (targetCommonsScore * TARGET_COMMONS_WEIGHT)
                    + (targetRaresScore * TARGET_RARES_WEIGHT)
                    + (strCommonsScore * STR_COMMONS_WEIGHT)
                    + (strRaresScore * STR_RARES_WEIGHT)
                    + (lengthScore * LENGTH_WEIGHT);

                return finalScore;
            }

            float maxScore = float.MinValue;
            string bestMatch = null;

            foreach (string s in strs)
            {
                if (s == null) continue;
                float score = SimilarityScoreToTarget(s);
                if (score > maxScore)
                {
                    maxScore = score;
                    bestMatch = s;
                }
            }

            HashSetPool<char>.Release(targetLetters);

            return bestMatch;
        }
    }
}
