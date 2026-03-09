using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public static class FakeTypeRegistry
    {
        private static readonly Dictionary<IGenericContainer, Dictionary<int, FakeGenericParameterType>> fakeGenericParameterRegistry
            = new Dictionary<IGenericContainer, Dictionary<int, FakeGenericParameterType>>();

        // private static readonly Dictionary<CodeAsset, FakeType> fakeTypeRegistry
        //     = new Dictionary<CodeAsset, FakeType>();

        public static FakeGenericParameterType GetOrCreate(
            IGenericContainer container,
            int position,
            string name,
            TypeParameterConstraints constraints = TypeParameterConstraints.None,
            Type baseType = null,
            List<Type> interfaces = null)
        {
            if (container == null)
            {
                throw new ArgumentNullException("Cannot create a FakeGenericParameterType with a null container!");
            }
            if (position < 0)
            {
                throw new ArgumentOutOfRangeException($"Position : {position}, Cannot be less than 0!");
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(name + " Cannot be null or a Empty string!");
            }
            if (!fakeGenericParameterRegistry.TryGetValue(container, out var innerDict))
            {
                innerDict = new Dictionary<int, FakeGenericParameterType>();
                fakeGenericParameterRegistry[container] = innerDict;
            }

            if (!innerDict.TryGetValue(position, out var fake))
            {
                fake = new FakeGenericParameterType(container, name, position, constraints, baseType, interfaces);
                innerDict[position] = fake;
            }

            return fake;
        }

        // public static FakeType GetOrCreate(CodeAsset container, string name, string @namespace, List<Type> interfaces = null)
        // {
        //     if (!fakeTypeRegistry.TryGetValue(container, out var fake))
        //     {
        //         fake = new FakeType(container, name, @namespace, interfaces);
        //         fakeTypeRegistry[container] = fake;
        //     }

        //     return fake;
        // }

        public static bool TryGet(IGenericContainer container, int position, out FakeGenericParameterType type)
        {
            type = null;
            return fakeGenericParameterRegistry.TryGetValue(container, out var inner) && inner.TryGetValue(position, out type);
        }

        public static bool Has(IGenericContainer container)
        {
            return fakeGenericParameterRegistry.TryGetValue(container, out _);
        }

        // public static bool TryGet(CodeAsset container, out FakeType type)
        // {
        //     return fakeTypeRegistry.TryGetValue(container, out type);
        // }

        public static IReadOnlyList<FakeGenericParameterType> GetAll(IGenericContainer container)
        {
            if (fakeGenericParameterRegistry.TryGetValue(container, out var inner))
                return new List<FakeGenericParameterType>(inner.Values);
            return Array.Empty<FakeGenericParameterType>();
        }

        public static void RemoveContainer(IGenericContainer container)
        {
            fakeGenericParameterRegistry.Remove(container);
        }

        // public static void RemoveType(CodeAsset container)
        // {
        //     fakeTypeRegistry.Remove(container);
        // }

        public static void RemoveAtPosition(IGenericContainer container, int position)
        {
            if (fakeGenericParameterRegistry.TryGetValue(container, out var innerDict))
            {
                if (innerDict.ContainsKey(position))
                {
                    innerDict.Remove(position);
                    return;
                }
                Debug.LogWarning("Invalid index no Generic found at " + position);
            }
            else
            {
                Debug.LogWarning("No container found for " + container);
                return;
            }
        }
    }
}
