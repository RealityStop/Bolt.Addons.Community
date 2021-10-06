#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMAssets_Children
    {
        public static HUMAssets.Data.All All(this HUMAssets.Data.Find find)
        {
            return new HUMAssets.Data.All(find);
        }

        public static HUMAssets.Data.FindTypeAll All(this HUMAssets.Data.FindType find)
        {
            return new HUMAssets.Data.FindTypeAll(find);
        }

        public static HUMAssets.Data.Assets Assets(this HUMAssets.Data.Find find)
        {
            return new HUMAssets.Data.Assets(find);
        }

        public static ScriptableObject[] ScriptableObjects(this HUMAssets.Data.FindType find)
        {
            string[] assetIds = UnityEditor.AssetDatabase.FindAssets("t:" + find.type);
            var assetsList = new List<ScriptableObject>();
            if (find.type.IsInterface)
            {
                assetIds = UnityEditor.AssetDatabase.FindAssets("t:" + typeof(ScriptableObject));
                assetsList = new List<ScriptableObject>();

                foreach (string assetId in assetIds)
                {
                    var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<ScriptableObject>(UnityEditor.AssetDatabase.GUIDToAssetPath(assetId)) as ScriptableObject;
                    if (asset != null && asset.GetType().Inherits(find.type)) assetsList.Add(asset);
                }
            }
            else
            {
                assetIds = UnityEditor.AssetDatabase.FindAssets("t:" + find.type);
                assetsList = new List<ScriptableObject>();

                foreach (string assetId in assetIds)
                {
                    var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<ScriptableObject>(UnityEditor.AssetDatabase.GUIDToAssetPath(assetId)) as ScriptableObject;
                    if (asset != null) assetsList.Add(asset);
                }
            }

            return assetsList.ToArray();
        }

        public static T[] ScriptableObjects<T>(this HUMAssets.Data.FindType find)
        {
            return ScriptableObjects(find).Cast<T>().ToArray();
        }
        public static List<TCastTo> Assets<TCastTo>(this HUMAssets.Data.FindType find) where TCastTo : UnityEngine.Object
        {
            var assetIds = UnityEditor.AssetDatabase.FindAssets("t:" + find.type);
            var assetsList = new List<TCastTo>();

            foreach (string assetId in assetIds)
            {
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(UnityEditor.AssetDatabase.GUIDToAssetPath(assetId)) as TCastTo;
                if (asset != null) assetsList.Add(asset);
            }

            return assetsList;
        }

        public static HUMAssets.Data.AssetsUsingType Assets(this HUMAssets.Data.FindUsingType find)
        {
            return new HUMAssets.Data.AssetsUsingType(find);
        }

        public static HUMAssets.Data.Prefabs Prefabs(this HUMAssets.Data.Find find)
        {
            return new HUMAssets.Data.Prefabs(find);
        }

        public static HUMAssets.Data.PrefabsUsingType Prefabs(this HUMAssets.Data.FindUsingType find)
        {
            return new HUMAssets.Data.PrefabsUsingType(find);
        }

        public static HUMAssets.Data.Behaviours Behaviours(this HUMAssets.Data.Find find)
        {
            return new HUMAssets.Data.Behaviours(find);
        }

        public static HUMAssets.Data.BehavioursUsingType Behaviours(this HUMAssets.Data.FindUsingType find)
        {
            return new HUMAssets.Data.BehavioursUsingType(find);
        }

        public static bool Prefab(this HUMAssets.Data.GameObjectIs @is)
        {
            return @is.gameObject.scene == null || @is.gameObject.scene.name == null;
        }

        public static HUMAssets.Data.AssetsWith With(this HUMAssets.Data.Assets assets)
        {
            return new HUMAssets.Data.AssetsWith(assets);
        }

        public static List<T> Name<T>(this HUMAssets.Data.AssetsWith assets, string name) where T : UnityEngine.Object
        {
            var assetIds = UnityEditor.AssetDatabase.FindAssets(name);
            var assetsList = new List<T>();

            foreach (string assetId in assetIds)
            {
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(UnityEditor.AssetDatabase.GUIDToAssetPath(assetId));
                assetsList.Add(asset);
            }

            return assetsList;
        }

        public static List<UnityEngine.Object> Name(this HUMAssets.Data.AssetsWithUsingType assetsWithUsingType, string name)
        {
            var assetIds = UnityEditor.AssetDatabase.FindAssets(name);
            var assetsList = new List<UnityEngine.Object>();

            foreach (string assetId in assetIds)
            {
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath(UnityEditor.AssetDatabase.GUIDToAssetPath(assetId), assetsWithUsingType.assets.find.type);
                assetsList.Add(asset);
            }

            return assetsList;
        }

        public static List<T> OfType<T>(this HUMAssets.Data.Assets assets) where T : class
        {
            var assetIds = UnityEditor.AssetDatabase.FindAssets("t:" + typeof(UnityEngine.Object));
            var assetsList = new List<T>();

            foreach (string assetId in assetIds)
            {
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(UnityEditor.AssetDatabase.GUIDToAssetPath(assetId)) as T;
                if (asset != null) assetsList.Add(asset);
            }
              
            return assetsList;
        }

        public static List<UnityEngine.Object> OfType(this HUMAssets.Data.AssetsUsingType assets)
        {
            var assetIds = UnityEditor.AssetDatabase.FindAssets("t:" + assets.find.type.FullName);
            var assetsList = new List<UnityEngine.Object>();

            foreach (string assetId in assetIds)
            {
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath(UnityEditor.AssetDatabase.GUIDToAssetPath(assetId), assets.find.type);
                assetsList.Add((ScriptableObject)asset);
            }

            return assetsList;
        }

        public static List<GameObject> Prefabs(this HUMAssets.Data.All all)
        {
            var objectIds = UnityEditor.AssetDatabase.FindAssets("t:GameObject");
            var objects = new List<GameObject>();

            foreach (string pathId in objectIds)
            {
                var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(UnityEditor.AssetDatabase.GUIDToAssetPath(pathId));
                if (obj.scene.name == null) objects.Add(obj);
            }

            return objects;
        }

        public static List<GameObject> GameObjects(this HUMAssets.Data.All all)
        {
            var objectIds = UnityEditor.AssetDatabase.FindAssets("t:GameObject");
            var objects = new List<GameObject>();

            foreach (string pathId in objectIds)
            {
                var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(UnityEditor.AssetDatabase.GUIDToAssetPath(pathId));
                objects.Add(obj);
            }

            return objects;
        }

        public static List<T> With<T>(this HUMAssets.Data.Prefabs prefabs) where T : MonoBehaviour
        {
            var _prefabs = HUMAssets.Find().All().Prefabs();

            var output = new List<T>();

            foreach (GameObject prefab in _prefabs)
            {
                var validBehaviour = prefab.GetComponent<T>();
                if (validBehaviour != null)
                {
                    output.Add(validBehaviour);
                }
            }

            return output;
        }
    }
}
#endif