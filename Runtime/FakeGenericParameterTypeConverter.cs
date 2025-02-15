using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// Allow serialization for fake generic parameter Type
    /// </summary>
    public class FakeGenericParameterTypeConverter : fsDirectConverter
    {
        public override Type ModelType => typeof(FakeGenericParameterType);
        public override object CreateInstance(fsData data, Type storageType)
        {
            object result = null;
            TryDeserialize(data, ref result, storageType);
            return result;
        }
        public override bool RequestCycleSupport(Type storageType)
        {
            return false;
        }
        public override bool RequestInheritanceSupport(Type storageType)
        {
            return false;
        }
        public override fsResult TrySerialize(object instance, out fsData serialized, Type storageType)
        {
            if (instance is not FakeGenericParameterType fakeGenericParameterType)
            {
                // Still allow serialization if its not a fake type
                if (instance is Type type)
                {
                    if (type.IsGenericType)
                    {
                        Serializer.TrySerialize(type.GetGenericTypeDefinition(), out var typeName);
                        var data = new Dictionary<string, fsData>
                        {
                            { "TypeName", typeName }
                        };
                        var genericArguments = type.GetGenericArguments();
                        var serializedArgs = new List<fsData>();

                        foreach (var arg in genericArguments)
                        {
                            var result = TrySerialize(arg, out fsData argData, storageType);
                            if (result.Failed)
                            {
                                serialized = default;
                                return result;
                            }
                            serializedArgs.Add(argData);
                        }
                        data["GenericArguments"] = new fsData(serializedArgs);
                        serialized = new fsData(data);
                        return fsResult.Success;
                    }
                    else if (type.IsArray)
                    {
                        var arrayBase = RuntimeTypeUtility.GetArrayBase(type);
                        TrySerialize(arrayBase, out var serializedArrayBase, arrayBase);
                        var serializedArrayData = new Dictionary<string, fsData>
                        {
                            { "ArrayBase", serializedArrayBase },
                            { "ArrayDepth", new fsData(RuntimeTypeUtility.GetArrayDepth(type)) }
                        };
                        serialized = new fsData(serializedArrayData);
                        return fsResult.Success;
                    }
                    Serializer.TrySerialize(type, out serialized);
                    return fsResult.Success;
                }

                serialized = default;
                return fsResult.Fail("Instance is not " + typeof(FakeGenericParameterType) + " or Type");
            }
            Serializer.TrySerialize(fakeGenericParameterType.Constraints, out var typeConstraints);
            Serializer.TrySerialize(fakeGenericParameterType.BaseType, out var baseTypeConstraint);
            Serializer.TrySerialize(fakeGenericParameterType.InterfaceConstraints.ToList(), out var interfaceConstraints);
            var fakeData = new Dictionary<string, fsData>
            {
                { "Name", new fsData(GetArrayBase(fakeGenericParameterType).Name) },
                { "TypeParameterConstraints", typeConstraints },
                { "BaseTypeConstraint", baseTypeConstraint },
                { "InterfaceConstraints", interfaceConstraints },
                { "Position", new fsData(fakeGenericParameterType._position) },
                { "IsArray", new fsData(fakeGenericParameterType._isArrayType) },
                { "ArrayDepth", new fsData(fakeGenericParameterType._isArrayType ? CountArrayDepth(fakeGenericParameterType) : 0) }
            };

            serialized = new fsData(fakeData);
            return fsResult.Success;
        }

        private long CountArrayDepth(FakeGenericParameterType value)
        {
            var depth = 0;
            while (value._isArrayType)
            {
                value = (FakeGenericParameterType)value.GetElementType();
                depth++;
            }
            return depth;
        }

        private FakeGenericParameterType GetArrayBase(FakeGenericParameterType value)
        {
            while (value._isArrayType)
            {
                value = (FakeGenericParameterType)value.GetElementType();
            }
            return value;
        }

        public override fsResult TryDeserialize(fsData data, ref object instance, Type storageType)
        {
            if (!data.IsDictionary)
            {
                if (data.IsList)
                {
                    var listData = data.AsList;
                    var genericArguments = new List<Type>();

                    foreach (var item in listData)
                    {
                        object argInstance = null;
                        var result = TryDeserialize(item, ref argInstance, typeof(Type));
                        if (result.Failed) return result;
                        if (argInstance == null)
                        {
                            return fsResult.Fail("Failed to deserialize generic argument");
                        }
                        genericArguments.Add(argInstance as Type);
                    }

                    instance = genericArguments;
                    return fsResult.Success;
                }
                else if (data.IsString)
                {
                    try
                    {
                        Type type = null;
                        var result = Serializer.TryDeserialize(data, ref type);
                        if (result.Failed || type == null)
                        {
                            Debug.LogError($"Failed to deserialize type from string: {data.AsString}");
                            instance = typeof(object);
                            return fsResult.Success;
                        }
                        instance = type;
                        return fsResult.Success;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Exception deserializing type: {ex}");
                        instance = typeof(object);
                        return fsResult.Success;
                    }
                }
                return fsResult.Fail("Expected dictionary or Type in " + data);
            }

            var serializedData = data.AsDictionary;

            // Handle FakeGenericParameterType
            if (serializedData.ContainsKey("TypeParameterConstraints"))
            {
                var name = serializedData["Name"].AsString;
                TypeParameterConstraints typeParameterConstraint = TypeParameterConstraints.None;
                Type baseTypeConstraint = typeof(object);
                List<Type> interfaceConstraints = new List<Type>();
                Serializer.TryDeserialize(serializedData["TypeParameterConstraints"], ref typeParameterConstraint);
                Serializer.TryDeserialize(serializedData["BaseTypeConstraint"], ref baseTypeConstraint);
                Serializer.TryDeserialize(serializedData["InterfaceConstraints"], ref interfaceConstraints);

                var position = (int)serializedData["Position"].AsInt64;
                var value = new FakeGenericParameterType(name, position, typeParameterConstraint, baseTypeConstraint, interfaceConstraints);

                if (serializedData.ContainsKey("IsArray") && serializedData["IsArray"].IsBool)
                {
                    if (serializedData["IsArray"].AsBool)
                    {
                        for (long i = 0; i < serializedData["ArrayDepth"].AsInt64; i++)
                        {
                            value = (FakeGenericParameterType)value.MakeArrayType();
                        }
                    }
                }

                instance = value;
                return fsResult.Success;
            }

            // Handle array types
            if (serializedData.ContainsKey("ArrayBase"))
            {
                var arrayBase = serializedData["ArrayBase"];
                object arrayType = typeof(object);
                TryDeserialize(arrayBase, ref arrayType, typeof(Type));
                for (long i = 0; i < serializedData["ArrayDepth"].AsInt64; i++)
                {
                    arrayType = (arrayType as Type).MakeArrayType();
                }
                instance = arrayType;
                return fsResult.Success;
            }

            try
            {
                if (serializedData.ContainsKey("TypeName"))
                {
                    Type genericDefinition = null;
                    var deserializationResult = Serializer.TryDeserialize(serializedData["TypeName"], ref genericDefinition);

                    if (deserializationResult.Failed || genericDefinition == null)
                    {
                        Debug.LogError("Failed to deserialize generic type definition");
                        instance = typeof(object);
                        return fsResult.Success;
                    }

                    if (serializedData.ContainsKey("GenericArguments"))
                    {
                        var genericArgsData = serializedData["GenericArguments"];
                        object genericArgs = null;
                        var result = TryDeserialize(genericArgsData, ref genericArgs, typeof(List<Type>));

                        if (result.Failed || genericArgs is not List<Type> genericArgsList || !genericArgsList.Any())
                        {
                            Debug.LogError("Failed to deserialize generic arguments");
                            instance = typeof(object);
                            return fsResult.Success;
                        }

                        try
                        {
                            if (genericArgsList.Any(arg => arg == null))
                            {
                                Debug.LogError("One or more generic arguments are null");
                                instance = typeof(object);
                                return fsResult.Success;
                            }

                            instance = genericDefinition.MakeGenericType(genericArgsList.ToArray());
                            return fsResult.Success;
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error creating generic type: {ex}");
                            instance = typeof(object);
                            return fsResult.Success;
                        }
                    }

                    instance = genericDefinition;
                    return fsResult.Success;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Critical error in deserialization: {ex}");
                instance = typeof(object);
                return fsResult.Success;
            }

            return fsResult.Fail("Invalid type data");
        }
    }
}