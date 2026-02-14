using UnityEngine;

namespace SensenToolkit
{
    [CreateAssetMenu(menuName = "Sensen/Data/Stat")]
    public class StatSO : ScriptableObject, ISteamStat
    {
        [SerializeField] private string _steamName;
        public string SteamName => _steamName;
    }
}
