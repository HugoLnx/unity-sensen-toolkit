using UnityEngine;

namespace SensenToolkit
{
    [CreateAssetMenu(fileName = "LocalizedFontOverrides", menuName = "Sensen/Localization/LocalizedFontOverridesSO", order = 1)]
    public class LocalizedFontDataOverridesSO : ScriptableObject
    {
        public LocalizedFontLocaleOverrides[] Overrides;
    }
}
