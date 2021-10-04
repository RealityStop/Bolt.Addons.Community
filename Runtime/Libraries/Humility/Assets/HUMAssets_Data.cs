using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMAssets
    {
        public static partial class Data
        {
            public struct GameObjectIs
            {
                public GameObject gameObject;

                public GameObjectIs(GameObject gameObject)
                {
                    this.gameObject = gameObject;
                }
            }

            public struct ComponentListTo<T> where T : MonoBehaviour
            {
                public IEnumerable<T> components;

                public ComponentListTo(IEnumerable<T> components)
                {
                    this.components = components;
                }
            }

            public struct Find { }

            public struct FindType
            {
                public Type type;

                public FindType(Type type)
                {
                    this.type = type;
                }
            }

            public struct All
            {
                public Find find;

                public All(Find find)
                {
                    this.find = find;
                }
            }

            public struct FindTypeAll
            {
                public FindType find;

                public FindTypeAll(FindType find)
                {
                    this.find = find;
                }
            }

            public struct FindUsingType
            {
                public Type type;

                public FindUsingType(Type type)
                {
                    this.type = type;
                }
            }

            public struct AssetsUsingType
            {
                public FindUsingType find;

                public AssetsUsingType(FindUsingType find)
                {
                    this.find = find;
                }
            }

            public struct Assets
            {
                public Find find;

                public Assets(Find find)
                {
                    this.find = find;
                }
            }

            public struct FindTypeOfAssets
            {
                public FindType find;

                public FindTypeOfAssets(FindType find)
                {
                    this.find = find;
                }
            }

            public struct AssetsWith
            {
                public Assets assets;

                public AssetsWith(Assets assets)
                {
                    this.assets = assets;
                }
            }

            public struct AssetsWithUsingType
            {
                public AssetsUsingType assets;

                public AssetsWithUsingType(AssetsUsingType assets)
                {
                    this.assets = assets;
                }
            }

            public struct PrefabsUsingType
            {
                public FindUsingType find;

                public PrefabsUsingType(FindUsingType find)
                {
                    this.find = find;
                }
            }

            public struct Prefabs
            {
                public Find find;

                public Prefabs(Find find)
                {
                    this.find = find;
                }
            }

            public struct BehavioursUsingType
            {
                public FindUsingType find;

                public BehavioursUsingType(FindUsingType find)
                {
                    this.find = find;
                }
            }

            public struct Behaviours
            {
                public Find find;

                public Behaviours(Find find)
                {
                    this.find = find;
                }
            }
        }
    }
}