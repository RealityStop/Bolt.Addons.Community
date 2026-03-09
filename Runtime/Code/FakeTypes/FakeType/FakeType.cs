// using System;
// using System.Collections.Generic;
// using System.Globalization;
// using System.Linq;
// using System.Reflection;
// using Unity.VisualScripting.FullSerializer;
// using UnityEngine;

// namespace Unity.VisualScripting.Community
// {
//     [Serializable]
//     [fsObject(Converter = typeof(FakeTypeConverter))]
//     [TypeIcon(typeof(Type))]
//     public class FakeType : Type
//     {
//         private string name;
//         private string @namespace;
//         private readonly List<Type> interfaces = new List<Type>();
//         public readonly CodeAsset container;

//         private readonly bool isArrayType;
//         private int arrayRank;
//         private Type elementType;

//         internal FakeType(CodeAsset container, string name, string @namespace, List<Type> interfaces = null)
//         {
//             this.container = container;
//             this.name = name;
//             this.@namespace = @namespace;
//             this.interfaces = interfaces ?? new List<Type>();
//         }

//         private FakeType(FakeType elementType, int rank)
//         {
//             isArrayType = true;
//             arrayRank = rank;
//             this.elementType = elementType;
//         }

//         #region Update Methods
//         public void UpdateName(string name) => this.name = name;
//         public void UpdateNamespace(string @namespace) => this.@namespace = @namespace;
//         #endregion

//         #region Array Support Overrides
//         public override Type GetElementType() => isArrayType ? elementType : throw new InvalidOperationException();
//         public override Type MakeArrayType() => new FakeType(this, 1);
//         public override Type MakeArrayType(int rank) => new FakeType(this, rank);
//         public override int GetArrayRank() => isArrayType ? arrayRank : 0;
//         protected override bool IsArrayImpl() => isArrayType;
//         protected override bool HasElementTypeImpl() => isArrayType;
//         #endregion

//         #region Base Type Resolution
//         private Type GetBaseType()
//         {
//             if (isArrayType)
//                 return typeof(Array);

//             if (container is ClassAsset classAsset && classAsset.inheritsType)
//                 return classAsset.GetInheritedType();
//             else if (container is StructAsset)
//                 return typeof(ValueType);
//             else if (container is EnumAsset)
//                 return typeof(Enum);
//             else if (container is InterfaceAsset)
//                 return null;

//             return typeof(object);
//         }

//         public override Type BaseType => GetBaseType();
//         #endregion

//         #region Metadata
//         public override string Name => isArrayType
//             ? $"{elementType.Name}[{new string(',', arrayRank - 1)}]"
//             : name;

//         public override string Namespace => @namespace;
//         public override string FullName => isArrayType
//             ? $"{elementType.FullName}[{new string(',', arrayRank - 1)}]"
//             : $"{@namespace}{(!string.IsNullOrEmpty(@namespace) ? "." : "")}{name}";
//         public override string AssemblyQualifiedName => FullName;
//         public override Assembly Assembly => BaseType?.Assembly ?? typeof(object).Assembly;
//         public override Module Module => BaseType?.Module ?? typeof(object).Module;
//         public override Guid GUID => Guid.NewGuid();
//         public override Type UnderlyingSystemType => BaseType;
//         #endregion

//         #region Reflection Info Simulation
//         public override Type[] GetInterfaces()
//         {
//             try
//             {
//                 return interfaces.SelectMany(i => RuntimeTypeUtility.GetAllInterfacesRecursive(i))
//                     .Concat(BaseType != null ? BaseType.GetInterfaces() : Array.Empty<Type>())
//                     .ToArray();
//             }
//             catch
//             {
//                 return interfaces.ToArray();
//             }
//         }

//         public override bool IsGenericParameter => false;
//         public override bool IsConstructedGenericType => false;
//         public override Type[] GetGenericArguments() => Array.Empty<Type>();

