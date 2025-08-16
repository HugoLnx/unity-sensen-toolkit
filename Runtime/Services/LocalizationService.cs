using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace SensenToolkit
{
    public class LocalizationService : APermanentSingleton<LocalizationService>
    {
        [SerializeField] private Locale _defaultLocale;
        [SerializeField] private Locale _enLocale;
        public Locale CurrentLocale => LocalizationSettings.SelectedLocale;

        protected override void AwakeSingleton()
        {
            base.AwakeSingleton();
            EnforceDefaultLocale();
        }

        public void EnforceDefaultLocale() => SetLocale(_defaultLocale);
        public void EnforceEnLocale() => SetLocale(_enLocale);

        public void SetLocale(Locale locale)
        {
            LocalizationSettings.SelectedLocale = locale;
        }
    }
}
