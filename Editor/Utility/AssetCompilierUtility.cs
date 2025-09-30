using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

#if VISUAL_SCRIPTING_1_7
using SMachine = Unity.VisualScripting.ScriptMachine;
#else
using SMachine = Unity.VisualScripting.FlowMachine;
#endif

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
                        var component = obj.GetComponent<SMachine>();
                        gen.current = component;
                        return component;
                    }
                    return gen.current;
                };
            }
        }
    }
}
