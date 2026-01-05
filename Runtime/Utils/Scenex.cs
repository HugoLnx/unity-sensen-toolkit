
using System;
using UnityEngine.SceneManagement;

namespace SensenToolkit
{
    public static class Scenex
    {
        public const string DONT_DESTROY_ON_LOAD_SCENE_NAME = "DontDestroyOnLoad";

        public static string DescribeScene(Scene scene)
        {
            if (scene == null) return "(Scene:null)";
            bool isDontDestroyOnLoad = IsDontDestroyOnLoadScene(scene);
            bool isEmpty = IsEmptyScene(scene);
            return $"(Scene[{scene.buildIndex}:'{scene.name}']{(isDontDestroyOnLoad ? " isDontDestroyOnLoad" : "")}{(isEmpty ? " isEmpty" : "")} '{scene.path}')";
        }

        public static bool IsDontDestroyOnLoadScene(Scene scene)
        {
            return scene != null
            && scene.buildIndex == -1
            && !string.IsNullOrEmpty(scene.name)
            && scene.name.Equals(DONT_DESTROY_ON_LOAD_SCENE_NAME, StringComparison.OrdinalIgnoreCase)
            && (
                string.IsNullOrEmpty(scene.path)
                || scene.path.Equals(DONT_DESTROY_ON_LOAD_SCENE_NAME, StringComparison.OrdinalIgnoreCase)
            );
        }

        public static bool IsEmptyScene(Scene scene)
        {
            return scene == null
                || (scene.buildIndex == -1 && string.IsNullOrEmpty(scene.name) && string.IsNullOrEmpty(scene.path));
        }
    }
}
