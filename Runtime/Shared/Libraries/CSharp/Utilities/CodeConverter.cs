using Unity.VisualScripting.Community.Libraries.Humility;
using System;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    public static class CodeConverter
    {
        public static string AsString(this AccessModifier scope)
        {
            if (scope == AccessModifier.None) return string.Empty;
            var output = EnumToLower(scope);

            if (scope == AccessModifier.PrivateProtected || scope == AccessModifier.ProtectedInternal) output = output.Add().Space().Between().Lowercase().And().Uppercase();

            return EnumToLower(scope);
        }

        public static string AsString(this RootAccessModifier scope)
        {
            return EnumToLower(scope);
        }

        public static readonly Dictionary<FieldModifier, FieldModifier[]> fieldModifierConflicts = new Dictionary<FieldModifier, FieldModifier[]>
        {
            { FieldModifier.Constant, new FieldModifier[] { FieldModifier.Static, FieldModifier.Readonly, FieldModifier.Volatile, FieldModifier.Unsafe } },
            { FieldModifier.Static,   new FieldModifier[] { FieldModifier.Constant } },
            { FieldModifier.Readonly, new FieldModifier[] { FieldModifier.Constant, FieldModifier.Volatile } },
            { FieldModifier.Volatile, new FieldModifier[] { FieldModifier.Constant, FieldModifier.Readonly } },
            { FieldModifier.Unsafe,   new FieldModifier[] { FieldModifier.Constant } },
            { FieldModifier.New, new FieldModifier[0] },
            { FieldModifier.None, new FieldModifier[0] }
        };

        public static readonly Dictionary<PropertyModifier, PropertyModifier[]> propertyModifierConflicts = new Dictionary<PropertyModifier, PropertyModifier[]>()
        {
            { PropertyModifier.Abstract, new[] { PropertyModifier.Static, PropertyModifier.Sealed, PropertyModifier.Override, PropertyModifier.Unsafe, PropertyModifier.Volatile } },
            { PropertyModifier.Override, new[] { PropertyModifier.Static, PropertyModifier.Sealed, PropertyModifier.Abstract, PropertyModifier.Unsafe, PropertyModifier.Volatile } },
            { PropertyModifier.Sealed,   new[] { PropertyModifier.Static, PropertyModifier.Abstract, PropertyModifier.Unsafe, PropertyModifier.Volatile } },
            { PropertyModifier.Static,   new[] { PropertyModifier.Abstract, PropertyModifier.Override, PropertyModifier.Sealed, PropertyModifier.Volatile } },
            { PropertyModifier.Unsafe,   new[] { PropertyModifier.Abstract, PropertyModifier.Override, PropertyModifier.Sealed, PropertyModifier.Volatile } },
            { PropertyModifier.Volatile, new[] { PropertyModifier.Abstract, PropertyModifier.Override, PropertyModifier.Sealed, PropertyModifier.Static, PropertyModifier.Unsafe } },
            { PropertyModifier.New, Array.Empty<PropertyModifier>() },
            { PropertyModifier.None, Array.Empty<PropertyModifier>() },
        };

        public static readonly Dictionary<ParameterModifier, ParameterModifier[]> parameterModifierConflicts = new Dictionary<ParameterModifier, ParameterModifier[]>
        {
            { ParameterModifier.In, new ParameterModifier[] { ParameterModifier.Out, ParameterModifier.Ref, ParameterModifier.Params, ParameterModifier.This } },
            { ParameterModifier.Out, new ParameterModifier[] { ParameterModifier.In, ParameterModifier.Ref, ParameterModifier.Params, ParameterModifier.This } },
            { ParameterModifier.Ref, new ParameterModifier[] { ParameterModifier.In, ParameterModifier.Out, ParameterModifier.Params } },
            { ParameterModifier.Params, new ParameterModifier[] { ParameterModifier.In, ParameterModifier.Out, ParameterModifier.Ref, ParameterModifier.This } },
            { ParameterModifier.This, new ParameterModifier[] { ParameterModifier.In, ParameterModifier.Out, ParameterModifier.Params } },
            { ParameterModifier.None, Array.Empty<ParameterModifier>() }
        };

        public static readonly Dictionary<MethodModifier, MethodModifier[]> methodModifierConflicts = new Dictionary<MethodModifier, MethodModifier[]>
        {
            { MethodModifier.Abstract, new MethodModifier[] { MethodModifier.Static, MethodModifier.Sealed, MethodModifier.Override, MethodModifier.Extern, MethodModifier.Virtual, MethodModifier.Async } },
            { MethodModifier.Virtual,  new MethodModifier[] { MethodModifier.Static, MethodModifier.Sealed, MethodModifier.Override, MethodModifier.Extern, MethodModifier.Abstract, MethodModifier.Async } },
            { MethodModifier.Override, new MethodModifier[] { MethodModifier.Static, MethodModifier.Sealed, MethodModifier.Virtual, MethodModifier.Extern, MethodModifier.Abstract } },
            { MethodModifier.Sealed,   new MethodModifier[] { MethodModifier.Static, MethodModifier.Virtual, MethodModifier.Override, MethodModifier.Extern, MethodModifier.Abstract } },
            { MethodModifier.Static,   new MethodModifier[] { MethodModifier.Abstract, MethodModifier.Virtual, MethodModifier.Override, MethodModifier.Sealed } },
            { MethodModifier.Extern,   new MethodModifier[] { MethodModifier.Abstract, MethodModifier.Virtual, MethodModifier.Override, MethodModifier.Sealed } },
            { MethodModifier.Async,    new MethodModifier[] { MethodModifier.Abstract } },
            { MethodModifier.Unsafe,   new MethodModifier[0] },
            { MethodModifier.None,     new MethodModifier[0] },
        };

        public static string AsString(this FieldModifier modifier)
        {
            if (modifier == FieldModifier.None) return string.Empty;
            return GetCSharpString(modifier);
        }

        private static string GetCSharpString<T>(T value) where T : Enum
        {
            var result = new List<string>();

            foreach (T flag in Enum.GetValues(typeof(T)))
            {
                if (Convert.ToInt64(flag) == 0) continue;
                if (value.HasFlag(flag))
                {
                    result.Add(EnumToLower(flag));
                }
            }

            return string.Join(" ", result);
        }


        public static string AsString(this ParameterModifier modifier)
        {
            if (modifier == ParameterModifier.None) return string.Empty;
            return GetCSharpString(modifier);
        }

        public static string AsString(this PropertyModifier modifier)
        {
            if (modifier == PropertyModifier.None) return string.Empty;
            return GetCSharpString(modifier);
        }

        public static string AsString(this ConstructorModifier modifier)
        {
            if (modifier == ConstructorModifier.None) return string.Empty;
            return EnumToLower(modifier);
        }

        public static string AsString(this MethodModifier modifier)
        {
            if (modifier == MethodModifier.None) return string.Empty;
            return GetCSharpString(modifier);
        }

        public static string AsString(this ClassModifier modifier)
        {
            if (modifier == ClassModifier.None) return string.Empty;
            return GetCSharpString(modifier);
        }

        public static string AsString(this StructModifier modifier)
        {
            if (modifier == StructModifier.None) return string.Empty;
            return EnumToLower(modifier);
        }

        private static string EnumToLower(Enum @enum)
        {
            return @enum.ToString().Add().Space().Between().Lowercase().And().Uppercase().ToLower().Replace("constant", "const");
        }
    }
}