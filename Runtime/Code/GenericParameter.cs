using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;
using UnityEngine.Pool;

namespace Unity.VisualScripting.Community
{
    public class GenericParameter : ISerializationCallbackReceiver
    {
        private static readonly ObjectPool<GenericParameter> parameterPool = new ObjectPool<GenericParameter>(
            createFunc: () => new GenericParameter(),
            actionOnGet: item => item.Clear(),
            actionOnRelease: item => item.Clear(),
            actionOnDestroy: null,
            defaultCapacity: 10
        );

        private static readonly Dictionary<Type, Type> arrayBaseTypeCache = new Dictionary<Type, Type>();
        private static readonly Dictionary<(Type, Type[]), Type> constructedTypeCache = new Dictionary<(Type, Type[]), Type>();

        public GenericParameter parent;
        public bool HasParent => parent != null;
        public SystemType type = new SystemType();
        public string name = "(null)";
        public SystemType selectedType = new SystemType();
        [SerializeField]
        [HideInInspector]
        public List<GenericParameter> nestedParameters = new List<GenericParameter>();
        [SerializeField]
        [HideInInspector]
        private SerializationData nestedParametersSerialization;
        [SerializeField]
        [HideInInspector]
        private SerializationData typeParameterConstraintsSerialization;
        public Type baseTypeConstraint;
        public TypeParameterConstraints typeParameterConstraints;
        public Type[] constraints;
#if UNITY_EDITOR
        public bool isOpen;
        public bool interfaceConstraintsOpen;
#endif