//         public override MethodInfo[] GetMethods(BindingFlags flags)
//         {
//             var results = new List<MethodInfo>();

//             // Local methods from the container (class or struct)
//             if (container is ClassAsset classAsset)
//             {
//                 foreach (var m in classAsset.methods)
//                     results.Add(new FakeMethodInfo(this, m));
//             }
//             else if (container is StructAsset structAsset)
//             {
//                 foreach (var m in structAsset.methods)
//                     results.Add(new FakeMethodInfo(this, m));
//             }

//             // Base type members
//             var baseType = BaseType;
//             try
//             {
//                 if (baseType != null)
//                 {
//                     foreach (var m in baseType.GetMethods(flags))
//                     {
//                         if (m.IsAssembly || m.IsPrivate || m.DeclaringType == typeof(void)) continue;

//                         bool exists = results.Any(f =>
//                             f.Name == m.Name &&
//                             f.GetParameters().Select(p => p.ParameterType)
//                             .SequenceEqual(m.GetParameters().Select(p => p.ParameterType)));

//                         if (!exists)
//                             results.Add(m);
//                     }
//                 }
//             }
//             catch (Exception ex) when (ex is NotSupportedException || ex is ReflectionTypeLoadException)
//             {
//                 Debug.LogWarning($"Failed to fetch base methods on {baseType?.Name}: {ex.Message}");
//             }

//             // Interface members
//             foreach (var iface in GetInterfaces())
//             {
//                 try
//                 {
//                     foreach (var m in iface.GetMethods(flags))
//                     {
//                         if (m.IsAssembly || m.IsPrivate || m.DeclaringType == typeof(void)) continue;

//                         bool exists = results.Any(f =>
//                             f.Name == m.Name &&
//                             f.GetParameters().Select(p => p.ParameterType)
//                             .SequenceEqual(m.GetParameters().Select(p => p.ParameterType)));

//                         if (!exists)
//                             results.Add(m);
//                     }
//                 }
//                 catch { }
//             }

//             // --- NEW SECTION: EXTENSION METHODS ---
//             try
//             {
//                 var assemblies = AppDomain.CurrentDomain.GetAssemblies();
//                 foreach (var asm in assemblies)
//                 {
//                     Type[] types;
//                     try { types = asm.GetTypes(); } catch { continue; }

//                     foreach (var type in types)
//                     {
//                         if (!type.IsSealed || !type.IsAbstract) continue; // static class
//                         foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
//                         {
//                             if (!method.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false))
//                                 continue;
//                             var parameters = method.GetParameters();
//                             if (parameters.Length == 0) continue;

//                             if (parameters[0].ParameterType.IsAssignableFrom(baseType))
//                             {
//                                 Debug.Log(method);

//                                 results.Add(method);
//                             }
//                         }
//                     }
//                 }
//             }
//             catch (Exception ex)
//             {
//                 Debug.LogWarning($"Failed to include extension methods for {Name}: {ex.Message}");
//             }

//             return results.ToArray();
//         }

//         public override ConstructorInfo[] GetConstructors(BindingFlags flags)
//         {
//             var results = new List<ConstructorInfo>();

//             if (container is ClassAsset classAsset)
//             {
//                 foreach (var c in classAsset.constructors)
//                     results.Add(new FakeConstructorInfo(this, c));
//             }
//             else if (container is StructAsset structAsset)
//             {
//                 foreach (var c in structAsset.constructors)
//                     results.Add(new FakeConstructorInfo(this, c));
//             }

//             var baseType = BaseType;
//             try
//             {
//                 if (baseType != null)
//                 {
//                     foreach (var c in baseType.GetConstructors(flags))
//                     {
//                         if (c.IsAssembly || c.IsPrivate || c.DeclaringType == typeof(void)) continue;
//                         bool exists = results.Any(fc =>
//                             fc.GetParameters().Select(p => p.ParameterType)
//                             .SequenceEqual(c.GetParameters().Select(p => p.ParameterType)));

