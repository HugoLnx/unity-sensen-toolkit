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

        public static string NameWithParent(this Component component)
        {
            Transform parent = component.transform.parent;
            return parent == null
                ? component.name
                : $"{parent.name}/{component.name}";
        }
    }
}
