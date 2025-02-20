using UnityEngine;

namespace SensenToolkit
{
    public static class ComponentExtensionMethods
    {
        public static bool TryGetComponentInParent<T>(
            this Component component,
            out T result,
            bool includeInactive = false
        ) where T : Component
        {
            result = component.GetComponentInParent<T>(includeInactive);
            return result != null;
        }
    }
}
