using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SensenToolkit
{
    public static class TransformExtensionMethods
    {
        private static readonly List<Transform> s_childrenBuffer = new();
        public static void DestroyAllChildren(this Transform transform, IEnumerable<Transform> except = null, bool immediate = false)
        {
            HashSet<int> exceptIds = except != null
                ? new HashSet<int>(except.Select(t => t.GetInstanceID()))
                : null;
            s_childrenBuffer.Clear();
            foreach (Transform child in transform)
            {
                if (exceptIds?.Contains(child.GetInstanceID()) == true)
                {
                    continue;
                }
                s_childrenBuffer.Add(child);
            }
            if (immediate)
            {
                foreach (Transform child in s_childrenBuffer)
                {
                    GameObject.DestroyImmediate(child.gameObject);
                }
            }
            else
            {
                foreach (Transform child in s_childrenBuffer)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        // public static string FullPath(this Transform transform)
        // {
        //     List<string> fullPathList = new();

        //     Transform currentTransform = transform;
        //     while (currentTransform != null)
        //     {
        //         fullPathList.Add(currentTransform.gameObject.name);
        //         currentTransform = currentTransform.parent;
        //     }

        //     fullPathList.Reverse();
        //     return string.Join("/", fullPathList);
        // }
    }
}
