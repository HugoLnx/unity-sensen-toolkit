using UnityEngine.Localization;

namespace SensenToolkit
{
    public class LocStringx
    {
        public static bool IsEmptyOrNull(LocalizedString str)
            => str == null || str.IsEmpty;

        public static bool IsPresent(LocalizedString current)
            => !IsEmptyOrNull(current);
    }
}