//                         if (!exists)
//                             results.Add(c);
//                     }
//                 }
//             }
//             catch (Exception ex) when (ex is NotSupportedException || ex is ReflectionTypeLoadException)
//             {
//                 Debug.LogWarning($"Failed to fetch constructors on {baseType?.Name}: {ex.Message}");
//             }

//             return results.ToArray();
//         }

//         public override FieldInfo[] GetFields(BindingFlags flags)
//         {
//             var results = new List<FieldInfo>();

//             if (container is ClassAsset classAsset)
//             {
//                 results.AddRange(classAsset.variables
//                     .Where(f => !f.isProperty)
//                     .Select(f => new FakeFieldInfo(this, f)));
//             }
//             else if (container is StructAsset structAsset)
//             {
//                 results.AddRange(structAsset.variables
//                     .Where(f => !f.isProperty)
//                     .Select(f => new FakeFieldInfo(this, f)));
//             }

//             var baseType = BaseType;
//             try
//             {
//                 if (baseType != null)
//                 {
//                     foreach (var f in baseType.GetFields(flags))
//                     {
//                         if (f.IsAssembly || f.IsPrivate || f.DeclaringType == typeof(void)) continue;
//                         if (!results.Any(ff => ff.Name == f.Name))
//                             results.Add(f);
//                     }
//                 }
//             }
//             catch (Exception ex) when (ex is NotSupportedException || ex is ReflectionTypeLoadException)
//             {
//                 Debug.LogWarning($"Failed to fetch fields on {baseType?.Name}: {ex.Message}");
//             }

//             foreach (var iface in GetInterfaces())
//             {
//                 try
//                 {
//                     foreach (var f in iface.GetFields(flags))
//                     {
//                         if (f.IsAssembly || f.IsPrivate || f.DeclaringType == typeof(void)) continue;
//                         if (!results.Any(ff => ff.Name == f.Name))
//                             results.Add(f);
//                     }
//                 }
//                 catch { }
//             }

//             return results.ToArray();
//         }

//         public override PropertyInfo[] GetProperties(BindingFlags flags)
//         {
//             var results = new List<PropertyInfo>();

//             if (container is ClassAsset classAsset)
//             {
//                 results.AddRange(classAsset.variables
//                     .Where(f => f.isProperty)
//                     .Select(f => new FakePropertyInfo(this, f)));
//             }
//             else if (container is StructAsset structAsset)
//             {
//                 results.AddRange(structAsset.variables
//                     .Where(f => f.isProperty)
//                     .Select(f => new FakePropertyInfo(this, f)));
//             }

//             var baseType = BaseType;
//             try
//             {
//                 if (baseType != null)
//                 {
//                     foreach (var p in baseType.GetProperties(flags))
//                     {
//                         if (p.DeclaringType == typeof(void)) continue;
//                         if (!results.Any(fp => fp.Name == p.Name))
//                             results.Add(p);
//                     }
//                 }
//             }
//             catch (Exception ex) when (ex is NotSupportedException || ex is ReflectionTypeLoadException)
//             {
//                 Debug.LogWarning($"Failed to fetch properties on {baseType?.Name}: {ex.Message}");
//             }

//             foreach (var iface in GetInterfaces())
//             {
//                 try
//                 {
//                     foreach (var p in iface.GetProperties(flags))
//                     {
//                         if (p.DeclaringType == typeof(void)) continue;
//                         if (!results.Any(fp => fp.Name == p.Name))
//                             results.Add(p);
//                     }
//                 }
//                 catch { }
//             }

//             return results.ToArray();
//         }

//         public override MemberInfo[] GetMembers(BindingFlags flags)
//         {
//             var list = new List<MemberInfo>();
//             list.AddRange(GetFields(flags));
//             list.AddRange(GetProperties(flags));
//             list.AddRange(GetMethods(flags));
//             list.AddRange(GetConstructors(flags));
//             return list.ToArray();
//         }

