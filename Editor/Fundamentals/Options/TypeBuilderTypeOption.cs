using System;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    [FuzzyOption(typeof(TypeBuilderType))]
    public class TypeBuilderTypeOption : DocumentedOption<TypeBuilderType>
    {
        public TypeBuilderTypeOption(Type type)
        {
            value = new TypeBuilderType() { Type = type };

            label = type.DisplayName() +
                (type.IsGenericTypeDefinition
                    ? " (" + string.Join(", ",
                        type.GetGenericArguments()
                            .Select(t =>
                            {
                                if (t.IsGenericParameter)
                                {
                                    var constraints = t.GetGenericParameterConstraints()
                                        .Select(c => c.As().CSharpName(false, false, false))
                                        .ToArray();

                                    string constraintText = constraints.Length > 0 ? string.Join(" & ", constraints) : "object";
                                    return $"{t.As().CSharpName(false, false, false)} : {constraintText}";
                                }
                                else
                                {
                                    return t.As().CSharpName(false, false, false);
                                }
                            })
                            .Where(s => !string.IsNullOrEmpty(s))) + ")"
                    : "");

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
            string typeName = SearchUtility.HighlightQuery(type.DisplayName(), query);

            if (type.IsGenericTypeDefinition)
            {
                string genericParams = string.Join(", ",
                    type.GetGenericArguments()
                        .Select(t =>
                        {
                            if (t.IsGenericParameter)
                            {
                                var constraints = t.GetGenericParameterConstraints()
                                    .Select(c => c.As().CSharpName(false, false, false))
                                    .ToArray();

                                string constraintText = constraints.Length > 0 ? string.Join(" & ", constraints) : "object";
                                return $"{t.As().CSharpName(false, false, false)} : {constraintText}";
                            }
                            else
                            {
                                return t.As().CSharpName(false, false, false);
                            }
                        })
                        .Where(s => !string.IsNullOrEmpty(s)));

                typeName += $" ({genericParams})";
            }

            return $"{typeName} <color=#{ColorPalette.unityForegroundDim.ToHexString()}>(in {type.Namespace().DisplayName()})</color>";
        }
    }
}