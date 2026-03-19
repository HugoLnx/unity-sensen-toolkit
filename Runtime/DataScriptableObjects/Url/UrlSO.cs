using UnityEngine;

namespace SensenToolkit
{
    [CreateAssetMenu(menuName = "Sensen/URL")]
    public class UrlSO : ScriptableObject
    {
        [SerializeField] private string _url;
        public void Open() => Application.OpenURL(_url);
    }
}