//         public override EventInfo[] GetEvents(BindingFlags bindingAttr) => Array.Empty<EventInfo>();
//         #endregion

//         #region Type Implementation Flags
//         protected override bool IsByRefImpl() => false;
//         protected override bool IsPointerImpl() => false;
//         protected override bool IsPrimitiveImpl() => false;
//         protected override bool IsCOMObjectImpl() => false;
//         #endregion

//         #region Required Overrides
//         protected override TypeAttributes GetAttributeFlagsImpl()
//         {
//             return TypeAttributes.Public | (container is InterfaceAsset ? TypeAttributes.Interface : TypeAttributes.Class);
//         }

//         protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention,
//             Type[] types, ParameterModifier[] modifiers)
//         {
//             var results = GetConstructors(bindingAttr);
//             return results.FirstOrDefault(c =>
//                 c.GetParameters().Select(p => p.ParameterType).SequenceEqual(types));
//         }

//         public override IEnumerable<CustomAttributeData> CustomAttributes => Enumerable.Empty<CustomAttributeData>();
//         public override IList<CustomAttributeData> GetCustomAttributesData() => new List<CustomAttributeData>();
//         protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention,
//             Type[] types, ParameterModifier[] modifiers) => throw new InvalidOperationException("FakeType does not support GetMethod.");
//         protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType,
//             Type[] types, ParameterModifier[] modifiers) => throw new InvalidOperationException("FakeType does not support GetProperty.");
//         public override Type GetNestedType(string name, BindingFlags bindingAttr) => null;
//         public override Type[] GetNestedTypes(BindingFlags bindingAttr) => Array.Empty<Type>();
//         public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target,
//             object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
//             => throw new InvalidOperationException("FakeType does not support InvokeMember.");
//         protected override bool IsValueTypeImpl() => container is StructAsset || BaseType == typeof(ValueType);
//         public override object[] GetCustomAttributes(bool inherit)
//         {
//             // Returning this because when trying to return baseType or interface types attributes its logging a warning not sure why.
//             return GetType().GetCustomAttributes(inherit);
//         }

//         public override object[] GetCustomAttributes(Type attributeType, bool inherit)
//         {
//             return GetType().GetCustomAttributes(attributeType, inherit);
//         }

//         public override bool IsDefined(Type attributeType, bool inherit) => false;
//         #endregion

//         public override string ToString() => FullName;

//         public override EventInfo GetEvent(string name, BindingFlags bindingAttr) => null;

//         public override FieldInfo GetField(string name, BindingFlags bindingAttr)
//         {
//             try
//             {
//                 if (container is ClassAsset classAsset)
//                 {
//                     var field = classAsset.variables
//                         .Where(f => !f.isProperty)
//                         .Select(f => new FakeFieldInfo(this, f))
//                         .FirstOrDefault(f => f.Name == name);

//                     if (field != null)
//                         return field;
//                 }
//                 else if (container is StructAsset structAsset)
//                 {
//                     var field = structAsset.variables
//                         .Where(f => !f.isProperty)
//                         .Select(f => new FakeFieldInfo(this, f))
//                         .FirstOrDefault(f => f.Name == name);

//                     if (field != null)
//                         return field;
//                 }

//                 var baseType = BaseType;
//                 if (baseType != null)
//                 {
//                     try
//                     {
//                         var field = baseType.GetField(name, bindingAttr);
//                         if (field != null)
//                             return field;
//                     }
//                     catch { }
//                 }

//                 foreach (var iface in GetInterfaces())
//                 {
//                     try
//                     {
//                         var field = iface.GetField(name, bindingAttr);
//                         if (field != null)
//                             return field;
//                     }
//                     catch { }
//                 }
//             }
//             catch { }

//             return null;
//         }

//         public override Type GetInterface(string name, bool ignoreCase) => null;
//     }
// }
