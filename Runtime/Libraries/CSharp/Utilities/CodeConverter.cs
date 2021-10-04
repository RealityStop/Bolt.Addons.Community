using Unity.VisualScripting.Community.Libraries.Humility;
using System;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    public static class CodeConverter
    {
        public static string AsString(this AccessModifier scope)
        {
            var output = EnumToLower(scope);

            if (scope == AccessModifier.PrivateProtected || scope == AccessModifier.ProtectedInternal) output = output.Add().Space().Between().Lowercase().And().Uppercase();

            return EnumToLower(scope);
        }

        public static string AsString(this RootAccessModifier scope)
        {
            return EnumToLower(scope);
        }

        public static string AsString(this FieldModifier modifier)
        {
            if (modifier == FieldModifier.None) return string.Empty;
            return EnumToLower(modifier);
        }

        public static string AsString(this ParameterModifier modifier)
        {
            if (modifier == ParameterModifier.None) return string.Empty;
            return EnumToLower(modifier);
        }

        public static string AsString(this PropertyModifier modifier)
        {
            if (modifier == PropertyModifier.None) return string.Empty;
            return EnumToLower(modifier);
        }

        public static string AsString(this ConstructorModifier modifier)
        {
            if (modifier == ConstructorModifier.None) return string.Empty;
            return EnumToLower(modifier);
        }

        public static string AsString(this MethodModifier modifier)
        {
            if (modifier == MethodModifier.None) return string.Empty;
            return EnumToLower(modifier);
        }

        public static string AsString(this ClassModifier modifier)
        {
            if (modifier == ClassModifier.None) return string.Empty;
            return EnumToLower(modifier);
        }

        public static string AsString(this StructModifier modifier)
        {
            if (modifier == StructModifier.None) return string.Empty;
            return EnumToLower(modifier);
        }

        private static string EnumToLower(Enum @enum)
        {
            return @enum.ToString().Add().Space().Between().Lowercase().And().Uppercase().ToLower();
        }
    }
}