using System.Collections.Generic;
using EasyButtons;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace SensenToolkit
{
    [CreateAssetMenu(menuName = "Sensen/SOIndexer")]
    public class ScriptableObjectsIndexer : ScriptableObject
    {
        [SerializeField] private List<ScriptableObject> _all = new();
        public IReadOnlyList<ScriptableObject> All => _all;

#if UNITY_EDITOR
        private void OnEnable()
        {
            if (Application.isPlaying) return;
            RefreshIndex();
        }
        private void OnValidate()
        {
            if (Application.isPlaying) return;
            RefreshIndex();
        }
#endif

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        [Button]
        public void RefreshIndex()
        {
            _all.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:ScriptableObject"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ScriptableObject so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (so != null && so != this)
                {
                    _all.Add(so);
                }
            }
            EditorUtility.SetDirty(this);
        }
    }
}
