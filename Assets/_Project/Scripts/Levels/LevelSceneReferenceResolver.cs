using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IdleonGame.Levels
{
    public static class LevelSceneReferenceResolver
    {
        public static T FindInActiveScene<T>() where T : Component
        {
            var scene = SceneManager.GetActiveScene();
            return FindInScene<T>(scene);
        }

        public static T FindInActiveSceneByName<T>(string objectName) where T : Component
        {
            var scene = SceneManager.GetActiveScene();
            return FindInSceneByName<T>(scene, objectName);
        }

        public static T FindInScene<T>(Scene scene) where T : Component
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return null;
            }

            var roots = scene.GetRootGameObjects();
            foreach (var root in roots)
            {
                var component = root.GetComponentInChildren<T>();
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }

        public static void FindAllInScene<T>(Scene scene, List<T> results) where T : Component
        {
            results.Clear();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return;
            }

            var roots = scene.GetRootGameObjects();
            foreach (var root in roots)
            {
                var components = root.GetComponentsInChildren<T>();
                foreach (var component in components)
                {
                    results.Add(component);
                }
            }
        }

        public static T FindInSceneByName<T>(Scene scene, string objectName) where T : Component
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return null;
            }

            var roots = scene.GetRootGameObjects();
            foreach (var root in roots)
            {
                var transforms = root.GetComponentsInChildren<Transform>();
                foreach (var child in transforms)
                {
                    if (child.name == objectName)
                    {
                        return child.GetComponent<T>();
                    }
                }
            }

            return null;
        }
    }
}
