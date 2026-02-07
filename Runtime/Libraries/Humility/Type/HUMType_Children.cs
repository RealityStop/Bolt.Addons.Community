using Unity.VisualScripting.Community.Libraries.CSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Reflection;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMType_Children
    {
        /// <summary>
        /// Begins a type filtering operation.
        /// </summary>
        public static HUMType.Data.With With(this HUMType.Data.Types types)
        {
            return new HUMType.Data.With(types);
        }

        /// <summary>
        /// Begins a type string operation.
        /// </summary>
        public static HUMType.Data.With With(this string str)
        {
            return new HUMType.Data.With(str);
        }

        public static string StringBefore(this HUMType.Data.With with, string @string)
        {
            if (with.value is string str)
            {
                return string.IsNullOrEmpty(@string) ? str : @string + str;
            }
            var _str = with.value.ToString();
            return string.IsNullOrEmpty(@string) ? _str : @string + _str;
        }

        public static string StringAfter(this HUMType.Data.With with, string @string)
        {
            if (with.value is string str)
            {
                return string.IsNullOrEmpty(@string) ? str : str + @string;
            }
            var _str = with.value.ToString();
            return string.IsNullOrEmpty(@string) ? _str : _str + @string;
        }

        public static T New<T>(this HUMType.Data.As @as)
        {
            return (T)HUMType.New(@as.type);
        }

        /// <summary>
        /// Converts a type to its actual declared csharp name. Custom types return the type name. Example 'System.Int32' becomes 'int'.
        /// </summary>
        public static string CSharpName(this HUMType.Data.As @as, bool hideSystemObject = false, bool fullName = false, bool highlight = true)
        {
            if (highlight)
                return @as.HighlightCSharpName(hideSystemObject, fullName);

            var type = @as.type;
            if (type == null)
                return "null";

            fullName = fullName && !string.IsNullOrEmpty(type.Namespace);

            if (type.Is().Void())
                return "void";

            if (type.IsConstructedGenericType || type.IsGenericType)
                return GenericDeclaration(type, fullName, false);

            if (type == typeof(int)) return "int";
            if (type == typeof(string)) return "string";
            if (type == typeof(float)) return "float";
            if (type == typeof(double)) return "double";
            if (type == typeof(bool)) return "bool";
            if (type == typeof(byte)) return "byte";
            if (type == typeof(object) && type.BaseType == null)
                return hideSystemObject ? string.Empty : "object";
            if (type == typeof(object[]))
                return "object[]";

            if (type is FakeGenericParameterType fake)
                return fake.Name;

            if (type.Name.EndsWith("Attribute", StringComparison.Ordinal))
            {
                var name = type.Name.Substring(0, type.Name.Length - 9);

                return fullName && !string.IsNullOrEmpty(type.Namespace)
                    ? $"{type.Namespace}.{name}"
                    : name;
            }

            if (type.IsArray)
            {
                var arrayDepth = 0;
                while (type.IsArray)
                {
                    arrayDepth++;
                    type = type.GetElementType();
                }

                var elementName = type.As().CSharpName(hideSystemObject, fullName, false);
                return elementName + new string('[', arrayDepth).Replace("[", "[]");
            }

            if (string.IsNullOrEmpty(type.Name))
                return "UnknownType".ErrorHighlight();
            if (string.IsNullOrEmpty(type.Namespace))
                return type.CSharpName();

            return fullName ? type.CSharpFullName() : type.CSharpName();
        }

        private static string HighlightCSharpName(this HUMType.Data.As @as, bool hideSystemObject = false, bool fullName = false)
        {
            var type = @as.type;
            if (type == null)
                return "null".ConstructHighlight();

            fullName = fullName && !string.IsNullOrEmpty(type.Namespace);

            if (type.Is().Void())
                return "void".ConstructHighlight();

            if (type.IsConstructedGenericType || type.IsGenericType)
                return GenericDeclaration(type, fullName, true);

            if (type == typeof(int)) return "int".ConstructHighlight();
            if (type == typeof(string)) return "string".ConstructHighlight();
            if (type == typeof(float)) return "float".ConstructHighlight();
            if (type == typeof(double)) return "double".ConstructHighlight();
            if (type == typeof(bool)) return "bool".ConstructHighlight();
            if (type == typeof(byte)) return "byte".ConstructHighlight();
            if (type == typeof(object) && type.BaseType == null)
                return hideSystemObject ? string.Empty : "object".ConstructHighlight();
            if (type == typeof(object[]))
                return "object".ConstructHighlight() + "[]";

            if (type is FakeGenericParameterType fake)
            {
                if (fake.IsArray)
                {
                    var temp = type;
                    while (((FakeGenericParameterType)temp).IsArray)
                        temp = temp.GetElementType();

                    var coreName = ((FakeGenericParameterType)temp).Name.TypeHighlight();
                    return fake.Name.Replace(((FakeGenericParameterType)temp).Name, coreName);
                }

                return fake.Name.TypeHighlight();
            }

            if (type.Name.EndsWith("Attribute", StringComparison.Ordinal))
            {
                var coreName = type.Name.Substring(0, type.Name.Length - 9).TypeHighlight();
                if (fullName && !string.IsNullOrEmpty(type.Namespace))
                    return $"{type.Namespace.NamespaceHighlight()}.{coreName}";
                return coreName;
            }

            if (type.IsInterface)
            {
                var prefix = fullName && !string.IsNullOrEmpty(type.Namespace)
                    ? type.Namespace.NamespaceHighlight() + "."
                    : "";
                return prefix + type.Name.InterfaceHighlight();
            }

            if (type.IsArray)
            {
                var arrayDepth = 0;
                while (type.IsArray)
                {
                    arrayDepth++;
                    type = type.GetElementType();
                }

                var elementName = type.As().CSharpName(hideSystemObject, fullName, true);
                return elementName + new string('[', arrayDepth).Replace("[", "[]");
            }

            if (string.IsNullOrEmpty(type.Name))
                return "UnknownType".ErrorHighlight();
            if (fullName && string.IsNullOrEmpty(type.Namespace))
                return type.CSharpName(true, true);

            return fullName ? type.CSharpFullName(true, true) : type.CSharpName(true, true);
        }

        public static string CSharpName(this HUMType.Data.As @as, bool hideSystemObject = false, Func<Type, bool> useFullName = null, bool highlight = true)
        {
            if (highlight)
                return @as.HighlightCSharpName(hideSystemObject, useFullName);

            var type = @as.type;

            if (type == null)
                return "null";

            if (type.Is().Void())
                return "void";

            if (type.IsConstructedGenericType || type.IsGenericType)
                return GenericDeclaration(type, useFullName, false);

            if (type == typeof(int)) return "int";
            if (type == typeof(string)) return "string";
            if (type == typeof(float)) return "float";
            if (type == typeof(double)) return "double";
            if (type == typeof(bool)) return "bool";
            if (type == typeof(byte)) return "byte";
            if (type == typeof(object) && type.BaseType == null)
                return hideSystemObject ? string.Empty : "object";
            if (type == typeof(object[]))
                return "object[]";

            if (type is FakeGenericParameterType fake)
                return fake.Name;

            if (type.Name.EndsWith("Attribute", StringComparison.Ordinal))
            {
                var name = type.Name.Substring(0, type.Name.Length - 9);

                bool fullName = useFullName != null && useFullName(type);
                
                return fullName && !string.IsNullOrEmpty(type.Namespace)
                    ? $"{type.Namespace}.{name}"
                    : name;
            }

            if (type.IsArray)
            {
                var arrayDepth = 0;
                while (type.IsArray)
                {
                    arrayDepth++;
                    type = type.GetElementType();
                }

                var elementName = type.As().CSharpName(hideSystemObject, useFullName, false);
                return elementName + new string('[', arrayDepth).Replace("[", "[]");
            }

            if (string.IsNullOrEmpty(type.Name))
                return "UnknownType".ErrorHighlight();
            if (string.IsNullOrEmpty(type.Namespace))
                return type.CSharpName();

            return type.CSharpFullName(useFullName);
        }

        private static string HighlightCSharpName(this HUMType.Data.As @as, bool hideSystemObject = false, Func<Type, bool> useFullName = null)
        {
            var type = @as.type;
            if (type == null)
                return "null".ConstructHighlight();

            if (type.Is().Void())
                return "void".ConstructHighlight();

            if (type.IsConstructedGenericType || type.IsGenericType)
                return GenericDeclaration(type, useFullName, true);

            if (type == typeof(int)) return "int".ConstructHighlight();
            if (type == typeof(string)) return "string".ConstructHighlight();
            if (type == typeof(float)) return "float".ConstructHighlight();
            if (type == typeof(double)) return "double".ConstructHighlight();
            if (type == typeof(bool)) return "bool".ConstructHighlight();
            if (type == typeof(byte)) return "byte".ConstructHighlight();
            if (type == typeof(object) && type.BaseType == null)
                return hideSystemObject ? string.Empty : "object".ConstructHighlight();
            if (type == typeof(object[]))
                return "object".ConstructHighlight() + "[]";

            if (type is FakeGenericParameterType fake)
            {
                if (fake.IsArray)
                {
                    var temp = type;
                    while (((FakeGenericParameterType)temp).IsArray)
                        temp = temp.GetElementType();

                    var coreName = ((FakeGenericParameterType)temp).Name.TypeHighlight();
                    return fake.Name.Replace(((FakeGenericParameterType)temp).Name, coreName);
                }

                return fake.Name.TypeHighlight();
            }

            if (type.Name.EndsWith("Attribute", StringComparison.Ordinal))
            {
                bool fullName = useFullName != null && useFullName(type);
                var coreName = type.Name.Substring(0, type.Name.Length - 9).TypeHighlight();
                if (fullName && !string.IsNullOrEmpty(type.Namespace))
                    return $"{type.Namespace.NamespaceHighlight()}.{coreName}";
                return coreName;
            }

            if (type.IsInterface)
            {
                bool fullName = useFullName != null && useFullName(type);
                var prefix = fullName && !string.IsNullOrEmpty(type.Namespace)
                    ? type.Namespace.NamespaceHighlight() + "."
                    : "";
                return prefix + type.Name.InterfaceHighlight();
            }

            if (type.IsArray)
            {
                var arrayDepth = 0;
                while (type.IsArray)
                {
                    arrayDepth++;
                    type = type.GetElementType();
                }

                var elementName = type.As().CSharpName(hideSystemObject, useFullName, true);
                return elementName + new string('[', arrayDepth).Replace("[", "[]");
            }

            if (string.IsNullOrEmpty(type.Name))
                return "UnknownType".ErrorHighlight();

            if (string.IsNullOrEmpty(type.Namespace))
                return type.CSharpName(true, true);

            return type.CSharpFullName(useFullName, true, true);
        }

        public enum HighlightType
        {
            None,
            Enum,
            Interface,
            Type,
            Construct,
            Comment,
            Control,
            Variable,
            Error
        }

        public static string WithHighlight(this string text, HighlightType type)
        {
            switch (type)
            {
                case HighlightType.None:
                    return text;
                case HighlightType.Comment:
                    return text.CommentHighlight();
                case HighlightType.Construct:
                    return text.ConstructHighlight();
                case HighlightType.Interface:
                    return text.InterfaceHighlight();
                case HighlightType.Enum:
                    return text.EnumHighlight();
                case HighlightType.Type:
                    return text.TypeHighlight();
                case HighlightType.Variable:
                    return text.VariableHighlight();
                case HighlightType.Control:
                    return text.ControlHighlight();
                case HighlightType.Error:
                    return text.ErrorHighlight();
                default:
                    return text;
            }
        }

        /// <summary>
        /// Returns true if a type is equal to another type.
        /// </summary>
        public static bool Type<T>(this HUMType.Data.Is isData)
        {
            return isData.type == typeof(T);
        }

        /// <summary>
        /// Returns true if a type is equal to another type.
        /// </summary>
        public static bool Type(this HUMType.Data.Is isData, Type type)
        {
            return isData.type == type;
        }

        /// <summary>
        /// Returns true if the type is a void.
        /// </summary>
        public static bool Void(this HUMType.Data.Is isData)
        {
            return isData.type == typeof(void) || isData.type == typeof(CSharp.Void);
        }

        public static bool NullOrVoid(this HUMType.Data.Is isData)
        {
            return isData.type == typeof(void) || isData.type == null;
        }

        /// <summary>
        /// Returns true if the type is Inheritable.
        /// </summary>
        public static bool Inheritable(this HUMType.Data.Is isData)
        {
            var t = isData.type;

            if (t.Is().Void())
            {
                return false;
            }

            if (t.IsSealed) return false;

            if (t.IsSpecialName || t.IsCOMObject)
            {
                return false;
            }

            if (t.IsNested)
            {
                return t.IsNestedPublic && (t.IsClass || (t.IsAbstract && !t.IsSealed));
            }

            if (typeof(Delegate).IsAssignableFrom(t))
            {
                return false;
            }

            return (t.IsClass || (t.IsAbstract && !t.IsSealed)) && t.IsPublic;
        }


        /// <summary>
        /// Returns true if the type is a enumerator collection type.
        /// </summary>
        public static bool Enumerator(this HUMType.Data.Is isData)
        {
            if (isData.type == typeof(IEnumerator) || isData.type == typeof(IEnumerable) || isData.type == typeof(IEnumerator<>) || isData.type == typeof(IEnumerable<>))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the type contains a field or method this or returns an Enumerator
        /// </summary>
        public static bool Enumerator(this HUMType.Data.Has hasData, bool fields = true, bool methods = true)
        {
            return hasData.type.HasFieldOrMethodOfType<IEnumerator>(fields, methods);
        }

        /// <summary>
        /// Returns true if the enumerator includes a generic argument.
        /// </summary>
        public static bool Enumerator(this HUMType.Data.Generic genericData)
        {
            var interfaces = genericData.isData.type.GetInterfaces();

            for (int i = 0; i < interfaces.Length; i++)
            {
                if (interfaces[i].IsGenericType && interfaces[i].GetGenericTypeDefinition() == typeof(IEnumerator<>))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if an enumerable includes a generic argument.
        /// </summary>
        public static bool Enumerable(this HUMType.Data.Generic genericData)
        {
            var interfaces = genericData.isData.type.GetInterfaces();

            for (int i = 0; i < interfaces.Length; i++)
            {
                if (interfaces[i].IsGenericType && interfaces[i].GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if the type is an enumerable collection type.
        /// </summary>
        public static bool Enumerable(this HUMType.Data.Is isData)
        {
            return isData.type.Inherits(typeof(IEnumerator)) || isData.type.Inherits(typeof(IEnumerable)) || isData.type.Is().Generic().Enumerator() || isData.Enumerator();
        }

        /// <summary>
        /// Returns true if the type has a field or method of this a type.
        /// </summary>
        public static bool Enumerable(this HUMType.Data.Has has, bool fields = true, bool methods = true)
        {
            return has.type.HasFieldOrMethodOfType<IEnumerable>(fields, methods);
        }

        /// <summary>
        /// Returns true if the type is a Coroutine.
        /// </summary>
        public static bool Coroutine(this HUMType.Data.Is isData)
        {
            if (isData.type == typeof(IEnumerator) && isData.type != typeof(IEnumerable) && isData.type != typeof(IEnumerator<>) && isData.type != typeof(IEnumerable<>))
            {
                return true;
            }

            return isData.type == typeof(Coroutine) || isData.type == typeof(YieldInstruction) || isData.type == typeof(CustomYieldInstruction);
        }

        /// <summary>
        /// Returns true if the type is a GameObject or a Component type.
        /// </summary>
        public static bool UnityObject(this HUMType.Data.Is isData)
        {
            return isData.type == typeof(GameObject) || typeof(Component).IsAssignableFrom(isData);
        }

        /// <summary>
        /// Finds all types with this attribute.
        /// </summary>
        public static Type[] Attribute<TAttribute>(this HUMType.Data.With with, Assembly _assembly = null, Func<TAttribute, bool> predicate = null) where TAttribute : Attribute
        {
            List<Type> result = new List<Type>();
            Assembly[] assemblies = _assembly == null ? AppDomain.CurrentDomain.GetAssemblies() : new Assembly[] { _assembly };

            for (int assembly = 0; assembly < assemblies.Length; assembly++)
            {
                Type[] types = assemblies[assembly].GetTypes();

                for (int type = 0; type < types.Length; type++)
                {
                    if (with.types.get.type.IsAssignableFrom(types[type]))
                    {
                        if (types[type].IsAbstract) continue;
                        var attribs = types[type].GetCustomAttributes(typeof(TAttribute), false);
                        if (attribs == null || attribs.Length == 0) continue;
                        TAttribute attrib = attribs[0] as TAttribute;
                        attrib.Is().NotNull(() =>
                        {
                            if (predicate != null)
                            {
                                if (predicate(attrib))
                                    result.Add(types[type]);
                            }
                            else
                            {
                                result.Add(types[type]);
                            }
                        });
                    }
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// Finds all types with this attribute.
        /// </summary>
        public static Type[] Attribute<TAttribute>(this HUMType.Data.With with, Func<TAttribute, bool> predicate) where TAttribute : Attribute
        {
            List<Type> result = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (int assembly = 0; assembly < assemblies.Length; assembly++)
            {
                Type[] types = assemblies[assembly].GetTypes();

                for (int type = 0; type < types.Length; type++)
                {
                    if (with.types.get.type.IsAssignableFrom(types[type]))
                    {
                        if (types[type].IsAbstract) continue;
                        var attribs = types[type].GetCustomAttributes(typeof(TAttribute), false);
                        if (attribs == null || attribs.Length == 0) continue;
                        TAttribute attrib = attribs[0] as TAttribute;
                        attrib.Is().NotNull(() =>
                        {
                            if (predicate(attrib))
                                result.Add(types[type]);
                        });
                    }
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// Starts an operation where we determing if a type is a generic of some kind.
        /// </summary>
        public static HUMType.Data.Generic Generic(this HUMType.Data.Is isData)
        {
            return new HUMType.Data.Generic(isData);
        }

        /// <summary>
        /// Returns all types that derive or have a base type of a this type.
        /// </summary>
        public static Type[] Derived(this HUMType.Data.Get derived, bool includeSelf = false)
        {
            List<Type> result = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (int assembly = 0; assembly < assemblies.Length; assembly++)
            {
                Type[] types = assemblies[assembly].GetTypes();

                for (int type = 0; type < types.Length; type++)
                {
                    if ((!types[type].IsAbstract && !types[type].IsInterface) && derived.type.IsAssignableFrom(types[type]))
                    {
                        result.Add(types[type]);
                    }
                }
            }
            if (includeSelf) result.Add(derived.type);
            return result.ToArray();
        }

        /// <summary>
        /// Returns all types that derive or have a base type of a this type.
        /// </summary>
        public static void Derived(this HUMType.Data.Get derived, Action<Type> action, bool includeSelf = false)
        {
            List<Type> result = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            if (includeSelf)
                action?.Invoke(derived.type);
            for (int assembly = 0; assembly < assemblies.Length; assembly++)
            {
                Type[] types = assemblies[assembly].GetTypes();

                for (int i = 0; i < types.Length; i++)
                {
                    var type = types[i];
                    if (!type.IsAbstract && !type.IsInterface && derived.type.IsAssignableFrom(type))
                    {
                        action?.Invoke(type);
                    }
                }
            }
        }

        /// <summary>
        /// Returns all types that derive or have a base type of a this type.
        /// </summary>
        public static Type[] All(this HUMType.Data.Get derived, bool includeSelf = false)
        {
            List<Type> result = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (int assembly = 0; assembly < assemblies.Length; assembly++)
            {
                Type[] types = assemblies[assembly].GetTypes();

                for (int type = 0; type < types.Length; type++)
                {
                    if (derived.type.IsAssignableFrom(types[type]))
                    {
                        result.Add(types[type]);
                    }
                }
            }
            if (includeSelf) result.Add(derived.type);
            return result.ToArray();
        }

        public static bool PrimitiveOrString(this HUMType.Data.Is @is)
        {
            return @is.type.IsPrimitive || @is.type == typeof(string);
        }

        public static bool PrimitiveStringOrVoid(this HUMType.Data.Is @is)
        {
            if (@is.type == null) return false;
            return @is.type.IsPrimitive || @is.type == typeof(string) || @is.type == typeof(void) || @is.type.BaseType == null && @is.type == typeof(object);
        }

        /// <summary>
        /// Continues the get operation by getting types of some kind.
        /// </summary>
        public static HUMType.Data.Types Types(this HUMType.Data.Get get)
        {
            return new HUMType.Data.Types(get);
        }

        /// <summary>
        /// Converts a value into code form. Example: a float value of '10' would be '10f'. A string would add quotes, etc.
        /// </summary>
        public static string Code(this HUMValue.Data.As @as, bool isNew, bool isLiteral = false, bool highlight = true, string parameters = "", bool newLineLiteral = false, bool fullName = false, bool variableForObjects = true)
        {
            if (highlight) return HighlightedCode(@as, isNew, isLiteral, parameters, newLineLiteral, fullName, variableForObjects);
            Type type = @as.value?.GetType();
            if (@as.value is Type) return "typeof(" + ((Type)@as.value).As().CSharpName(false, fullName, false) + ")";
            if (type == null) return "null";
            if (type == typeof(void)) return "void";
            if (type == typeof(bool)) return @as.value.ToString().ToLower();
            if (type == typeof(float)) return @as.value.ToString().Replace(",", ".") + "f";
            if (type == typeof(double) || type == typeof(decimal)) return @as.value.ToString().Replace(",", ".");
            if (type == typeof(string))
            {
                var str = @as.value.ToString();
                if (str.Contains('\n') || str.Contains('\r'))
                {
                    var lines = str.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
                    string output = "@\"";

                    for (int i = 0; i < lines.Length; i++)
                    {
                        var line = lines[i];
                        bool isLast = i == lines.Length - 1;

                        output += line;
                        if (!isLast)
                            output += "\n";
                    }

                    output += "\"";
                    return output;
                }
                else
                {
                    return "\"" + str + "\"";
                }
            }
            if (type == typeof(char)) return string.IsNullOrEmpty(@as.value.ToString()) ? "new Char()" : $"'{@as.value}'";
            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                if (variableForObjects)
                {
                    var hasVariable = CodeGeneratorValueUtility.TryGetVariable((UnityEngine.Object)@as.value, out string current);
                    var variable = hasVariable ? current : $"ObjectVariable_{(@as.value as UnityEngine.Object).name.LegalMemberName() + "_" + Guid.NewGuid().ToString().Substring(0, 3)}";

                    if (!hasVariable)
                    {
                        CodeGeneratorValueUtility.AddValue(variable, (UnityEngine.Object)@as.value);
                    }
                    else
                    {
                        CodeGeneratorValueUtility.SetIsUsed(current);
                    }
                    return variable;
                }
                else
                {
                    return "null";
                }
            }

            //Special Cases
            if (type == typeof(Vector2))
            {
                var value = (Vector2)@as.value;
                return Create("Vector2", value.x.As().Code(false, false, false, "", false, fullName), value.y.As().Code(false, false, false, "", false, fullName));
            }
            if (type == typeof(Vector3))
            {
                var value = (Vector3)@as.value;
                return Create("Vector3", value.x.As().Code(false, false, false, "", false, fullName), value.y.As().Code(false, false, false, "", false, fullName), value.z.As().Code(false, false, false, "", false, fullName));
            }
            if (type == typeof(Vector4))
            {
                var value = (Vector4)@as.value;
                return Create("Vector4", value.x.As().Code(false, false, false, "", false, fullName), value.y.As().Code(false, false, false, "", false, fullName), value.z.As().Code(false, false, false, "", false, fullName), value.w.As().Code(false, false, false, "", false, fullName));
            }
            if (type == typeof(AnimationCurve))
            {
                var value = @as.value as AnimationCurve;
                return Create("AnimationCurve", value.keys.Select(k => Create("Keyframe", k.time.As().Code(false, false, false, "", false, fullName), k.value.As().Code(false, false, false, "", false, fullName), k.inTangent.As().Code(false, false, false, "", false, fullName), k.outTangent.As().Code(false, false, false, "", false, fullName), k.inWeight.As().Code(false, false, false, "", false, fullName), k.outWeight.As().Code(false, false, false, "", false, fullName))).ToArray());
            }
            if (type == typeof(Color))
            {
                var value = (Color)@as.value;
                return Create("Color", value.r.As().Code(false, false, false, "", false, fullName), value.g.As().Code(false, false, false, "", false, fullName), value.b.As().Code(false, false, false, "", false, fullName), value.a.As().Code(false, false, false, "", false, fullName));
            }
            if (type == typeof(WaitForFlowLogic))
            {
                var value = (WaitForFlowLogic)@as.value;
                return Create("WaitForFlowLogic", value.InputCount.As().Code(false, false, false, "", false, fullName), value.ResetOnExit.As().Code(false, false, false, "", false, fullName));
            }
            if (type.IsNumeric()) return @as.value.ToString();
            if (type.IsEnum) return (@as.value as Enum).ToMultipleEnumString(false, " | ", fullName);
            if (isNew)
            {
                if (type.IsClass || !type.IsClass && !type.IsInterface && !type.IsEnum)
                {
                    if (type.IsGenericType) return isLiteral ? Literal(@as.value, newLineLiteral, fullName, false, variableForObjects) : "new " + GenericDeclaration(type, fullName, false) + "()";
                    if (type.IsConstructedGenericType) return isLiteral ? Literal(@as.value, newLineLiteral, fullName, false, variableForObjects) : "new " + GenericDeclaration(type, fullName, false) + "(" + parameters + ")";
                    return isLiteral ? Literal(@as.value, newLineLiteral, fullName, false, variableForObjects) : "new " + type.As().CSharpName(false, fullName, false) + "(" + parameters + ")";
                }
                else
                {
                    if (type.IsValueType && !type.IsEnum && !type.IsPrimitive) return isLiteral ? Literal(@as.value, newLineLiteral, fullName, false, variableForObjects) : "new " + type.As().CSharpName(false, fullName, false) + "(" + parameters + ")";
                }
            }

            return @as.value.ToString();
        }

        private static string HighlightedCode(this HUMValue.Data.As @as, bool isNew, bool isLiteral = false, string parameters = "", bool newLineLiteral = false, bool fullName = false, bool variableForObjects = true)
        {
            Type type = @as.value?.GetType();
            if (@as.value is Type) return "typeof".ConstructHighlight() + "(" + ((Type)@as.value).As().CSharpName(false, fullName) + ")";
            if (type == null) return "null".ConstructHighlight();
            if (type == typeof(void)) return "void".ConstructHighlight();
            if (type == typeof(bool)) return @as.value.ToString().ToLower().ConstructHighlight();
            if (type == typeof(float)) return (@as.value.ToString().Replace(",", ".") + "f").NumericHighlight();
            if (type == typeof(double) || type == typeof(decimal)) return @as.value.ToString().Replace(",", ".").NumericHighlight();
            if (type == typeof(string))
            {
                var str = @as.value.ToString();
                if (str.Contains('\n') || str.Contains('\r'))
                {
                    var lines = str.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
                    string output = "@\"".StringHighlight();

                    for (int i = 0; i < lines.Length; i++)
                    {
                        var line = lines[i];
                        bool isLast = i == lines.Length - 1;

                        output += line.StringHighlight();
                        if (!isLast)
                            output += "\n";
                    }

                    output += "\"".StringHighlight();
                    return output;
                }
                else
                {
                    return ("\"" + str + "\"").StringHighlight();
                }
            }
            if (type == typeof(char)) return (char)@as.value == char.MinValue ? "/* Cannot have a empty character */".ErrorHighlight() : $"'{@as.value}'".StringHighlight();
            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                if (variableForObjects)
                {
                    var hasVariable = CodeGeneratorValueUtility.TryGetVariable((UnityEngine.Object)@as.value, out string current);
                    var variable = hasVariable ? current : $"ObjectVariable_{(@as.value as UnityEngine.Object).name.LegalMemberName() + "_" + Guid.NewGuid().ToString().Substring(0, 3)}";

                    if (!hasVariable)
                        CodeGeneratorValueUtility.AddValue(variable, (UnityEngine.Object)@as.value);
                    else
                        CodeGeneratorValueUtility.SetIsUsed(current);
                    return variable.VariableHighlight();
                }
                else
                {
                    return "null".ConstructHighlight();
                }
            }

            //Special Cases
            if (type == typeof(Vector2))
            {
                var value = (Vector2)@as.value;
                return CreateHighlighted("Vector2", value.x.As().Code(false, false, true, "", false, fullName), value.y.As().Code(false, false, true, "", false, fullName));
            }
            if (type == typeof(Vector3))
            {
                var value = (Vector3)@as.value;
                return CreateHighlighted("Vector3", value.x.As().Code(false, false, true, "", false, fullName), value.y.As().Code(false, false, true, "", false, fullName), value.z.As().Code(false, false, true, "", false, fullName));
            }
            if (type == typeof(Vector4))
            {
                var value = (Vector4)@as.value;
                return CreateHighlighted("Vector4", value.x.As().Code(false, false, true, "", false, fullName), value.y.As().Code(false, false, true, "", false, fullName), value.z.As().Code(false, false, true, "", false, fullName), value.w.As().Code(false, false, true, "", false, fullName));
            }
            if (type == typeof(AnimationCurve))
            {
                var value = @as.value as AnimationCurve;
                return CreateHighlighted("AnimationCurve", value.keys.Select(k => CreateHighlighted("Keyframe", k.time.As().Code(false, false, true, "", false, fullName), k.value.As().Code(false, false, true, "", false, fullName), k.inTangent.As().Code(false, false, true, "", false, fullName), k.outTangent.As().Code(false, false, true, "", false, fullName), k.inWeight.As().Code(false, false, true, "", false, fullName), k.outWeight.As().Code(false, false, true, "", false, fullName))).ToArray());
            }
            if (type == typeof(Color))
            {
                var value = (Color)@as.value;
                return CreateHighlighted("Color", value.r.As().Code(false, false, true, "", false, fullName), value.g.As().Code(false, false, true, "", false, fullName), value.b.As().Code(false, false, true, "", false, fullName), value.a.As().Code(false, false, true, "", false, fullName));
            }
            if (type == typeof(WaitForFlowLogic))
            {
                var value = (WaitForFlowLogic)@as.value;
                return CreateHighlighted("WaitForFlowLogic", value.InputCount.As().Code(false, false, true, "", false, fullName), value.ResetOnExit.As().Code(false, false, true, "", false, fullName));
            }
            if (type.IsNumeric()) return @as.value.ToString().NumericHighlight();
            if (type.IsEnum) return (@as.value as Enum).ToMultipleEnumString(true, " | ", fullName);
            if (isNew)
            {
                if (type.IsClass || !type.IsClass && !type.IsInterface && !type.IsEnum)
                {
                    if (type.IsGenericType) return isLiteral ? Literal(@as.value, newLineLiteral, fullName, true, variableForObjects) : "new ".ConstructHighlight() + GenericDeclaration(type, fullName) + "()";
                    if (type.IsConstructedGenericType) return isLiteral ? Literal(@as.value, newLineLiteral, fullName, true, variableForObjects) : "new ".ConstructHighlight() + GenericDeclaration(type, fullName) + "(" + parameters + ")";
                    return isLiteral ? Literal(@as.value, newLineLiteral, fullName, true, variableForObjects) : "new ".ConstructHighlight() + type.As().CSharpName(false, fullName, true) + "(" + parameters + ")";
                }
                else
                {
                    if (type.IsValueType && !type.IsEnum && !type.IsPrimitive) return isLiteral ? Literal(@as.value, newLineLiteral, fullName, true, variableForObjects) : "new ".ConstructHighlight() + type.As().CSharpName(false, fullName, true) + "(" + parameters + ")";
                }
            }

            return @as.value.ToString();
        }
        /// <summary>
        /// Converts a value into code form. Example: a float value of '10' would be '10f'. A string would add quotes, etc.
        /// </summary>
        public static string Code(this HUMValue.Data.As @as, bool isNew, bool isLiteral = false, bool highlight = true, string parameters = "", bool newLineLiteral = false, Func<Type, bool> fullName = null, bool variableForObjects = true)
        {
            if (highlight) return HighlightedCode(@as, isNew, isLiteral, parameters, newLineLiteral, fullName, variableForObjects);
            Type type = @as.value?.GetType();
            if (@as.value is Type) return "typeof(" + ((Type)@as.value).As().CSharpName(false, fullName, false) + ")";
            if (type == null) return "null";
            if (type == typeof(void)) return "void";
            if (type == typeof(bool)) return @as.value.ToString().ToLower();
            if (type == typeof(float)) return @as.value.ToString().Replace(",", ".") + "f";
            if (type == typeof(double) || type == typeof(decimal)) return @as.value.ToString().Replace(",", ".");
            if (type == typeof(string))
            {
                var str = @as.value.ToString();
                if (str.Contains('\n') || str.Contains('\r'))
                {
                    var lines = str.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
                    string output = "@\"";

                    for (int i = 0; i < lines.Length; i++)
                    {
                        var line = lines[i];
                        bool isLast = i == lines.Length - 1;

                        output += line;
                        if (!isLast)
                            output += "\n";
                    }

                    output += "\"";
                    return output;
                }
                else
                {
                    return "\"" + str + "\"";
                }
            }
            if (type == typeof(char)) return string.IsNullOrEmpty(@as.value.ToString()) ? "new Char()" : $"'{@as.value}'";
            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                if (variableForObjects)
                {
                    var hasVariable = CodeGeneratorValueUtility.TryGetVariable((UnityEngine.Object)@as.value, out string current);
                    var variable = hasVariable ? current : $"ObjectVariable_{(@as.value as UnityEngine.Object).name.LegalMemberName() + "_" + Guid.NewGuid().ToString().Substring(0, 3)}";

                    if (!hasVariable)
                    {
                        CodeGeneratorValueUtility.AddValue(variable, (UnityEngine.Object)@as.value);
                    }
                    else
                    {
                        CodeGeneratorValueUtility.SetIsUsed(current);
                    }
                    return variable;
                }
                else
                {
                    return "null";
                }
            }

            //Special Cases
            if (type == typeof(Vector2))
            {
                var value = (Vector2)@as.value;
                return Create("Vector2", value.x.As().Code(false, false, false, "", false, fullName), value.y.As().Code(false, false, false, "", false, fullName));
            }
            if (type == typeof(Vector3))
            {
                var value = (Vector3)@as.value;
                return Create("Vector3", value.x.As().Code(false, false, false, "", false, fullName), value.y.As().Code(false, false, false, "", false, fullName), value.z.As().Code(false, false, false, "", false, fullName));
            }
            if (type == typeof(Vector4))
            {
                var value = (Vector4)@as.value;
                return Create("Vector4", value.x.As().Code(false, false, false, "", false, fullName), value.y.As().Code(false, false, false, "", false, fullName), value.z.As().Code(false, false, false, "", false, fullName), value.w.As().Code(false, false, false, "", false, fullName));
            }
            if (type == typeof(AnimationCurve))
            {
                var value = @as.value as AnimationCurve;
                return Create("AnimationCurve", value.keys.Select(k => Create("Keyframe", k.time.As().Code(false, false, false, "", false, fullName), k.value.As().Code(false, false, false, "", false, fullName), k.inTangent.As().Code(false, false, false, "", false, fullName), k.outTangent.As().Code(false, false, false, "", false, fullName), k.inWeight.As().Code(false, false, false, "", false, fullName), k.outWeight.As().Code(false, false, false, "", false, fullName))).ToArray());
            }
            if (type == typeof(Color))
            {
                var value = (Color)@as.value;
                return Create("Color", value.r.As().Code(false, false, false, "", false, fullName), value.g.As().Code(false, false, false, "", false, fullName), value.b.As().Code(false, false, false, "", false, fullName), value.a.As().Code(false, false, false, "", false, fullName));
            }
            if (type == typeof(WaitForFlowLogic))
            {
                var value = (WaitForFlowLogic)@as.value;
                return Create("WaitForFlowLogic", value.InputCount.As().Code(false, false, false, "", false, fullName), value.ResetOnExit.As().Code(false, false, false, "", false, fullName));
            }
            if (type.IsNumeric()) return @as.value.ToString();
            if (type.IsEnum) return (@as.value as Enum).ToMultipleEnumString(false, " | ", fullName != null && fullName(type));
            if (isNew)
            {
                if (type.IsClass || !type.IsClass && !type.IsInterface && !type.IsEnum)
                {
                    if (type.IsGenericType) return isLiteral ? Literal(@as.value, newLineLiteral, fullName, false, variableForObjects) : "new " + GenericDeclaration(type, fullName, false) + "()";
                    if (type.IsConstructedGenericType) return isLiteral ? Literal(@as.value, newLineLiteral, fullName, false, variableForObjects) : "new " + GenericDeclaration(type, fullName, false) + "(" + parameters + ")";
                    return isLiteral ? Literal(@as.value, newLineLiteral, fullName, false, variableForObjects) : "new " + type.As().CSharpName(false, fullName, false) + "(" + parameters + ")";
                }
                else
                {
                    if (type.IsValueType && !type.IsEnum && !type.IsPrimitive) return isLiteral ? Literal(@as.value, newLineLiteral, fullName, false, variableForObjects) : "new " + type.As().CSharpName(false, fullName, false) + "(" + parameters + ")";
                }
            }

            return @as.value.ToString();
        }

        private static string HighlightedCode(this HUMValue.Data.As @as, bool isNew, bool isLiteral = false, string parameters = "", bool newLineLiteral = false, Func<Type, bool> fullName = null, bool variableForObjects = true)
        {
            Type type = @as.value?.GetType();
            if (@as.value is Type) return "typeof".ConstructHighlight() + "(" + ((Type)@as.value).As().CSharpName(false, fullName) + ")";
            if (type == null) return "null".ConstructHighlight();
            if (type == typeof(void)) return "void".ConstructHighlight();
            if (type == typeof(bool)) return @as.value.ToString().ToLower().ConstructHighlight();
            if (type == typeof(float)) return (@as.value.ToString().Replace(",", ".") + "f").NumericHighlight();
            if (type == typeof(double) || type == typeof(decimal)) return @as.value.ToString().Replace(",", ".").NumericHighlight();
            if (type == typeof(string))
            {
                var str = @as.value.ToString();
                if (str.Contains('\n') || str.Contains('\r'))
                {
                    var lines = str.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
                    string output = "@\"".StringHighlight();

                    for (int i = 0; i < lines.Length; i++)
                    {
                        var line = lines[i];
                        bool isLast = i == lines.Length - 1;

                        output += line.StringHighlight();
                        if (!isLast)
                            output += "\n";
                    }

                    output += "\"".StringHighlight();
                    return output;
                }
                else
                {
                    return ("\"" + str + "\"").StringHighlight();
                }
            }
            if (type == typeof(char)) return (char)@as.value == char.MinValue ? "/* Cannot have a empty character */".ErrorHighlight() : $"'{@as.value}'".StringHighlight();
            if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                if (variableForObjects)
                {
                    var hasVariable = CodeGeneratorValueUtility.TryGetVariable((UnityEngine.Object)@as.value, out string current);
                    var variable = hasVariable ? current : $"ObjectVariable_{(@as.value as UnityEngine.Object).name.LegalMemberName() + "_" + Guid.NewGuid().ToString().Substring(0, 3)}";

                    if (!hasVariable)
                        CodeGeneratorValueUtility.AddValue(variable, (UnityEngine.Object)@as.value);
                    else
                        CodeGeneratorValueUtility.SetIsUsed(current);
                    return variable.VariableHighlight();
                }
                else
                {
                    return "null".ConstructHighlight();
                }
            }

            //Special Cases
            if (type == typeof(Vector2))
            {
                var value = (Vector2)@as.value;
                return CreateHighlighted("Vector2", value.x.As().Code(false, false, true, "", false, fullName), value.y.As().Code(false, false, true, "", false, fullName));
            }
            if (type == typeof(Vector3))
            {
                var value = (Vector3)@as.value;
                return CreateHighlighted("Vector3", value.x.As().Code(false, false, true, "", false, fullName), value.y.As().Code(false, false, true, "", false, fullName), value.z.As().Code(false, false, true, "", false, fullName));
            }
            if (type == typeof(Vector4))
            {
                var value = (Vector4)@as.value;
                return CreateHighlighted("Vector4", value.x.As().Code(false, false, true, "", false, fullName), value.y.As().Code(false, false, true, "", false, fullName), value.z.As().Code(false, false, true, "", false, fullName), value.w.As().Code(false, false, true, "", false, fullName));
            }
            if (type == typeof(AnimationCurve))
            {
                var value = @as.value as AnimationCurve;
                return CreateHighlighted("AnimationCurve", value.keys.Select(k => CreateHighlighted("Keyframe", k.time.As().Code(false, false, true, "", false, fullName), k.value.As().Code(false, false, true, "", false, fullName), k.inTangent.As().Code(false, false, true, "", false, fullName), k.outTangent.As().Code(false, false, true, "", false, fullName), k.inWeight.As().Code(false, false, true, "", false, fullName), k.outWeight.As().Code(false, false, true, "", false, fullName))).ToArray());
            }
            if (type == typeof(Color))
            {
                var value = (Color)@as.value;
                return CreateHighlighted("Color", value.r.As().Code(false, false, true, "", false, fullName), value.g.As().Code(false, false, true, "", false, fullName), value.b.As().Code(false, false, true, "", false, fullName), value.a.As().Code(false, false, true, "", false, fullName));
            }
            if (type == typeof(WaitForFlowLogic))
            {
                var value = (WaitForFlowLogic)@as.value;
                return CreateHighlighted("WaitForFlowLogic", value.InputCount.As().Code(false, false, true, "", false, fullName), value.ResetOnExit.As().Code(false, false, true, "", false, fullName));
            }
            if (type.IsNumeric()) return @as.value.ToString().NumericHighlight();
            if (type.IsEnum) return (@as.value as Enum).ToMultipleEnumString(true, " | ", fullName != null && fullName(type));
            if (isNew)
            {
                if (type.IsClass || !type.IsClass && !type.IsInterface && !type.IsEnum)
                {
                    if (type.IsGenericType) return isLiteral ? Literal(@as.value, newLineLiteral, fullName, true, variableForObjects) : "new ".ConstructHighlight() + GenericDeclaration(type, fullName) + "()";
                    if (type.IsConstructedGenericType) return isLiteral ? Literal(@as.value, newLineLiteral, fullName, true, variableForObjects) : "new ".ConstructHighlight() + GenericDeclaration(type, fullName) + "(" + parameters + ")";
                    return isLiteral ? Literal(@as.value, newLineLiteral, fullName, true, variableForObjects) : "new ".ConstructHighlight() + type.As().CSharpName(false, fullName, true) + "(" + parameters + ")";
                }
                else
                {
                    if (type.IsValueType && !type.IsEnum && !type.IsPrimitive) return isLiteral ? Literal(@as.value, newLineLiteral, fullName, true, variableForObjects) : "new ".ConstructHighlight() + type.As().CSharpName(false, fullName, true) + "(" + parameters + ")";
                }
            }

            return @as.value.ToString();
        }

        private static string New(bool highlight = true)
        {
            return highlight ? "new ".ConstructHighlight() : "new ";
        }

        private static string CreateHighlighted(string type, params string[] parameters)
        {
            return New() + Type(type) + "(" + string.Join(", ", parameters) + ")";
        }

        private static string Create(string type, params string[] parameters)
        {
            return New(false) + Type(type, false) + "(" + string.Join(", ", parameters) + ")";
        }

        private static string Type(string type, bool highlight = true)
        {
            return highlight ? type.TypeHighlight() : type;
        }

        private static readonly Dictionary<Type, MemberInfo[]> memberCache = new Dictionary<Type, MemberInfo[]>();

        private static MemberInfo[] GetCachedMembers(Type type)
        {
            if (type == null) return Array.Empty<MemberInfo>();

            if (!memberCache.TryGetValue(type, out var members))
            {
                var memberList = new List<MemberInfo>();

                memberList.AddRange(type.GetFields(BindingFlags.Public | BindingFlags.Instance));

                memberList.AddRange(
                    type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(p => p.HasAttribute<GeneratePropertyAttribute>())
                );

                members = memberList.ToArray();
                memberCache[type] = members;
            }

            return members;
        }

        private static string Literal(object value, bool newLine = false, bool fullName = false, bool highlight = true, bool variableForObjects = true)
        {
            if (highlight)
            {
                return HightlightedLiteral(value, newLine, fullName, variableForObjects);
            }

            var members = GetCachedMembers(value?.GetType());
            var output = string.Empty;
            var usableMembers = new List<MemberInfo>();

            foreach (var member in members)
            {
                if (member is FieldInfo field)
                {
                    if (field.IsPublic && !field.IsStatic && !field.IsInitOnly)
                        usableMembers.Add(field);
                }
                else if (member is PropertyInfo property)
                {
                    if (property.SetMethod != null && property.SetMethod.IsPublic && !property.IsStatic())
                        usableMembers.Add(property);
                }
            }

            output += (newLine ? "\n" + CodeBuilder.GetCurrentIndent() : string.Empty) +
            "new " + (!value.GetType().IsGenericType ? value.GetType().As().CSharpName(false, fullName, false) : GenericDeclaration(value.GetType(), fullName)) +
            (value.GetType().IsArray ? "" : "()");

            bool isMultiLine = (value is ICollection col && col.Count > 0) || usableMembers.Count > 0;

            if (isMultiLine)
            {
                output += "\n" + CodeBuilder.GetCurrentIndent() + "{";
                CodeBuilder.Indent(CodeBuilder.currentIndent + 1);
            }

            var indent = CodeBuilder.GetCurrentIndent();

            if (value is IList list && list.Count > 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    output += "\n" + indent +
                    list[i].As().Code(true, true, false, "", false, fullName, variableForObjects);

                    if (i < list.Count - 1)
                        output += ", ";
                }
            }
            else if (value is IDictionary dict && dict.Count > 0)
            {
                int i = 0;
                foreach (DictionaryEntry entry in dict)
                {
                    output += "\n" + indent + "{ " +
                    entry.Key.As().Code(true, true, false, "", false, fullName, variableForObjects) +
                    ", " +
                    entry.Value.As().Code(true, true, false, "", false, fullName, variableForObjects) +
                    " }";

                    if (++i < dict.Count)
                        output += ", ";
                }
            }
            else if (value is Array array && array.Length > 0)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    output += "\n" + indent +
                    array.GetValue(i).As().Code(true, true, false, "", false, fullName, variableForObjects);

                    if (i < array.Length - 1)
                        output += ", ";
                }
            }

            for (int i = 0; i < usableMembers.Count; i++)
            {
                var memberValue = usableMembers[i] is FieldInfo f
                    ? f.GetValueOptimized(value)
                    : ((PropertyInfo)usableMembers[i]).GetValueOptimized(value);

                output += "\n" + indent +
                          usableMembers[i].Name + " = " +
                          memberValue.As().Code(true, true, false, "", false, fullName, variableForObjects);

                if (i < usableMembers.Count - 1)
                    output += ", ";
            }

            if (isMultiLine)
            {
                CodeBuilder.Indent(CodeBuilder.currentIndent - 1);
                output += "\n" + CodeBuilder.GetCurrentIndent() + "}";
            }

            return output;
        }

        private static string HightlightedLiteral(object value, bool newLine = false, Func<Type, bool> fullName = null, bool variableForObjects = true)
        {
            var members = GetCachedMembers(value?.GetType());
            var output = string.Empty;
            var usableMembers = new List<MemberInfo>();

            foreach (var member in members)
            {
                if (member is FieldInfo field)
                {
                    if (field.IsPublic && !field.IsStatic && !field.IsInitOnly)
                        usableMembers.Add(field);
                }
                else if (member is PropertyInfo property)
                {
                    if (property.SetMethod != null && property.SetMethod.IsPublic && !property.IsStatic())
                        usableMembers.Add(property);
                }
            }

            output += (newLine ? "\n" + CodeBuilder.GetCurrentIndent() : string.Empty) +
            "new ".ConstructHighlight() +
            (!value.GetType().IsGenericType ? value.GetType().As().CSharpName(false, fullName) : GenericDeclaration(value.GetType(), fullName)) +
            (value.GetType().IsArray ? "" : "()");

            bool isMultiLine = (value is ICollection col && col.Count > 0) || usableMembers.Count > 0;

            if (isMultiLine)
            {
                output += "\n" + CodeBuilder.GetCurrentIndent() + "{";
                CodeBuilder.Indent(CodeBuilder.currentIndent + 1);
            }

            var indent = CodeBuilder.GetCurrentIndent();

            if (value is IList list && list.Count > 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    output += "\n" + indent +
                    list[i].As().Code(true, true, true, "", false, fullName, variableForObjects);

                    if (i < list.Count - 1)
                        output += ", ";
                }
            }
            else if (value is IDictionary dict && dict.Count > 0)
            {
                int i = 0;
                foreach (DictionaryEntry entry in dict)
                {
                    output += "\n" + indent + "{ " +
                    entry.Key.As().Code(true, true, true, "", false, fullName, variableForObjects) +
                    ", " +
                    entry.Value.As().Code(true, true, true, "", false, fullName, variableForObjects) +
                    " }";

                    if (++i < dict.Count)
                        output += ", ";
                }
            }
            else if (value is Array array && array.Length > 0)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    output += "\n" + indent +
                    array.GetValue(i).As().Code(true, true, true, "", false, fullName, variableForObjects);

                    if (i < array.Length - 1)
                        output += ", ";
                }
            }

            for (int i = 0; i < usableMembers.Count; i++)
            {
                var memberValue = usableMembers[i] is FieldInfo f
                    ? f.GetValueOptimized(value)
                    : ((PropertyInfo)usableMembers[i]).GetValueOptimized(value);

                output += "\n" + indent +
                usableMembers[i].Name.VariableHighlight() + " = " +
                memberValue.As().Code(true, true, true, "", false, fullName, variableForObjects);

                if (i < usableMembers.Count - 1)
                    output += ", ";
            }

            if (isMultiLine)
            {
                CodeBuilder.Indent(CodeBuilder.currentIndent - 1);
                output += "\n" + CodeBuilder.GetCurrentIndent() + "}";
            }

            return output;
        }
        
        private static string Literal(object value, bool newLine = false, Func<Type, bool> fullName = null, bool highlight = true, bool variableForObjects = true)
        {
            if (highlight)
            {
                return HightlightedLiteral(value, newLine, fullName, variableForObjects);
            }

            var members = GetCachedMembers(value?.GetType());
            var output = string.Empty;
            var usableMembers = new List<MemberInfo>();

            foreach (var member in members)
            {
                if (member is FieldInfo field)
                {
                    if (field.IsPublic && !field.IsStatic && !field.IsInitOnly)
                        usableMembers.Add(field);
                }
                else if (member is PropertyInfo property)
                {
                    if (property.SetMethod != null && property.SetMethod.IsPublic && !property.IsStatic())
                        usableMembers.Add(property);
                }
            }

            output += (newLine ? "\n" + CodeBuilder.GetCurrentIndent() : string.Empty) +
            "new " + (!value.GetType().IsGenericType ? value.GetType().As().CSharpName(false, fullName, false) : GenericDeclaration(value.GetType(), fullName)) +
            (value.GetType().IsArray ? "" : "()");

            bool isMultiLine = (value is ICollection col && col.Count > 0) || usableMembers.Count > 0;

            if (isMultiLine)
            {
                output += "\n" + CodeBuilder.GetCurrentIndent() + "{";
                CodeBuilder.Indent(CodeBuilder.currentIndent + 1);
            }

            var indent = CodeBuilder.GetCurrentIndent();

            if (value is IList list && list.Count > 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    output += "\n" + indent +
                    list[i].As().Code(true, true, false, "", false, fullName, variableForObjects);

                    if (i < list.Count - 1)
                        output += ", ";
                }
            }
            else if (value is IDictionary dict && dict.Count > 0)
            {
                int i = 0;
                foreach (DictionaryEntry entry in dict)
                {
                    output += "\n" + indent + "{ " +
                    entry.Key.As().Code(true, true, false, "", false, fullName, variableForObjects) +
                    ", " +
                    entry.Value.As().Code(true, true, false, "", false, fullName, variableForObjects) +
                    " }";

                    if (++i < dict.Count)
                        output += ", ";
                }
            }
            else if (value is Array array && array.Length > 0)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    output += "\n" + indent +
                    array.GetValue(i).As().Code(true, true, false, "", false, fullName, variableForObjects);

                    if (i < array.Length - 1)
                        output += ", ";
                }
            }

            for (int i = 0; i < usableMembers.Count; i++)
            {
                var memberValue = usableMembers[i] is FieldInfo f
                    ? f.GetValueOptimized(value)
                    : ((PropertyInfo)usableMembers[i]).GetValueOptimized(value);

                output += "\n" + indent +
                          usableMembers[i].Name + " = " +
                          memberValue.As().Code(true, true, false, "", false, fullName, variableForObjects);

                if (i < usableMembers.Count - 1)
                    output += ", ";
            }

            if (isMultiLine)
            {
                CodeBuilder.Indent(CodeBuilder.currentIndent - 1);
                output += "\n" + CodeBuilder.GetCurrentIndent() + "}";
            }

            return output;
        }

        private static string HightlightedLiteral(object value, bool newLine = false, bool fullName = false, bool variableForObjects = true)
        {
            var members = GetCachedMembers(value?.GetType());
            var output = string.Empty;
            var usableMembers = new List<MemberInfo>();

            foreach (var member in members)
            {
                if (member is FieldInfo field)
                {
                    if (field.IsPublic && !field.IsStatic && !field.IsInitOnly)
                        usableMembers.Add(field);
                }
                else if (member is PropertyInfo property)
                {
                    if (property.SetMethod != null && property.SetMethod.IsPublic && !property.IsStatic())
                        usableMembers.Add(property);
                }
            }

            output += (newLine ? "\n" + CodeBuilder.GetCurrentIndent() : string.Empty) +
            "new ".ConstructHighlight() +
            (!value.GetType().IsGenericType ? value.GetType().As().CSharpName(false, fullName) : GenericDeclaration(value.GetType(), fullName)) +
            (value.GetType().IsArray ? "" : "()");

            bool isMultiLine = (value is ICollection col && col.Count > 0) || usableMembers.Count > 0;

            if (isMultiLine)
            {
                output += "\n" + CodeBuilder.GetCurrentIndent() + "{";
                CodeBuilder.Indent(CodeBuilder.currentIndent + 1);
            }

            var indent = CodeBuilder.GetCurrentIndent();

            if (value is IList list && list.Count > 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    output += "\n" + indent +
                    list[i].As().Code(true, true, true, "", false, fullName, variableForObjects);

                    if (i < list.Count - 1)
                        output += ", ";
                }
            }
            else if (value is IDictionary dict && dict.Count > 0)
            {
                int i = 0;
                foreach (DictionaryEntry entry in dict)
                {
                    output += "\n" + indent + "{ " +
                    entry.Key.As().Code(true, true, true, "", false, fullName, variableForObjects) +
                    ", " +
                    entry.Value.As().Code(true, true, true, "", false, fullName, variableForObjects) +
                    " }";

                    if (++i < dict.Count)
                        output += ", ";
                }
            }
            else if (value is Array array && array.Length > 0)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    output += "\n" + indent +
                    array.GetValue(i).As().Code(true, true, true, "", false, fullName, variableForObjects);

                    if (i < array.Length - 1)
                        output += ", ";
                }
            }

            for (int i = 0; i < usableMembers.Count; i++)
            {
                var memberValue = usableMembers[i] is FieldInfo f
                    ? f.GetValueOptimized(value)
                    : ((PropertyInfo)usableMembers[i]).GetValueOptimized(value);

                output += "\n" + indent +
                usableMembers[i].Name.VariableHighlight() + " = " +
                memberValue.As().Code(true, true, true, "", false, fullName, variableForObjects);

                if (i < usableMembers.Count - 1)
                    output += ", ";
            }

            if (isMultiLine)
            {
                CodeBuilder.Indent(CodeBuilder.currentIndent - 1);
                output += "\n" + CodeBuilder.GetCurrentIndent() + "}";
            }

            return output;
        }

        public static string GenericDeclaration(Type type, bool fullName = true, bool highlight = true)
        {
            if (!type.IsConstructedGenericType && !type.IsGenericType) throw new Exception("Type is not a generic type but you are trying to declare a generic.");

            return fullName ? type.CSharpFullName(true, highlight) : type.CSharpName(true, highlight);
        }

        public static string GenericDeclaration(Type type, Func<Type, bool> useFullName = null, bool highlight = true)
        {
            if (!type.IsConstructedGenericType && !type.IsGenericType) throw new Exception("Type is not a generic type but you are trying to declare a generic.");

            return type.CSharpFullName(useFullName, true, highlight);
        }

        public static string GenericDeclaration(Type type, List<Type> parameters, bool fullName = true, bool highlight = true)
        {
            var output = string.Empty;

            if (!type.IsConstructedGenericType && !type.IsGenericType) return type.As().CSharpName(false, fullName, highlight);
            if (highlight)
            {
                output += type.Name.Contains("`") ? type.Name.Remove(type.Name.IndexOf("`"), type.Name.Length - type.Name.IndexOf("`")).WithHighlight(type.IsInterface ? HighlightType.Interface : HighlightType.Type) : type.Name.WithHighlight(type.IsInterface ? HighlightType.Interface : HighlightType.Type);
            }
            else
            {
                output += type.Name.Contains("`") ? type.Name.Remove(type.Name.IndexOf("`"), type.Name.Length - type.Name.IndexOf("`")) : type.Name;
            }

            if (parameters.Count > 0)
            {
                output += "<";
                for (int i = 0; i < parameters.Count; i++)
                {
                    output += parameters[i].As().CSharpName(false, fullName, highlight);
                    if (i < parameters.Count - 1) output += ", ";
                }
                output += ">";
            }
            else
            {
                output += "<";

                var args = type.GetGenericArguments();

                for (int i = 0; i < args.Length; i++)
                {
                    output += args[i].As().CSharpName(false, fullName, highlight);
                    if (i < args.Length - 1) output += ", ";
                }

                output += ">";
            }

            return output;
        }

        public static string GenericDeclaration(Type type, bool fullName = true, bool highlight = true, params Type[] parameters)
        {
            var output = string.Empty;

            if (!type.IsConstructedGenericType && !type.IsGenericType) return type.As().CSharpName(false, highlight);
            if (highlight)
            {
                output += type.Name.Contains("`") ? type.Name.Remove(type.Name.IndexOf("`"), type.Name.Length - type.Name.IndexOf("`")).WithHighlight(type.IsInterface ? HighlightType.Interface : HighlightType.Type) : type.Name.WithHighlight(type.IsInterface ? HighlightType.Interface : HighlightType.Type);
            }
            else
            {
                output += type.Name.Contains("`") ? type.Name.Remove(type.Name.IndexOf("`"), type.Name.Length - type.Name.IndexOf("`")) : type.Name;
            }

            if (parameters.Length > 0)
            {
                output += "<";
                for (int i = 0; i < parameters.Length; i++)
                {
                    output += parameters[i].As().CSharpName(false, fullName, highlight);
                    if (i < parameters.Length - 1) output += ", ";
                }
                output += ">";
            }
            else
            {
                output += "<";

                var args = type.GetGenericArguments();

                for (int i = 0; i < args.Length; i++)
                {
                    output += args[i].As().CSharpName(false, fullName, highlight);
                    if (i < args.Length - 1) output += ", ";
                }

                output += ">";
            }

            return output;
        }

    }
}

namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// Copied from visual scripting
    /// </summary>
    public static class CSharpNameUtility
    {
        private static readonly Dictionary<Type, string> primitives = new Dictionary<Type, string>
        {
            { typeof(byte), "byte" },
            { typeof(sbyte), "sbyte" },
            { typeof(short), "short" },
            { typeof(ushort), "ushort" },
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(float), "float" },
            { typeof(double), "double" },
            { typeof(decimal), "decimal" },
            { typeof(string), "string" },
            { typeof(char), "char" },
            { typeof(bool), "bool" },
            { typeof(void), "void" },
            { typeof(object), "object" },
        };

        public static readonly Dictionary<string, string> operators = new Dictionary<string, string>
        {
            { "op_Addition", "+" },
            { "op_Subtraction", "-" },
            { "op_Multiply", "*" },
            { "op_Division", "/" },
            { "op_Modulus", "%" },
            { "op_ExclusiveOr", "^" },
            { "op_BitwiseAnd", "&" },
            { "op_BitwiseOr", "|" },
            { "op_LogicalAnd", "&&" },
            { "op_LogicalOr", "||" },
            { "op_Assign", "=" },
            { "op_LeftShift", "<<" },
            { "op_RightShift", ">>" },
            { "op_Equality", "==" },
            { "op_GreaterThan", ">" },
            { "op_LessThan", "<" },
            { "op_Inequality", "!=" },
            { "op_GreaterThanOrEqual", ">=" },
            { "op_LessThanOrEqual", "<=" },
            { "op_MultiplicationAssignment", "*=" },
            { "op_SubtractionAssignment", "-=" },
            { "op_ExclusiveOrAssignment", "^=" },
            { "op_LeftShiftAssignment", "<<=" },
            { "op_ModulusAssignment", "%=" },
            { "op_AdditionAssignment", "+=" },
            { "op_BitwiseAndAssignment", "&=" },
            { "op_BitwiseOrAssignment", "|=" },
            { "op_Comma", "," },
            { "op_DivisionAssignment", "/=" },
            { "op_Decrement", "--" },
            { "op_Increment", "++" },
            { "op_UnaryNegation", "-" },
            { "op_UnaryPlus", "+" },
            { "op_OnesComplement", "~" },
        };

        private static readonly HashSet<char> illegalTypeFileNameCharacters = new HashSet<char>()
        {
            '<',
            '>',
            '?',
            ' ',
            ',',
            ':',
        };

        public static string CSharpName(this MemberInfo member, ActionDirection direction)
        {
            if (member is MethodInfo && ((MethodInfo)member).IsOperator())
            {
                return operators[member.Name] + " operator";
            }

            if (member is ConstructorInfo)
            {
                return "new " + member.DeclaringType.CSharpName();
            }

            if ((member is FieldInfo || member is PropertyInfo) && direction != ActionDirection.Any)
            {
                return $"{member.Name} ({direction.ToString().ToLower()})";
            }

            return member.Name;
        }

        public static string CSharpName(this Type type, bool includeGenericParameters = true, bool highlight = false)
        {
            return type.CSharpName(TypeQualifier.Name, includeGenericParameters, highlight);
        }

        public static string CSharpName(this Type type, Func<Type, bool> useFullName, bool includeGenericParameters = true, bool highlight = false)
        {
            return type.CSharpName(TypeQualifier.Name, useFullName, includeGenericParameters, highlight);
        }

        public static string CSharpFullName(this Type type, bool includeGenericParameters = true, bool highlight = false)
        {
            return type.CSharpName(TypeQualifier.Namespace, includeGenericParameters, highlight);
        }

        public static string CSharpFullName(this Type type, Func<Type, bool> useFullName, bool includeGenericParameters = true, bool highlight = false)
        {
            return type.CSharpName(TypeQualifier.Namespace, useFullName, includeGenericParameters, highlight);
        }

        public static string CSharpUniqueName(this Type type, bool includeGenericParameters = true)
        {
            return type.CSharpName(TypeQualifier.GlobalNamespace, includeGenericParameters);
        }

        public static string CSharpFileName(this Type type, bool includeNamespace, bool includeGenericParameters = false)
        {
            var fileName = type.CSharpName(includeNamespace ? TypeQualifier.Namespace : TypeQualifier.Name, includeGenericParameters);

            if (!includeGenericParameters && type.IsGenericType && fileName.Contains('<'))
            {
                fileName = fileName.Substring(0, fileName.IndexOf('<'));
            }

            fileName = fileName.ReplaceMultiple(illegalTypeFileNameCharacters, '_')
                .Trim('_')
                .RemoveConsecutiveCharacters('_');

            return fileName;
        }

        private static string CSharpName(this Type type, TypeQualifier qualifier, bool includeGenericParameters = true, bool highlight = false)
        {
            if (type == null) return "";

            if (primitives.ContainsKey(type))
            {
                return highlight ? primitives[type].ConstructHighlight() : primitives[type];
            }
            else if (type.IsGenericParameter)
            {
                return includeGenericParameters ? highlight ? type.Name.TypeHighlight() : type.Name : "";
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var nonNullable = Nullable.GetUnderlyingType(type);

                var underlyingName = nonNullable?.CSharpName(qualifier, includeGenericParameters, highlight) ?? (highlight ? "Unknown".ErrorHighlight() : "Unknown");

                return underlyingName + "?";
            }
            else if (type.IsArray)
            {
                var tempType = type.GetElementType();
                var arrayString = "[]";
                while (tempType.IsArray)
                {
                    tempType = tempType.GetElementType();
                    arrayString += "[]";
                }

                var tempTypeName = tempType.CSharpName(qualifier, includeGenericParameters, highlight);
                return tempTypeName + arrayString;
            }
            else
            {
                var name = type.Name;

                if (type.IsGenericType && name.Contains('`'))
                {
                    name = highlight ? name.Substring(0, name.IndexOf('`')).TypeHighlight() : name.Substring(0, name.IndexOf('`'));
                }
                else
                {
                    name = highlight ? name.TypeHighlight() : name;
                }

                var genericArguments = (IEnumerable<Type>)type.GetGenericArguments();

                if (type.IsNested)
                {
                    name = type.DeclaringType.CSharpName(qualifier, includeGenericParameters, highlight) + "." + (highlight ? name.TypeHighlight() : name);

                    if (type.DeclaringType.IsGenericType)
                    {
                        genericArguments = genericArguments.Skip(type.DeclaringType.GetGenericArguments().Length);
                    }
                }

                if (!type.IsNested)
                {
                    if (qualifier == TypeQualifier.Namespace || qualifier == TypeQualifier.GlobalNamespace)
                    {
                        name = (highlight ? type.Namespace.NamespaceHighlight() : type.Namespace) + "." + (highlight ? name.TypeHighlight() : name);
                    }

                    if (qualifier == TypeQualifier.GlobalNamespace)
                    {
                        name = (highlight ? "global::".ConstructHighlight() : "global::") + (highlight ? name.TypeHighlight() : name);
                    }
                }

                if (genericArguments.Any())
                {
                    name += "<";
                    name += string.Join(includeGenericParameters ? ", " : ",", genericArguments.Select(t => t.CSharpName(qualifier, includeGenericParameters, highlight)).ToArray());
                    name += ">";
                }

                return name;
            }
        }

        private static string CSharpName(this Type type, TypeQualifier qualifier, Func<Type, bool> resolver, bool includeGenericParameters = true, bool highlight = false)
        {
            if (type == null) return "";

            if (primitives.ContainsKey(type))
            {
                return highlight ? primitives[type].ConstructHighlight() : primitives[type];
            }
            else if (type.IsGenericParameter)
            {
                return includeGenericParameters ? highlight ? type.Name.TypeHighlight() : type.Name : "";
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var nonNullable = Nullable.GetUnderlyingType(type);

                var underlyingName = nonNullable?.CSharpName(qualifier, resolver, includeGenericParameters, highlight) ?? (highlight ? "Unknown".ErrorHighlight() : "Unknown");

                return underlyingName + "?";
            }
            else if (type.IsArray)
            {
                var tempType = type.GetElementType();
                var arrayString = "[]";
                while (tempType.IsArray)
                {
                    tempType = tempType.GetElementType();
                    arrayString += "[]";
                }

                var tempTypeName = tempType.CSharpName(qualifier, resolver, includeGenericParameters, highlight);
                return tempTypeName + arrayString;
            }
            else
            {
                var name = type.Name;

                if (type.IsGenericType && name.Contains('`'))
                {
                    name = highlight ? name.Substring(0, name.IndexOf('`')).TypeHighlight() : name.Substring(0, name.IndexOf('`'));
                }
                else
                {
                    name = highlight ? name.TypeHighlight() : name;
                }

                var genericArguments = (IEnumerable<Type>)type.GetGenericArguments();

                if (type.IsNested)
                {
                    name = type.DeclaringType.CSharpName(qualifier, resolver, includeGenericParameters, highlight) + "." + (highlight ? name.TypeHighlight() : name);

                    if (type.DeclaringType.IsGenericType)
                    {
                        genericArguments = genericArguments.Skip(type.DeclaringType.GetGenericArguments().Length);
                    }
                }

                if (!type.IsNested)
                {
                    bool fullName = resolver != null && resolver(type);
                    if ((qualifier == TypeQualifier.Namespace || qualifier == TypeQualifier.GlobalNamespace) && fullName)
                    {
                        name = (highlight ? type.Namespace.NamespaceHighlight() : type.Namespace) + "." + (highlight ? name.TypeHighlight() : name);
                    }

                    if (qualifier == TypeQualifier.GlobalNamespace)
                    {
                        name = (highlight ? "global::".ConstructHighlight() : "global::") + (highlight ? name.TypeHighlight() : name);
                    }
                }

                if (genericArguments.Any())
                {
                    name += "<";
                    name += string.Join(includeGenericParameters ? ", " : ",", genericArguments.Select(t => t.CSharpName(qualifier, resolver, includeGenericParameters, highlight)).ToArray());
                    name += ">";
                }

                return name;
            }
        }
    }
}