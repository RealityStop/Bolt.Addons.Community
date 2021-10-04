using Unity.VisualScripting.Community.Libraries.CSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

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

        public static T New<T>(this HUMType.Data.As @as)
        {
            return (T)HUMType.New(@as.type);
        }

        /// <summary>
        /// Converts a type to its actual declared csharp name. Custom types return the type name. Example 'System.Int32' becomes 'int'.
        /// </summary>
        public static string CSharpName(this HUMType.Data.As @as, bool hideSystemObject = false, bool fullName = false, bool highlight = true)
        {
            if (highlight) return @as.HighlightCSharpName(hideSystemObject, fullName);
            if (@as.type == null) return "null";
            if (@as.type == typeof(CSharp.Void)) return "void";
            if (@as.type.IsConstructedGenericType || @as.type.IsGenericType) return GenericDeclaration(@as.type);
            if (@as.type == typeof(int)) return "int";
            if (@as.type == typeof(string)) return "string";
            if (@as.type == typeof(float)) return "float";
            if (@as.type == typeof(void)) return "void";
            if (@as.type == typeof(double)) return "double";
            if (@as.type == typeof(bool)) return "bool";
            if (@as.type == typeof(byte)) return "byte";
            if (@as.type == typeof(void)) return "void";
            if (@as.type == typeof(object) && @as.type.BaseType == null) return hideSystemObject ? string.Empty : "object";
            if (@as.type == typeof(object[])) return "object[]";

            return fullName ? @as.type.FullName : @as.type.Name;
        }

        private static string HighlightCSharpName(this HUMType.Data.As @as, bool hideSystemObject = false, bool fullName = false)
        {
            if (@as.type == null) return "null".ConstructHighlight();
            if (@as.type == typeof(CSharp.Void)) return "void".ConstructHighlight();
            if (@as.type.IsConstructedGenericType || @as.type.IsGenericType) return GenericDeclaration(@as.type);
            if (@as.type == typeof(int)) return "int".ConstructHighlight();
            if (@as.type == typeof(string)) return "string".ConstructHighlight();
            if (@as.type == typeof(float)) return "float".ConstructHighlight();
            if (@as.type == typeof(void)) return "void".ConstructHighlight();
            if (@as.type == typeof(double)) return "double".ConstructHighlight();
            if (@as.type == typeof(bool)) return "bool".ConstructHighlight();
            if (@as.type == typeof(byte)) return "byte".ConstructHighlight();
            if (@as.type == typeof(void)) return "void".ConstructHighlight();
            if (@as.type.IsEnum) return @as.type.Name.EnumHighlight();
            if (@as.type.IsInterface) return @as.type.Name.InterfaceHighlight();
            if (@as.type == typeof(System.Object) && @as.type.BaseType == null) return hideSystemObject ? string.Empty : "object".ConstructHighlight();
            if (@as.type == typeof(object[])) return "object".ConstructHighlight() + "[]";

            return fullName ? @as.type.FullName.Replace(@as.type.Name, @as.type.Name.TypeHighlight()) : @as.type.Name.TypeHighlight();
        }

        /// <summary>
        /// Converts a string that was retrieved via type.Name to its actual declared csharp name. Custom types return the type name. Example 'Int32' becomes 'int'.
        /// </summary>
        public static string CSharpName(this HUMString.Data.As asData, bool hideSystemObject = false, HighlightType highlightType = HighlightType.None)
        {
            if (string.IsNullOrEmpty(asData.text) || string.IsNullOrWhiteSpace(asData.text)) return "null";
            if (asData.text == "Int32") return "int";
            if (asData.text == "String") return "string";
            if (asData.text == "Float") return "float";
            if (asData.text == "Void") return "void";
            if (asData.text == "Double") return "double";
            if (asData.text == "Bool" || asData.text == "Boolean") return "bool";
            if (asData.text == "Byte") return "byte";
            if (asData.text == "Object") return hideSystemObject ? string.Empty : "object";
            return asData.text.WithHighlight(highlightType);
        }

        public enum HighlightType
        {
            None,
            Enum, 
            Interface,
            Type,
            Construct,
            Comment
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
            return isData.type == typeof(void);
        }

        public static bool NullOrVoid(this HUMType.Data.Is isData)
        {
            return isData.type == typeof(void) || isData.type == null;
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
        /// Finds all types with this attribute.
        /// </summary>
        public static Type[] Attribute<TAttribute>(this HUMType.Data.With with) where TAttribute : Attribute
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
                        attrib.Is().NotNull(() => { result.Add(types[type]); });
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
        /// Converts a value into code form. Example: a float value of '10' would be '10f'. A string would add qoutes, ect.
        /// </summary>
        public static string Code(this HUMValue.Data.As @as, bool isNew, bool isLiteral = false, bool highlight = true, string parameters = "", bool newLineLiteral = false)
        {
            if (highlight) return HighlightedCode(@as, isNew, isLiteral, parameters, newLineLiteral);
            Type type = @as.value?.GetType();
            if (@as.value is Type) return "typeof(" + ((Type)@as.value).As().CSharpName() + ")";
            if (type == null) return "null";
            if (type == typeof(void)) return "void";
            if (type == typeof(bool)) return @as.value.ToString().ToLower();
            if (type == typeof(float)) return @as.value.ToString() + "f";
            if (type == typeof(string)) return @"""" + @as.value.ToString() + @"""";
            if (type == typeof(UnityEngine.GameObject)) return "null";
            if (type == typeof(int) || type == typeof(uint) || type == typeof(byte) || type == typeof(long) || type == typeof(short) || type == typeof(double)) return @as.value.ToString();
            if (type.IsEnum) return type.Name + "." + @as.value.ToString();

            if (isNew)
            {
                if (type.IsClass || !type.IsClass && !type.IsInterface && !type.IsEnum)
                {
                    if (type.IsConstructedGenericType) return isLiteral ? Literal(@as.value, newLineLiteral) : "new " + GenericDeclaration(type) + "(" + parameters + ")";
                    return isLiteral ? Literal(@as.value, newLineLiteral) : "new " + type.Name +  "(" + parameters + ")";
                }
                else
                {
                    if (type.IsValueType && !type.IsEnum && !type.IsPrimitive) return isLiteral ? Literal(@as.value, newLineLiteral) : "new " + type.Name + "(" + parameters + ")";
                }
            }

            return @as.value.ToString();
        }

        private static string HighlightedCode(this HUMValue.Data.As @as, bool isNew, bool isLiteral = false, string parameters = "", bool newLineLiteral = false)
        {
            Type type = @as.value?.GetType();
            if (@as.value is Type) return "typeof".ConstructHighlight() + "(" + ((Type)@as.value).As().CSharpName().TypeHighlight() + ")";
            if (type == null) return "null".ConstructHighlight();
            if (type == typeof(void)) return "void".ConstructHighlight();
            if (type == typeof(bool)) return @as.value.ToString().ToLower().ConstructHighlight();
            if (type == typeof(float)) return (@as.value.ToString() + "f").NumericHighlight();
            if (type == typeof(string)) return (@"""" + @as.value.ToString() + @"""").StringHighlight();
            if (type == typeof(UnityEngine.GameObject)) return "null".ConstructHighlight();
            if (type == typeof(int) || type == typeof(uint) || type == typeof(byte) || type == typeof(long) || type == typeof(short) || type == typeof(double)) return @as.value.ToString().NumericHighlight();
            if (type.IsEnum) return type.Name.EnumHighlight() + "." + @as.value.ToString();
            if (isNew)
            {
                if (type.IsClass || !type.IsClass && !type.IsInterface && !type.IsEnum)
                {
                    if (type.IsConstructedGenericType) return isLiteral ? Literal(@as.value, newLineLiteral) : "new ".ConstructHighlight() + GenericDeclaration(type) +"(" + parameters + ")";
                    return isLiteral ? Literal(@as.value, newLineLiteral) : "new ".ConstructHighlight() + type.Name.TypeHighlight() + "(" + parameters + ")";
                }
                else
                {
                    if (type.IsValueType && !type.IsEnum && !type.IsPrimitive) return isLiteral? Literal(@as.value, newLineLiteral) : "new ".ConstructHighlight() + type.Name.TypeHighlight() + "(" + parameters + ")";
                }
            }

            return @as.value.ToString();
        }

        private static string Literal(object value, bool newLine = false)
        {
            var fields = value?.GetType().GetFields();
            var output = string.Empty;
            var usableFields = new List<FieldInfo>();
            var isMultiLine = fields.Length > 2;

            output += (newLine ? "\n" : string.Empty) + "new ".ConstructHighlight() + value.GetType().Name.TypeHighlight() + "()" + (isMultiLine ? "\n" + "{" + "\n" : " { ");

            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i].IsPublic && !fields[i].IsStatic && !fields[i].IsInitOnly)
                {
                    usableFields.Add(fields[i]);
                }
            }

            for (int i = 0; i < usableFields.Count; i++)
            {
                output += (isMultiLine ? CodeBuilder.Indent(1) : string.Empty) + usableFields[i].Name + " = " + usableFields[i].GetValue(value).As().Code(true);
                output += i < usableFields.Count - 1 ? ", " + (isMultiLine ? "\n" : string.Empty) : string.Empty;
            }

            output += isMultiLine ? "\n" + "}" : " } ";

            return output;
        }

        public static string GenericDeclaration(Type type, params Type[] declaredGenerics)
        {
            var output = string.Empty;

            if (!type.IsConstructedGenericType && !type.IsGenericType) throw new Exception("Type is not a generic type but you are trying to declare a generic.");
            output += type.Name.Contains("`") ? type.Name.Remove(type.Name.IndexOf("`"), type.Name.Length - type.Name.IndexOf("`")).TypeHighlight() : type.Name.TypeHighlight();
            output += "<";

            var args = type.GetGenericArguments();

            for (int i = 0; i < args.Length; i++)
            {
                output += args[i].As().CSharpName();
                if (i < args.Length - 1) output += ", ";
            }

            output += ">";

            return output;
        }
    }
}