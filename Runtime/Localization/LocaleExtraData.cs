using System;
using MyBox;
using UnityEngine.Localization;

namespace SensenToolkit
{
    [Serializable]
    public class LocaleExtraData
    {
        [MustBeAssigned] public Locale Locale;
        [MustBeAssigned] public string EnglishName;
        [MustBeAssigned] public string NativeName;
    }
}
