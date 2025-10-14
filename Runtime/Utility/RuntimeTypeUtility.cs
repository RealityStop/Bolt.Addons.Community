using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;
using ParameterModifier = Unity.VisualScripting.Community.Libraries.CSharp.ParameterModifier;

namespace Unity.VisualScripting.Community
{
    public static class RuntimeTypeUtility
    {
        public static IEnumerable<Type> GetAllInterfacesRecursive(Type type)
        {
            var seen = new HashSet<Type>();
            void Add(Type t)
            {
                if (t == null) return;
                if (t.IsInterface) seen.Add(t);
                foreach (var i in t.GetInterfaces())
                {
                    if (seen.Add(i))
                    {
                        Add(i);
                    }
                }
                if (t.BaseType != null) Add(t.BaseType);
            }
            Add(type);
            return seen;
        }
        public static IEnumerable<FakeGenericParameterType> GetNestedFakeGenerics(Type type)
        {
            if (!type.IsGenericType && !(type is FakeGenericParameterType))
                yield break;
            var genericParameters = GetArrayBase(type).GetGenericArguments();
            foreach (var generic in genericParameters)
            {
                if (generic is FakeGenericParameterType fakeGenericParameterType)
                {
                    yield return fakeGenericParameterType;
                }
                else if (generic.IsGenericType)
                {
                    foreach (var nestedGeneric in GetNestedFakeGenerics(generic))
                    {
                        yield return nestedGeneric;
                    }
                }
            }
        }

        public static string GetEnumString<T>(this T @enum, Enum noneValue, string noneString = "None", string separator = ", ") where T : Enum
        {
            if (Convert.ToInt32(@enum) == Convert.ToInt32(noneValue))
                return noneString;

            var value = EnumUtility.ValuesByNames<T>();
            List<string> selected = new List<string>();

            foreach (var name in Enum.GetNames(typeof(T)))
            {
                if (!value[name].Equals(noneValue) && @enum.HasFlag(value[name]))
                    selected.Add(name);
            }

            return selected.Count > 0 ? string.Join(separator, selected) : noneString;
        }

        public static string GetModifierAsString(ParameterModifier modifiers)
        {
            List<string> selected = new List<string>();

            if (modifiers == ParameterModifier.None)
                return "";
            if ((modifiers & ParameterModifier.In) != 0)
                selected.Add("in".ConstructHighlight());
            if ((modifiers & ParameterModifier.Out) != 0)
                selected.Add("out".ConstructHighlight());
            if ((modifiers & ParameterModifier.Ref) != 0)
                selected.Add("ref".ConstructHighlight());
            if ((modifiers & ParameterModifier.Params) != 0)
                selected.Add("params".ConstructHighlight());
            if ((modifiers & ParameterModifier.This) != 0)
                selected.Add("this".ConstructHighlight());
            return string.Join(" ", selected) + (selected.Count > 0 ? " " : "");
        }

        public static string GetEnumString<T>(this T @enum, string separator = ", ") where T : Enum
        {
            var value = EnumUtility.ValuesByNames<T>();
            List<string> selected = new List<string>();
            foreach (var name in Enum.GetNames(typeof(T)))
            {
                if (@enum.HasFlag(value[name]))
                    selected.Add(name);
            }
            return string.Join(separator, selected);
        }

        /// <summary>
        /// Get the depth of a array for example string[][] would return 2
        /// </summary>
        public static int GetArrayDepth(Type type)
        {
            var depth = 0;
            while (type.IsArray || (type is FakeGenericParameterType fakeGenericParameterType && fakeGenericParameterType.IsArray))
            {
                type = type.GetElementType();
                depth++;
            }
            return depth;
        }

        /// <summary>
        /// Get the base type if its an array for example string[][] would return string.
        /// Will return the type inputed if not a array
        /// </summary>
        public static Type GetArrayBase(Type type)
        {
            if (type.IsArray)
            {
                var tempType = type.GetElementType();
                while (tempType.IsArray)
                {
                    tempType = tempType.GetElementType();
                }
                return tempType;
            }
            return type;
        }

        public static bool IsValidGenericConstraint(Type constraintType)
        {
            if (constraintType == null)
                return false;

            if (constraintType.IsStatic())
                return false;

            if (constraintType.IsArray)
                return false;

            if (constraintType.IsPrimitive)
                return false;

            if (constraintType.IsGenericTypeDefinition)
                return false;

            if (constraintType.IsInterface)
                return true;

            if (constraintType.IsClass && !constraintType.IsSealed)
                return true;

            if (constraintType.IsStruct())
                return false;

            if (constraintType == typeof(Enum) || constraintType == typeof(Delegate))
                return true;

            return false;
        }

        public static string GetNamespace(this Type type)
        {
            if (type == typeof(Libraries.CSharp.Void))
            {
                return typeof(void).Namespace;
            }
            return type.Namespace;
        }

        public static bool IsCastingRequired(Type sourceType, Type targetType, bool ignoreInputType)
        {
            bool isRequired = true;
            if (!ignoreInputType && targetType == typeof(object))
            {
                isRequired = false;
            }

            if (sourceType == targetType)
            {
                isRequired = false;
            }

            if (targetType.IsConvertibleTo(sourceType, true))
            {
                isRequired = false;
            }

            if (sourceType == typeof(object) && targetType != typeof(object))
            {
                isRequired = true;
            }

            return isRequired;
        }

        public static bool IsCastingPossible(Type sourceType, Type targetType)
        {
            if (targetType.IsAssignableFrom(sourceType))
            {
                return true;
            }

            if (targetType.IsInterface && targetType.IsAssignableFrom(sourceType))
            {
                return true;
            }

            if (IsNumericConversionCompatible(targetType, sourceType))
            {
                return true;
            }

            if (IsNullableConversionCompatible(sourceType, targetType))
            {
                return true;
            }

            return targetType.IsConvertibleTo(sourceType, true);
        }

        public static bool IsNumericConversionCompatible(Type targetType, Type sourceType)
        {
            Type[] numericTypes = { typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal) };

            if (Array.Exists(numericTypes, t => t == targetType) &&
                Array.Exists(numericTypes, t => t == sourceType))
            {
                return true;
            }

            return false;
        }

        public static bool IsNullableConversionCompatible(Type sourceType, Type targetType)
        {
            if (Nullable.GetUnderlyingType(targetType) != null)
            {
                Type underlyingTargetType = Nullable.GetUnderlyingType(targetType);
                return underlyingTargetType.IsAssignableFrom(sourceType);
            }

            if (Nullable.GetUnderlyingType(sourceType) != null)
            {
                Type underlyingSourceType = Nullable.GetUnderlyingType(sourceType);
                return targetType.IsAssignableFrom(underlyingSourceType);
            }
            return false;
        }

    }
}