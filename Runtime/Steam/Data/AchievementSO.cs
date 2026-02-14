using UnityEngine;

namespace SensenToolkit
{
    [CreateAssetMenu(menuName = "Sensen/Data/Achievement")]
    public class AchievementSO : ScriptableObject, ISteamAchievement
    {
        [SerializeField] private string _steamName;
        public string SteamName => _steamName;
    }
}
