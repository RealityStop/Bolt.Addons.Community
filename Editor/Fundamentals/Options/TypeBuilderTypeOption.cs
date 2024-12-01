using System;

namespace Unity.VisualScripting.Community
{
    [FuzzyOption(typeof(TypeBuilderType))]
    public class TypeBuilderTypeOption : DocumentedOption<TypeBuilderType>
    {
        public TypeBuilderTypeOption(Type type)
        {
            value = new TypeBuilderType() { Type = type };
            label = type.DisplayName();
            UnityAPI.Async(() => icon = type.Icon());
            documentation = type.Documentation();
            zoom = true;
        }

        public TypeBuilderTypeOption(Type type, bool parentOnly) : this(type)
        {
            this.parentOnly = parentOnly;
        }

        public static string Haystack(Type type)
        {
            return type.DisplayName();
        }

        public static string SearchResultLabel(Type type, string query)
        {
            return $"{SearchUtility.HighlightQuery(type.DisplayName(), query)} <color=#{ColorPalette.unityForegroundDim.ToHexString()}>(in {type.Namespace().DisplayName()})</color>";
        }
    }
}