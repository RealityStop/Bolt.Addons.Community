// using System;
// using System.Collections.Generic;
// using System.Linq;
// using Unity.VisualScripting.FullSerializer;
// using UnityEngine;

// namespace Unity.VisualScripting.Community
// {
//     public class FakeTypeConverter : fsConverter
//     {
//         public override bool CanProcess(Type type)
//         {
//             return typeof(Type).IsAssignableFrom(type);
//         }

//         public override bool RequestCycleSupport(Type storageType) => false;
//         public override bool RequestInheritanceSupport(Type storageType) => true;

//         public override fsResult TrySerialize(object instance, out fsData serialized, Type storageType)
//         {
//             if (instance is FakeType fake)
//             {
//                 var dict = new Dictionary<string, fsData>();

//                 var baseType = RuntimeTypeUtility.GetArrayBase(fake);
//                 var depth = RuntimeTypeUtility.GetArrayDepth(fake);

//                 Serializer.TrySerialize(fake.GetInterfaces().ToList(), out var interfacesData);

//                 dict["Fake"] = new fsData(true);
//                 dict["Name"] = new fsData(baseType?.Name ?? fake.Name ?? string.Empty);
//                 dict["Namespace"] = new fsData(baseType?.Namespace ?? fake.Namespace ?? string.Empty);
//                 dict["Interfaces"] = interfacesData;
//                 dict["Depth"] = new fsData(depth);

//                 serialized = new fsData(dict);
//                 return fsResult.Success;
//             }

//             if (instance is Type realType)
//             {
//                 return Serializer.TrySerialize(realType, out serialized);
//             }

//             serialized = default;
//             return fsResult.Fail("Unsupported type: " + (instance?.GetType().ToString() ?? "<null>"));
//         }

//         public override fsResult TryDeserialize(fsData data, ref object instance, Type storageType)
//         {
//             if (data.IsDictionary && data.AsDictionary.ContainsKey("Fake"))
//             {
//                 var dict = data.AsDictionary;

//                 var name = dict.ContainsKey("Name") ? dict["Name"].AsString : string.Empty;
//                 var @namespace = dict.ContainsKey("Namespace") ? dict["Namespace"].AsString : string.Empty;
//                 var depth = dict.ContainsKey("Depth") ? (int)dict["Depth"].AsInt64 : 0;

//                 var interfaces = new List<Type>();
//                 if (dict.ContainsKey("Interfaces"))
//                 {
//                     Serializer.TryDeserialize(dict["Interfaces"], ref interfaces);
//                 }

//                 var type = new FakeType(null, name, @namespace, interfaces ?? new List<Type>());

//                 for (int i = 0; i < depth; i++)
//                 {
//                     type = (FakeType)type.MakeArrayType();
//                 }

//                 instance = type;
//                 return fsResult.Success;
//             }

//             Type t = null;
//             var result = Serializer.TryDeserialize(data, ref t);
//             instance = t;
//             return result;
//         }
//     }
// }