using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    public static class TypeConversionUtility
    {
        private static readonly Type[] NumericTypes =
        {
            typeof(byte), typeof(sbyte), typeof(short), typeof(ushort),
            typeof(int), typeof(uint), typeof(long), typeof(ulong),
            typeof(float), typeof(double), typeof(decimal)
        };

        private static readonly Dictionary<(Type from, Type to), bool> _implicitCache = new Dictionary<(Type from, Type to), bool>();
        private static readonly Dictionary<(Type from, Type to), bool> _explicitCache = new Dictionary<(Type from, Type to), bool>();

        /// <summary>
        /// Determines if C# allows an implicit conversion between two types.
        /// </summary>
        public static bool IsImplicitlyConvertible(Type from, Type to)
        {
            if (from == null || to == null)
                return false;

            var key = (from, to);
            if (_implicitCache.TryGetValue(key, out bool result))
                return result;

            // Direct assignability
            if (to.IsAssignableFrom(from))
                return _implicitCache[key] = true;

            // Handle numeric implicit conversions
            if (ConversionUtility.HasImplicitNumericConversion(from, to))
            {
                return _implicitCache[key] = true;
            }

            // Nullable conversions (T -> T?)
            if (Nullable.GetUnderlyingType(to) == from)
                return _implicitCache[key] = true;

            // Enums
            if (from.IsEnum && Enum.GetUnderlyingType(from) == to)
                return _implicitCache[key] = true;
            if (to.IsEnum && Enum.GetUnderlyingType(to) == from)
                return _implicitCache[key] = true;

            // Check for implicit operators
            if (HasConversionOperator("op_Implicit", from, to))
                return _implicitCache[key] = true;

            return _implicitCache[key] = false;
        }

        /// <summary>
        /// Determines if a valid explicit cast exists (e.g. (int)float).
        /// </summary>
        public static bool IsExplicitlyConvertible(Type from, Type to)
        {
            if (from == null || to == null)
                return false;

            var key = (from, to);
            if (_explicitCache.TryGetValue(key, out bool result))
                return result;

            if (ConversionUtility.HasExplicitNumericConversion(from, to))
                return _explicitCache[key] = true;

            // Implicit covers explicit as well
            if (IsImplicitlyConvertible(from, to))
                return _explicitCache[key] = true;

            if ((from.IsEnum && NumericTypes.Contains(to)) ||
                (to.IsEnum && NumericTypes.Contains(from)))
                return _explicitCache[key] = true;

            // Nullable <-> underlying
            if (Nullable.GetUnderlyingType(from) == to || Nullable.GetUnderlyingType(to) == from)
                return _explicitCache[key] = true;

            // Explicit operator
            if (HasConversionOperator("op_Explicit", from, to))
                return _explicitCache[key] = true;

            // Handle UnityEngine.Object and null
            if (typeof(UnityEngine.Object).IsAssignableFrom(to) && from == typeof(object))
                return _explicitCache[key] = true;

            return _explicitCache[key] = false;
        }

        private static bool HasConversionOperator(string name, Type from, Type to)
        {
            var methods = from.GetMethods(BindingFlags.Public | BindingFlags.Static)
                          .Concat(to.GetMethods(BindingFlags.Public | BindingFlags.Static));
            return methods.Any(m => m.Name == name && m.ReturnType == to &&
                                    m.GetParameters().Length == 1 &&
                                    m.GetParameters()[0].ParameterType == from);
        }

        /// <summary>
        /// Generates the proper C# cast syntax when needed.
        /// </summary>
        public static string CastTo(string code, Type from, Type to)
        {
            if (to == null || from == null || from == to)
                return code;

            if (IsImplicitlyConvertible(from, to))
                return code;

            if (IsExplicitlyConvertible(from, to))
            {
                return $"({to.As().CSharpName(false, true)}{code}";
            }

            return code;
        }

        /// <summary>
        /// Determines whether a cast is both required and valid between two types.
        /// </summary>
        public static bool ShouldCast(Type sourceType, Type targetType)
        {
            if (sourceType == null || targetType == null)
                return false;

            if (sourceType == targetType)
                return false;

            // Target is object > any type can go into it
            if (targetType == typeof(object))
                return false;

            // Source is object > cast is needed unless target is also object
            if (sourceType == typeof(object))
                return true;

            if (sourceType == typeof(void) || targetType == typeof(void))
                return false;

            // If target is assignable from source > no cast required
            if (targetType.IsAssignableFrom(sourceType))
                return false;

            // If both are numeric and conversion is valid > cast required (e.g., int > float)
            if (sourceType.IsNumeric() && targetType.IsNumeric() && !ConversionUtility.HasImplicitNumericConversion(sourceType, targetType))
                return IsCastingPossible(sourceType, targetType);

            // If nullable conversion is valid > cast required
            if (IsNullableConversion(sourceType, targetType))
                return IsCastingPossible(sourceType, targetType);

            // If we can safely cast via reference or boxing/unboxing > cast required
            if (IsCastingPossible(sourceType, targetType))
                return true;

            // Otherwise > no cast
            return false;
        }

        /// <summary>
        /// Determines if a cast is possible at runtime (without compile-time error).
        /// </summary>
        public static bool IsCastingPossible(Type sourceType, Type targetType)
        {
            try
            {
                if (targetType.IsAssignableFrom(sourceType))
                    return true;

                if (IsExplicitlyConvertible(sourceType, targetType))
                    return true;

                if (ConversionUtility.HasExplicitNumericConversion(sourceType, targetType))
                    return true;

                if (IsNullableConversion(sourceType, targetType))
                    return true;
            }
            catch
            {
                return false;
            }

            return false;
        }

        /// <summary>
        /// Checks if the types are nullable-compatible.
        /// </summary>
        public static bool IsNullableConversion(Type sourceType, Type targetType)
        {
            Type sourceUnderlying = Nullable.GetUnderlyingType(sourceType);
            Type targetUnderlying = Nullable.GetUnderlyingType(targetType);

            if (sourceUnderlying != null && targetUnderlying != null)
                return sourceUnderlying == targetUnderlying;

            if (sourceUnderlying != null)
                return targetType.IsStrictlyAssignableFrom(sourceUnderlying);

            if (targetUnderlying != null)
                return targetUnderlying.IsStrictlyAssignableFrom(sourceType);

            return false;
        }
    }
}