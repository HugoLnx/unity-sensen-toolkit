using EasyButtons;
using UnityEngine;
using UnityEngine.Localization;

namespace SensenToolkit
{
    public class LocalizedFontEditorUtils : MonoBehaviour
    {
#if !UNITY_EDITOR
        private void Awake()
        {
            Destroy(this);
        }
#endif

        [Button]
        private void ApplyLocaleRecursive(Locale locale)
        {
            LocalizedFont[] localizedFonts = GetComponentsInChildren<LocalizedFont>(true);
            foreach (LocalizedFont lf in localizedFonts)
            {
                lf.ApplyLocale(locale);
            }
        }

        [Button]
        private void ApplyLocaleAndTestTextRecursive(Locale locale)
        {
            LocalizedFont[] localizedFonts = GetComponentsInChildren<LocalizedFont>(true);
            foreach (LocalizedFont lf in localizedFonts)
            {
                lf.ApplyLocaleAndTestText(locale);
            }
        }
    }
}
