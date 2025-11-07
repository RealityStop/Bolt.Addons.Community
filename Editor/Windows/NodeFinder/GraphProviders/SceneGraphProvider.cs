using System.Collections.Generic;
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
            var count = SceneManager.sceneCount;
            for (int i = 0; i < count; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.IsValid() || !scene.isLoaded)
                    continue;
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
}
