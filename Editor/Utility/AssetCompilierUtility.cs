using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.VisualScripting.Community
{
    public static class AssetCompilierUtility
    {
        public static IEnumerable<T> FindObjectsOfTypeIncludingInactive<T>()
        {
            return FindObjectsOfTypeIncludingInactive<T>(SceneManager.GetActiveScene());
        }

        public static IEnumerable<T> FindObjectsOfTypeIncludingInactive<T>(Scene scene)
        {
            if (scene.isLoaded)
            {
                foreach (var rootGameObject in scene.GetRootGameObjects())
                {
                    foreach (var result in rootGameObject.GetComponents<T>())
                    {
                        yield return result;
                    }
                    foreach (var result in rootGameObject.GetComponentsInChildren<T>(true))
                    {
                        yield return result;
                    }
                }
            }
        }

        [InitializeOnLoad]
        private static class ScriptMachineFetcher
        {
            static ScriptMachineFetcher()
            {
                CodeGeneratorValueUtility.requestMachine += (obj) =>
                {
                    var gen = GameObjectGenerator.GetSingleDecorator(obj) as GameObjectGenerator;
                    if (gen.current == null)
                    {
                        var component = obj.GetComponent<ScriptMachine>();
                        gen.current = component;
                        return component;
                    }
                    return gen.current;
                };
            }
        }
    }
}
