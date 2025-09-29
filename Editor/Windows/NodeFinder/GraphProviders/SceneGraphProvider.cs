using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Community;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.VisualScripting.Community 
{
    public abstract class SceneGraphProvider : BaseGraphProvider
    {
        protected SceneGraphProvider(NodeFinderWindow window) : base(window)
        {
        }
        
        protected IEnumerable<T> GetTargetsFromScene<T>()
        {
            var scene = SceneManager.GetActiveScene();
            foreach (var rootGameObject in scene.GetRootGameObjects())
            {
                foreach (var result in rootGameObject.GetComponentsInChildren<T>(true))
                {
                    yield return result;
                }
            }
        }
    } 
}
