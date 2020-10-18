using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Bolt.Addons.Community.Utility
{
    public class CommunityUnitsAOTLoader : MonoBehaviour
    {
        public static CommunityUnitsAOTLoader playerInstance;
        void Awake()
        {
            DontDestroyOnLoad(this);

            if (playerInstance == null)
            {
                playerInstance = this;
            }
            else
            {
                DestroyObject(gameObject);
            }
        }

        public static void Construct()
        {
            var newObj = new GameObject("Addons AOT Loader");
            newObj.AddComponent<CommunityUnitsAOTLoader>().FindAndStoreLoaders();
        }


        public void FindAndStoreLoaders()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (IsDependentOnUtility(assembly))
                {
                    Debug.Log($"Loader assembly found: {assembly.GetName()}");
                    foreach (var loaderType in assembly.GetTypesSafely().Where(x => IsLoader(x) && x.IsConcrete() && x.GetDefaultConstructor() != null))
                    {
                        Debug.Log($"Loader type found: {loaderType.Name} from {assembly.GetName()}");
                        if (GetComponent(loaderType) == null)
                            gameObject.AddComponent(loaderType);
                    }
                }
            }
        }

        private bool IsDependentOnUtility(Assembly assembly)
        {
            if (assembly.GetName().Name == "Bolt.Addons.Community.Utility")
                return false;

            foreach (var dependency in assembly.GetReferencedAssemblies())
            {
                if (dependency.Name == "Bolt.Addons.Community.Utility")
                {
                    return true;
                }
            }

            return false;
        }

        //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        //static void OnBeforeSceneLoadRuntimeMethod()
        //{
        //    Debug.Log("Before first Scene loaded");
        //}

        private bool IsLoader(Type type)
        {
            return typeof(IAOTLoader).IsAssignableFrom(type);
        }
    }
}