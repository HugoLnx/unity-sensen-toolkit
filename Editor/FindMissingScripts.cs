using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SensenToolkit.EditorToolkit
{
    public sealed class FindMissingScriptsEditor : Editor
    {
        [MenuItem("Tools/Sensen/FindMissingScripts")]
        public static void FindMissingScripts()
        {
            GameObject[] allObjs = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Debug.Log($"Searching {allObjs.Length} GameObjects for missing scripts...");

            List<GameObject> foundObjects = new();

            foreach (GameObject obj in allObjs)
            {
                Component[] components = obj.GetComponents<Component>();

                foreach (Component component in components)
                {
                    if (component == null)
                    {
                        foundObjects.Add(obj);
                        Debug.Log($"Missing script found in GameObject: {obj.transform.FullPath()}", obj);
                        break;
                    }
                }
            }

            Debug.Log($"Search complete. Found {foundObjects.Count} GameObjects with missing scripts.");
        }
    }
}
