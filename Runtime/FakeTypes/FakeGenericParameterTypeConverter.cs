using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class FakeGenericParameterTypeConverter : fsConverter
    {
        public override bool CanProcess(Type type)
        {
            return typeof(Type).IsAssignableFrom(type);
        }

        public override bool RequestCycleSupport(Type storageType) => false;
        public override bool RequestInheritanceSupport(Type storageType) => true;

        public override fsResult TrySerialize(object instance, out fsData serialized, Type storageType)
        {
            if (instance is FakeGenericParameterType fake)
            {
                var dict = new Dictionary<string, fsData>();
                Serializer.TrySerialize(fake.Constraints, out var constraints);
                Serializer.TrySerialize(fake.BaseType, out var baseType);
                Serializer.TrySerialize(fake.InterfaceConstraints.ToList(), out var interfaces);
                Serializer.TrySerialize(fake.container, out var container);
                dict["Fake"] = new fsData(true);
                dict["Name"] = new fsData(RuntimeTypeUtility.GetArrayBase(fake).Name);
                dict["Position"] = new fsData(fake.GenericParameterPosition);
                dict["Constraints"] = constraints;
                dict["BaseType"] = baseType;
                dict["Interfaces"] = interfaces;
                dict["Depth"] = new fsData(RuntimeTypeUtility.GetArrayDepth(fake));
                dict["Container"] = container;

                serialized = new fsData(dict);
                return fsResult.Success;
            }
            // else if (instance is FakeType fakeType)
            // {
            //     return Serializer.TrySerialize(fakeType, out serialized);
            // }
            else if (instance is Type realType)
            {
                return Serializer.TrySerialize(realType, out serialized);
            }

            serialized = default;
            return fsResult.Fail("Unsupported type: " + instance?.GetType());
        }

        public override fsResult TryDeserialize(fsData data, ref object instance, Type storageType)
        {
            if (data.IsDictionary && data.AsDictionary.ContainsKey("Fake"))
            {
                var dict = data.AsDictionary;
                var name = dict["Name"].AsString;
                var position = (int)dict["Position"].AsInt64;

                TypeParameterConstraints constraints = TypeParameterConstraints.None;
                Type baseType = typeof(object);
                List<Type> interfaces = new();
                IGenericContainer container = null;

                Serializer.TryDeserialize(dict["Constraints"], ref constraints);
                Serializer.TryDeserialize(dict["BaseType"], ref baseType);
                Serializer.TryDeserialize(dict["Interfaces"], ref interfaces);
                Serializer.TryDeserialize(dict["Container"], ref container);
                var type = FakeTypeRegistry.GetOrCreate(container, position, name, constraints, baseType, interfaces);
                if (dict.ContainsKey("Depth"))
                {
                    for (int i = 0; i < (int)dict["Depth"].AsInt64; i++)
                    {
                        type = (FakeGenericParameterType)type.MakeArrayType();
                    }
                }
                instance = type;
                return fsResult.Success;
            }
            // else if (instance is FakeType fakeType)
            // {
            //     var result = Serializer.TryDeserialize(data, ref fakeType);
            //     instance = fakeType;
            //     return result;
            // }
            else
            {
                Type type = null;
                var result = Serializer.TryDeserialize(data, ref type);
                instance = type;
                return result;
            }
        }
    }
}