        public void SetName(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Private constructor for object pooling
        /// </summary>
        private GenericParameter()
        {
        }

        public static GenericParameter Create(Type type, string name)
        {
            var parameter = parameterPool.Get();
            parameter.Initialize(type, name);
            return parameter;
        }

        private void Initialize(Type type, string name)
        {
            this.type = new SystemType(type);
            this.name = name;
            selectedType = new SystemType(type);
        }

        public void Release()
        {
            Clear();
            parameterPool.Release(this);
        }

        public GenericParameter(Type _type, string _name)
        {
            type = new SystemType(_type);
            name = _name;
            selectedType = new SystemType(_type);
        }

        public GenericParameter(GenericParameter parent, Type _type, string _name) : this(_type, _name)
        {
            this.parent = parent;
        }

        public GenericParameter(GenericParameter genericParameter, bool typeDefinitions = false)
        {
            type = genericParameter.type;
            name = genericParameter.name;
            selectedType = genericParameter.selectedType;
            if (typeDefinitions)
            {
                genericParameter.SetToTypeDefinition();
                nestedParameters = genericParameter.nestedParameters;
            }
            else
            {
                nestedParameters = genericParameter.nestedParameters;
            }
        }

        public void SetToTypeDefinition()
        {
            if (type.type.IsGenericType)
            {
                type.type = type.type.GetGenericTypeDefinition();
                if (selectedType.type.IsGenericType)
                    selectedType.type = selectedType.type.GetGenericTypeDefinition();
            }
            else if (type.type.IsArray)
            {
                var tempType = type.type.GetElementType();
                int arrays = 1;
                while (tempType.IsArray)
                {
                    tempType = tempType.GetElementType();
                    arrays++;
                }

                if (tempType.IsGenericType)
                {
                    type.type = tempType.GetGenericTypeDefinition();
                    selectedType.type = tempType.GetGenericTypeDefinition();
                    for (int i = 0; i < arrays; i++)
                    {
                        type.type = type.type.MakeArrayType();
                        selectedType.type = type.type.MakeArrayType();
                    }
                }
            }

            foreach (var nestedParam in nestedParameters)
            {
                nestedParam.SetToTypeDefinition();
            }
        }

        public void Clear()
        {
            foreach (var item in nestedParameters)
            {
                item.Clear();
            }
            nestedParameters.Clear();
        }

        public void OnAfterDeserialize()
        {
            nestedParameters = (List<GenericParameter>)nestedParametersSerialization.Deserialize();
            typeParameterConstraints = (TypeParameterConstraints)typeParameterConstraintsSerialization.Deserialize();
        }

        public void OnBeforeSerialize()
        {
            nestedParametersSerialization = nestedParameters.Serialize();
            typeParameterConstraintsSerialization = typeParameterConstraints.Serialize();
        }

        public Type ConstructType()
        {
            if (type?.type == null) return null;

            try
            {
                if (type.type.IsGenericType)
                {
                    return ConstructGenericType();
                }
                else if (type.type.IsArray)
                {
                    return ConstructArrayType();
                }

                return type.type;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to construct type: {ex.Message}");
                return type.type;
            }
        }

        private Type ConstructGenericType()
        {
            var genericArgs = nestedParameters.Select(tp => tp.ConstructType()).ToArray();
            var key = (type.type.GetGenericTypeDefinition(), genericArgs);

            if (constructedTypeCache.TryGetValue(key, out var cachedType))
            {
                return cachedType;
            }

            ValidateGenericArguments(genericArgs);
            var constructedType = type.type.GetGenericTypeDefinition().MakeGenericType(genericArgs);
            constructedTypeCache[key] = constructedType;
            return constructedType;
        }

        private Type ConstructArrayType()
        {
            var elementType = GetArrayBaseType(type.type);
            if (elementType.IsGenericType)
            {
                var genericArgs = nestedParameters.Select(tp => tp.ConstructType()).ToArray();
                ValidateGenericArguments(genericArgs);
                var constructedBase = elementType.GetGenericTypeDefinition().MakeGenericType(genericArgs);
                return ReconstructArrayType(constructedBase, RuntimeTypeUtility.GetArrayDepth(type.type));
            }
            return type.type;
        }

        private void ValidateGenericArguments(Type[] arguments)
        {
            if (arguments.Any(arg => arg == null))
            {
                throw new ArgumentException("Generic argument cannot be null");
            }
        }

        private static Type GetArrayBaseType(Type arrayType)
        {
            if (arrayBaseTypeCache.TryGetValue(arrayType, out var cachedBase))
            {
                return cachedBase;
            }

            var baseType = arrayType;
            while (baseType.IsArray)
            {
                baseType = baseType.GetElementType();
            }
            arrayBaseTypeCache[arrayType] = baseType;
            return baseType;
        }

        private static Type ReconstructArrayType(Type baseType, int rank)
        {
            var result = baseType;
            for (int i = 0; i < rank; i++)
            {
                result = result.MakeArrayType();
            }
            return result;
        }

        public void AddGenericParameters(Type type, Action<GenericParameter> foreachParameter = null)
        {
            try
            {
                if (type.IsGenericType)
                {
                    AddGenericTypeParameters(type, foreachParameter);
                }
                else if (type.IsArray)
                {
                    AddArrayTypeParameters(type, foreachParameter);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to add generic parameters: {ex.Message}");
            }
        }

        private void AddGenericTypeParameters(Type type, Action<GenericParameter> foreachParameter)
        {
            var genericArgs = type.GetGenericArguments();
            for (int i = 0; i < genericArgs.Length; i++)
            {
                var arg = genericArgs[i];
                var parameter = Create(
                    type.GetGenericTypeDefinition().GetGenericArguments()[i],
                    arg.As().CSharpName(false, false, false)
                );
                parameter.parent = this;
                foreachParameter?.Invoke(parameter);
                parameter.type.type = arg;
                nestedParameters.Add(parameter);
                parameter.AddGenericParameters(arg, foreachParameter);
            }
        }

        private void AddArrayTypeParameters(Type type, Action<GenericParameter> foreachParameter)
        {
            var baseType = GetArrayBaseType(type);
            if (baseType.IsGenericType)
            {
                AddGenericTypeParameters(baseType, foreachParameter);

                if (!type.IsArray)
                {
                    this.type.type = ReconstructArrayType(ConstructType(), RuntimeTypeUtility.GetArrayDepth(type));
                }
                else
                {
                    this.type.type = ConstructType();
                }
            }
        }
    }
}