using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class GenericParameter : ISerializationCallbackReceiver
    {
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
        public Type[] constraints;
        public int specialType;
        public int previousSpecialType;
#if UNITY_EDITOR
        public bool isOpen;
#endif
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
        }

        public void OnBeforeSerialize()
        {
            nestedParametersSerialization = nestedParameters.Serialize();
        }

        public Type ConstructType()
        {
            if (type.type.IsGenericType)
            {
                try
                {
                    var genericArguments = nestedParameters.Select(tp =>
                    {
                        if (tp.selectedType.type == null)
                        {
                            throw new ArgumentException($"Type argument for {tp.name} is null");
                        }
                        return tp.ConstructType();
                    }).ToArray();

                    var constructedType = type.type.GetGenericTypeDefinition().MakeGenericType(genericArguments);
                    return constructedType;
                }
                catch (ArgumentException ex)
                {
                    Debug.LogError($"Failed to construct generic type: {ex.Message}, Type: {type.type.FullName}");
                    return type.type;
                }
            }
            else if (type.type.IsArray)
            {
                var tempType = type.type.GetElementType();
                var ArrayCount = 1;
                var amountToGoBack = 0;
                while (tempType.IsArray)
                {
                    tempType = tempType.GetElementType();
                    ArrayCount++;
                    amountToGoBack++;
                }

                if (tempType.IsGenericType)
                {
                    try
                    {
                        GenericParameter _target = this;
                        for (int i = 0; i < amountToGoBack; i++)
                        {
                            if (_target.parent != null)
                            {
                                _target = _target.parent;
                            }
                        }

                        var genericArguments = _target.nestedParameters.Select(tp =>
                        {
                            if (tp.selectedType.type == null)
                            {
                                throw new ArgumentException($"Type argument for {tp.name} is null");
                            }
                            return tp.ConstructType();
                        }).ToArray();

                        var constructedType = tempType.GetGenericTypeDefinition().MakeGenericType(genericArguments);
                        for (int i = 0; i < ArrayCount; i++)
                        {
                            constructedType = constructedType.MakeArrayType();
                        }
                        return constructedType;
                    }
                    catch (ArgumentException ex)
                    {
                        Debug.LogError($"Failed to construct generic array type: {ex.Message}");
                        return type.type;
                    }
                }
            }

            return type.type;
        }

        public void AddGenericParameters(Type type, Action<GenericParameter> foreachParameter = null)
        {
            if (type.IsGenericType)
            {
                var genericArguments = type.GetGenericArguments();
                var index = 0;
                foreach (var arg in genericArguments)
                {
                    var parameter = new GenericParameter(this, type.GetGenericTypeDefinition().GetGenericArguments()[index], arg.As().CSharpName(false, false, false));
                    foreachParameter?.Invoke(parameter);
                    parameter.type.type = arg;
                    nestedParameters.Add(parameter);
                    parameter.AddGenericParameters(arg, foreachParameter);
                    index++;
                }
            }
            else if (type.IsArray)
            {
                var tempType = type.GetElementType();
                var ArrayCount = 1;
                while (tempType.IsArray)
                {
                    tempType = tempType.GetElementType();
                    ArrayCount++;
                }
                if (tempType.IsGenericType)
                {
                    var genericArguments = tempType.GetGenericArguments();
                    var index = 0;
                    foreach (var arg in genericArguments)
                    {
                        var parameter = new GenericParameter(this, tempType.GetGenericTypeDefinition().GetGenericArguments()[index], arg.As().CSharpName(false, false, false));
                        foreachParameter?.Invoke(parameter);
                        parameter.type.type = arg;
                        nestedParameters.Add(parameter);
                        parameter.AddGenericParameters(arg, foreachParameter);
                        index++;
                    }
                    var constructedType = ConstructType();
                    for (int i = 0; i < ArrayCount; i++)
                    {
                        this.type.type = constructedType.MakeArrayType();
                    }
                }
            }
        }
    }
}