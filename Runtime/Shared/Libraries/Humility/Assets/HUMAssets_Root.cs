#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMAssets
    {
        public static Data.FindType Find(this Type type)
        {
            return new Data.FindType(type);
        }

        public static Data.Find Find()
        {
            return new Data.Find();
        }

        public static Data.GameObjectIs Is(this GameObject gameObject)
        {
            return new Data.GameObjectIs(gameObject);
        }

        public static Data.ComponentListTo<T> To<T>(this List<T> components)
            where T : MonoBehaviour
        {
            return new Data.ComponentListTo<T>(components);
        }

        public static List<T> With<T>(this List<GameObject> objects) where T : MonoBehaviour
        {
            var output = new List<T>();

            foreach (GameObject obj in objects)
            {
                var validBehaviour = obj.GetComponent<T>();
                if (validBehaviour != null)
                {
                    output.Add(validBehaviour);
                }
            }

            return output;
        }

        public static List<GameObject> GameObjects<T>(this HUMAssets.Data.ComponentListTo<T> componentsList) where T : MonoBehaviour
        {
            var output = new List<GameObject>();

            foreach (T mono in componentsList.components)
            {
                var obj = mono.gameObject;
                output.Add(obj);
            }

            return output;
        }

        public static void SaveAndDirtyAllOfType<T>(bool refresh = true) where T : UnityEngine.Object
        {
            if (refresh) AssetDatabase.Refresh();
            var assets = Find().Assets().OfType<T>();
            foreach (T asset in assets)
            {
                EditorUtility.SetDirty(asset);
            }
            AssetDatabase.SaveAssets();
        }

        public static void SaveAndDirty(this UnityEngine.Object obj, bool refresh = true)
        {
            if (refresh) AssetDatabase.Refresh();
            EditorUtility.SetDirty(obj);
            AssetDatabase.SaveAssets();
        }

        public static string GetGUID(this UnityEngine.Object obj)
        {
            var guid = string.Empty;
            long localId = 0;
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out guid, out localId);
            return guid;
        }

        public static GameObject SavePrefabAndSelect(string path, GameObject root)
        {
            HUMIO.Ensure(path);
            var prefabPath = path + ((path[path.Length - 1].ToString() == "/") ? string.Empty : "/") + root.name + ".prefab";
            HUMIO.Save(root).Asset(prefabPath, true);
            GameObject.DestroyImmediate(root);
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            return Selection.activeGameObject;
        }

        public static string ProjectWindowPath()
        {
            Type projectWindowUtilType = typeof(ProjectWindowUtil);
            MethodInfo getActiveFolderPath = projectWindowUtilType.GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);
            object obj = getActiveFolderPath.Invoke(null, new object[0]);
            string pathToCurrentFolder = obj.ToString();
            return pathToCurrentFolder;
        }
    }
}
#endif