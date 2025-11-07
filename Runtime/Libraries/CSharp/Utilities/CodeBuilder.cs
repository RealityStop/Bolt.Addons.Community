using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using Unity.VisualScripting.Community.Libraries.Humility;
using System.Linq;
using Unity.VisualScripting.Community.Utility;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    /// <summary>
    /// Gives you access to utilities methods to more easily build custom code.
    /// </summary>
    public static class CodeBuilder
    {
        #region Colors
        public static string EnumColor = "FFFFBB";
        public static string ConstructColor = "4488FF";
        public static string WarningColor = "CC3333";
        public static string InterfaceColor = "DDFFBB";
        public static string TypeColor = "33EEAA";
        public static string StringColor = "CC8833";
        public static string NumericColor = "DDFFBB";
        public static string CommentColor = "009900";
        public static string SummaryColor = "00CC00";
        public static string VariableColor = "00FFFF";
        public static string RecommendationColor = "FFD700";
        /// <summary>
        /// Unused
        /// </summary>
        public static string MethodColor = "EBEB5B";
        #endregion

        public static int currentIndent { get; private set; }
        /// <summary>
        /// Creates the opening of a new body as a string.
        /// </summary>
        public static string OpenBody(int indent)
        {
            var output = string.Empty;

            output += Indent(indent) + "{";

            return output;
        }

        /// <summary>
        /// Creates the opening of a new body as a string with custom indent spacing.
        /// </summary>
        public static string OpenBody(int indent, int spaces)
        {
            var output = string.Empty;

            output += Indent(indent) + "{";

            for (int i = 0; i < spaces; i++)
            {
                output += "\n";
            }

            return output;
        }

        /// <summary>
        /// Creates the closing of a body as a string.
        /// </summary>
        public static string CloseBody(int indent)
        {
            var output = string.Empty;

            output += Indent(indent) + "}";

            return output;
        }

        /// <summary>
        /// Creates the closing of a body as a string with custom indent spacing.
        /// </summary>
        public static string CloseBody(int indent, int spaces)
        {
            var output = string.Empty;

            output += Indent(indent) + "}";

            for (int i = 0; i < spaces; i++)
            {
                output += "\n";
            }

            return output;
        }

        public static string ToMultipleEnumString(this Enum value, bool highlight, string separator = ", ", bool fullName = false)
        {
            Type type = value.GetType();
            string typeString = type.As().CSharpName(false, fullName, highlight);

            if (!type.IsDefined(typeof(FlagsAttribute), false))
                return highlight ? typeString + "." + value.ToString().EnumHighlight()
                                 : typeString + "." + value.ToString();

            List<string> values = new List<string>();
            foreach (Enum enumValue in Enum.GetValues(type))
            {
                if (enumValue.Equals(Enum.ToObject(type, 0))) continue;

                if (value.HasFlag(enumValue))
                    values.Add(highlight
                        ? $"{typeString}.{enumValue.ToString().EnumHighlight()}"
                        : $"{typeString}.{enumValue}");
            }

            if (values.Count > 0)
                return string.Join(separator, values);

            Enum zeroValue = (Enum)Enum.GetValues(type).GetValue(0);
            return highlight
                ? $"{typeString}.{zeroValue.ToString().EnumHighlight()}"
                : $"{typeString}.{zeroValue}";
        }

        private static readonly Dictionary<int, string> indentCache = new Dictionary<int, string>();

        /// <summary>
        /// Creates an indentation. The spacing is equal to 4 whitespaces.
        /// </summary>
        public static string Indent(int amount)
        {
            currentIndent = amount;
            if (indentCache.TryGetValue(amount, out var indent))
                return indent;

            indent = amount <= 0 ? "" : new string(' ', amount * 4);
            indentCache[amount] = indent;

            return indent;
        }

        private static readonly Dictionary<(int, int), string> customIndentCache = new Dictionary<(int, int), string>();

        /// <summary>
        /// Creates an indentation with a custom amount of whitespaces per indent.
        /// </summary>
        public static string Indent(int amount, int spacing)
        {
            currentIndent = amount;
            if (customIndentCache.TryGetValue((amount, spacing), out var indent))
                return indent;

            indent = amount <= 0 || spacing <= 0 ? "" : new string(' ', amount * spacing);

            customIndentCache[(amount, spacing)] = indent;

            return indent;
        }

        /// <summary>
        /// Creates an indentation with the current indent amount.
        /// </summary>
        public static string GetCurrentIndent()
        {
            var output = Indent(currentIndent);

            return output;
        }

        /// <summary>
        /// Creates an indentation with the current indent + addAmount.
        /// </summary>
        public static string GetCurrentIndent(int addAmount)
        {
            var output = Indent(currentIndent + addAmount);

            return output;
        }

        /// <summary>
        /// Creates a series of using statements for namespace access.
        /// </summary>
        public static string Using(string[] namespaces)
        {
            var output = string.Empty;

            for (int i = 0; i < namespaces.Length; i++)
            {
                output += "using ".ConstructHighlight() + namespaces[i] + ";" + (i < namespaces.Length - 1 ? "\n" : string.Empty);
            }

            return output;
        }

        public static string Using(List<string> namespaces)
        {
            var _namespaces = namespaces.ToArray();
            return Using(_namespaces);
        }

        public static string Using(this ClassGenerator generator)
        {
            var usings = new List<string>();
            for (int i = 0; i < generator.fields.Count; i++)
            {
                var @namespace = generator.fields[i].type.Namespace;
                if (!usings.Contains(@namespace)) usings.Add(@namespace);
            }

            for (int i = 0; i < generator.properties.Count; i++)
            {
                var @namespace = generator.properties[i].returnType.Namespace;
                if (!usings.Contains(@namespace)) usings.Add(@namespace);
            }

            for (int i = 0; i < generator.methods.Count; i++)
            {
                var @namespace = generator.methods[i].returnType.Namespace;
                if (!usings.Contains(@namespace)) usings.Add(@namespace);
            }

            var output = string.Empty;

            for (int i = 0; i < usings.Count; i++)
            {
                output += "using ".ConstructHighlight() + usings[i] + ";" + (i < usings.Count - 1 ? "\n" : string.Empty);
            }

            return output;
        }

        public static List<string> Usings(this ClassGenerator generator)
        {
            var usings = new List<string>();

            for (int i = 0; i < generator.attributes.Count; i++)
            {
                usings.MergeUnique(generator.attributes[i].Usings());
            }

            for (int i = 0; i < generator.fields.Count; i++)
            {
                usings.MergeUnique(generator.fields[i].Usings());
                for (int attrIndex = 0; attrIndex < generator.fields[i].attributes.Count; attrIndex++)
                {
                    usings.MergeUnique(generator.fields[i].attributes[attrIndex].Usings());
                }
            }

            for (int i = 0; i < generator.properties.Count; i++)
            {
                usings.MergeUnique(generator.properties[i].Usings());
                for (int attrIndex = 0; attrIndex < generator.properties[i].attributes.Count; attrIndex++)
                {
                    usings.MergeUnique(generator.properties[i].attributes[attrIndex].Usings());
                }
            }

            for (int i = 0; i < generator.methods.Count; i++)
            {
                usings.MergeUnique(generator.methods[i].Usings());
                for (int attrIndex = 0; attrIndex < generator.methods[i].attributes.Count; attrIndex++)
                {
                    usings.MergeUnique(generator.methods[i].attributes[attrIndex].Usings());
                }
                for (int paramIndex = 0; paramIndex < generator.methods[i].parameters.Count; paramIndex++)
                {
                    usings.MergeUnique(generator.methods[i].parameters[paramIndex].Usings());
                    List<string> parameterAttributeNamespaces = new List<string>();
                    foreach (var attribute in generator.methods[i].parameters[paramIndex].attributes)
                    {
                        parameterAttributeNamespaces.Add(attribute.GetAttributeType().Namespace);
                    }
                    usings.MergeUnique(parameterAttributeNamespaces);
                }
            }

            return usings;
        }

        public static bool IsMoreRestrictive(this AccessModifier scope, AccessModifier than)
        {
            var accessLevels = new Dictionary<AccessModifier, int>
            {
                { AccessModifier.Public, 0 },
                { AccessModifier.ProtectedInternal, 1 },
                { AccessModifier.Internal, 2 },
                { AccessModifier.Protected, 3 },
                { AccessModifier.PrivateProtected, 4 },
                { AccessModifier.Private, 5 }
            };

            return accessLevels[scope] > accessLevels[than];
        }

        public static AccessModifier GetMoreRestrictive(this AccessModifier scope1, AccessModifier scope2)
        {
            if (scope1.IsMoreRestrictive(scope2))
            {
                return scope1;

            }
            else return scope2;
        }

        public static AccessModifier GetLessRestrictive(this AccessModifier scope1, AccessModifier scope2)
        {
            if (!scope1.IsMoreRestrictive(scope2))
            {
                return scope1;

            }
            else return scope2;
        }

        public static AccessModifier GetScope(this MethodInfo method)
        {
            if (method.IsPublic) return AccessModifier.Public;
            if (method.IsPrivate) return AccessModifier.Private;
            if (method.IsPrivate && method.IsFamily) return AccessModifier.PrivateProtected;
            if (method.IsFamilyAndAssembly) return AccessModifier.ProtectedInternal;
            if (method.IsFamily) return AccessModifier.Protected;
            if (method.IsAssembly) return AccessModifier.Internal;
            return AccessModifier.Public;
        }

        public static ParameterModifier GetModifier(this ParameterInfo parameter)
        {
            if (parameter.ParameterType.IsByRef) return ParameterModifier.Ref;
            if (parameter.IsOut) return ParameterModifier.Out;
            if (parameter.IsIn) return ParameterModifier.In;
            return ParameterModifier.None;
        }

        public static AccessModifier GetScope(this ConstructorInfo constructor)
        {
            if (constructor.IsPublic) return AccessModifier.Public;
            if (constructor.IsPrivate) return AccessModifier.Private;
            if (constructor.IsPrivate && constructor.IsFamily) return AccessModifier.PrivateProtected;
            if (constructor.IsFamilyAndAssembly) return AccessModifier.ProtectedInternal;
            if (constructor.IsFamily) return AccessModifier.Protected;
            if (constructor.IsAssembly) return AccessModifier.Internal;
            return AccessModifier.Public;
        }

        public static AccessModifier GetScope(this PropertyInfo property)
        {
            var getMethod = property.GetGetMethod(true);
            var setMethod = property.GetSetMethod(true);

            if (getMethod != null && setMethod == null)
            {
                return getMethod.GetScope();
            }
            else if (setMethod != null && getMethod == null)
            {
                return setMethod.GetScope();
            }
            else
            {
                AccessModifier getMethodAccess = getMethod.GetScope();
                AccessModifier setMethodAccess = setMethod.GetScope();

                return getMethodAccess.GetLessRestrictive(setMethodAccess);
            }
        }

        public static ParameterAttributes ToAttributes(this TypeParam parameter)
        {
            var attributes = ParameterAttributes.None;

            switch (parameter.modifier)
            {
                case ParameterModifier.In:
                    attributes |= ParameterAttributes.In;
                    break;
                case ParameterModifier.Out:
                    attributes |= ParameterAttributes.Out;
                    break;
                case ParameterModifier.Ref:
                    attributes |= ParameterAttributes.In | ParameterAttributes.Out;
                    break;
            }

            if (parameter.hasDefault)
            {
                attributes |= ParameterAttributes.HasDefault;
            }

            return attributes;
        }

        public static MethodModifier GetModifier(this MethodInfo method)
        {
            if (method.IsStatic) return MethodModifier.Static;
            if (method.IsVirtual) return MethodModifier.Virtual;
            if (method.IsAbstract) return MethodModifier.Abstract;
            if (method.IsFinal) return MethodModifier.Sealed;
            return MethodModifier.None;
        }

        public static ConstructorModifier GetModifier(this ConstructorInfo Constructor)
        {
            if (Constructor.IsStatic) return ConstructorModifier.Static;
            return ConstructorModifier.None;
        }

        public static string Parameters(this List<ParameterGenerator> parameters)
        {
            var output = "(";
            for (int i = 0; i < parameters.Count; i++)
            {
                output += parameters[i].Generate(0);
                if (i < parameters.Count - 1) output += ", ";
            }
            output += ")";
            return output;
        }

        public static string InitializeVariable(string name, Type type)
        {
            return !type.Is().NullOrVoid() ? type.As().CSharpName() + " " + name.Assign(HUMValue.Create().New(type).As().Code(false)) + "\n" : string.Empty;
        }

        public static string InitializeVariable(string name, Type type, string value)
        {
            return !type.Is().NullOrVoid() ? type.As().CSharpName() + " " + name.Assign(value) + "\n" : string.Empty;
        }

        public static string Quote()
        {
            return @"""";
        }

        public static string Quotes(this string value)
        {
            return @"""" + value + @"""";
        }


        public static string Comma()
        {
            return ", ";
        }

        public static bool Null(string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static string NullOrNot(object obj, string @null, string not)
        {
            return obj == null ? @null : not;
        }

        public static string NullVoidOrNot(Type type, string nullOrVoid, string not)
        {
            return type.Is().NullOrVoid() ? nullOrVoid : not;
        }

        public static string NullAsEmptyOr(string str, string or)
        {
            return Null(str) ? string.Empty : or;
        }

        public static string End()
        {
            return ");";
        }

        public static string End(this string code)
        {
            return code + ");";
        }

        public static string End(this string code, Unit unit)
        {
            return code + ");".MakeClickable(unit);
        }

        public static string Parentheses(this string value)
        {
            return "(" + value + ")";
        }

        public static string Parentheses(this string value, Unit unit)
        {
            return "(".MakeClickable(unit) + value + ")".MakeClickable(unit);
        }

        public static string SingleLineLambda(string parameters, string body)
        {
            return "(" + parameters + ") => { " + body + " }";
        }

        public static string ExpressionLambda(this string body, string parameters = "", Unit unit = null)
        {
            return "(".MakeClickable(unit) + parameters + ") => ".MakeClickable(unit) + body;
        }

        public static string ExpressionLambda(this string body, Unit unit = null)
        {
            return "() => ".MakeClickable(unit) + body;
        }

        public static string MultiLineLambda(string parameters, string body, int Indent)
        {
            return $"({parameters}) =>" + "\n" +
                    CodeBuilder.Indent(Indent) + "{" + "\n" +
                    body + "\n" +
                   CodeBuilder.Indent(Indent) + "}";
        }

        public static string MultiLineLambda(Unit unit, string parameters, string body, int Indent)
        {
            return CodeUtility.MakeClickable(unit, "(") + parameters + CodeUtility.MakeClickable(unit, ") =>") + "\n" +
                   CodeBuilder.Indent(Indent) + CodeUtility.MakeClickable(unit, "{") + "\n" +
                    body + "\n" +
                   CodeBuilder.Indent(Indent) + CodeUtility.MakeClickable(unit, "}");
        }

        public static string Assign(this string member, string value, Type castedType)
        {
            return member + " = " + value.CastTo(castedType) + ";";
        }

        public static string Assign(this string member, string value)
        {
            return member + " = " + value + ";";
        }

        public static string Return(this string value, bool highlight = true, Unit unit = null)
        {
            return (highlight ? "return ".ControlHighlight() : "return ").MakeClickable(unit) + value + ";".MakeClickable(unit);
        }

        public static string YieldReturn(this string value, bool highlight = true, Unit unit = null)
        {
            return (highlight ? "yield return ".ControlHighlight() : "yield return ").MakeClickable(unit) + value + ";".MakeClickable(unit);
        }

        public static string Create(this Type type, string parameters = "", bool fullName = true, bool highlight = true, Unit unit = null)
        {
            return ((highlight ? "new ".ConstructHighlight() : "new ") + type.As().CSharpName(false, fullName, highlight) + $"(").MakeClickable(unit) + parameters + ")".MakeClickable(unit);
        }

        private static string MakeClickable(this string value, Unit unit)
        {
            if (unit != null)
            {
                return CodeUtility.MakeClickable(unit, value);
            }
            else
            {
                return value;
            }
        }

        private static string MakeClickableIf(this string value, Unit unit, bool isTrue)
        {
            if (isTrue)
            {
                return CodeUtility.MakeClickable(unit, value);
            }
            else
            {
                return value;
            }
        }

        public static string GetConvertToString<T>(this string str, Unit unit = null)
        {
            return str + $".ConvertTo<{typeof(T).As().CSharpName(false, true)}>()".MakeClickable(unit);
        }

        public static string GetConvertToString(this string str, Type type, Unit unit = null)
        {
            return str + $".ConvertTo<{type.As().CSharpName(false, true)}>()".MakeClickable(unit);
        }

        public static string CastTo(this string value, Type type, bool shouldCast = true)
        {
            if (shouldCast)
                return $"({type.As().CSharpName(false, true)}){value}";
            return value;
        }

        public static string CastTo(this string value, Type type, Unit unit, bool shouldCast = true)
        {
            if (shouldCast)
                return $"({type.As().CSharpName(false, true)})".MakeClickable(unit) + value;
            return value;
        }

        public static string CastAs(this string value, Type type, bool shouldCast)
        {
            if (shouldCast)
                return $"(({type.As().CSharpName(false, true)}){value})";
            return value;
        }

        public static string CastAs(this string value, Type type, Unit unit, bool shouldCast)
        {
            if (shouldCast)
                return $"(({type.As().CSharpName(false, true)})".MakeClickable(unit) + value + ")".MakeClickable(unit);
            return value;
        }

        public static string LegalMemberName(this string memberName)
        {
            if (string.IsNullOrEmpty(memberName))
                return string.Empty;

            var builder = new System.Text.StringBuilder(memberName.Length);

            foreach (var c in memberName)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    builder.Append(c);
                }
            }

            if (builder.Length > 0 && char.IsDigit(builder[0]))
            {
                builder.Insert(0, '_');
            }

            return builder.ToString();
        }

        public static string GenericName(this string memberName, int count)
        {
            if (string.IsNullOrWhiteSpace(memberName))
                return "T" + count;

            var builder = new System.Text.StringBuilder(memberName.Length);

            foreach (char c in memberName)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    builder.Append(c);
                }
            }

            if (builder.Length == 0)
            {
                return "T" + count;
            }

            if (char.IsDigit(builder[0]))
            {
                builder.Insert(0, 'T');
            }

            return builder.ToString();
        }

        /// <summary>
        /// Generate code for calling a method in the CSharpUtility class
        /// </summary>
        /// <param name="unit">Unit to make the code selectable for</param>
        /// <param name="methodName">Method to call, This is not made selectable</param>
        /// <param name="parameters">Parameters for the method, This is not made selectable</param>
        /// <returns>The method call as a string</returns>
        public static string CallCSharpUtilityMethod(Unit unit, string methodName, params string[] parameters)
        {
            return CodeUtility.MakeClickable(unit, $"{typeof(CSharpUtility).As().CSharpName(false, true)}.") + methodName + CodeUtility.MakeClickable(unit, "(") + string.Join(CodeUtility.MakeClickable(unit, ", "), parameters) + CodeUtility.MakeClickable(unit, ")");
        }

        /// <summary>
        /// Generate code for calling a generic method in the CSharpUtility class
        /// </summary>
        /// <param name="unit">Unit to make the code selectable for</param>
        /// <param name="methodName">Method to call, This is not made selectable</param>
        /// <param name="parameters">Parameters for the method, This is not made selectable</param>
        /// <returns>The method call as a string</returns>
        public static string CallCSharpUtilityGenericMethod(Unit unit, string methodName, string[] parameters, params Type[] genericTypes)
        {
            if (genericTypes.Length > 0)
            {
                string genericTypeString = CodeUtility.MakeClickable(unit, "<" + string.Join(", ", genericTypes.Select(t => t.As().CSharpName(false, true))) + ">");
                return CodeUtility.MakeClickable(unit, $"{typeof(CSharpUtility).As().CSharpName(false, true)}.") + methodName + genericTypeString + CodeUtility.MakeClickable(unit, "(") + string.Join(CodeUtility.MakeClickable(unit, ", "), parameters) + CodeUtility.MakeClickable(unit, ")");
            }
            else
            {
                return CallCSharpUtilityMethod(unit, methodName, parameters);
            }
        }

        /// <summary>
        /// Generate code for calling a extensition method in the CSharpUtility class
        /// </summary>
        /// <param name="unit">Unit to make the code selectable for</param>
        /// <param name="target">Target for the method This is not made selectable</param>
        /// <param name="methodName">Method to call, This is not made selectable</param>
        /// <param name="parameters">Parameters for the method, This is not made selectable</param>
        /// <returns>The method call as a string</returns>
        public static string CallCSharpUtilityExtensitionMethod(Unit unit, string target, string methodName, params string[] parameters)
        {
            return target + ".".MakeClickable(unit) + methodName + "(".MakeClickable(unit) + string.Join(", ".MakeClickable(unit), parameters) + ")".MakeClickable(unit);
        }

        /// <summary>
        /// Generate code for calling a static method in the Type inputed
        /// </summary>
        /// <param name="unit">Unit to make the code selectable for</param>
        /// <param name="methodName">Method to call, This is not made selectable</param>
        /// <param name="fullName">if the type should generate with its full name or not</param>
        /// <param name="parameters">Parameters for the method, This is not made selectable</param>
        /// <returns>The method call as a string</returns>
        public static string StaticCall(Unit unit, Type type, string methodName, bool fullName = true, params string[] parameters)
        {
            var typeName = type.As().CSharpName(false, fullName);
            var clickableType = typeName.MakeClickable(unit);
            var clickableMethodName = methodName.MakeClickable(unit);
            var clickableOpenParen = "(".MakeClickable(unit);
            var clickableComma = ", ".MakeClickable(unit);
            var clickableCloseParen = ")".MakeClickable(unit);

            return clickableType + "." + clickableMethodName + clickableOpenParen +
                   string.Join(clickableComma, parameters) + clickableCloseParen;
        }

        /// <summary>
        /// Generate code for calling a static method in the Type inputed
        /// </summary>
        /// <param name="unit">Unit to make the code selectable for</param>
        /// <param name="methodName">Method to call, This is not made selectable</param>
        /// <param name="fullName">if the type should generate with its full name or not</param>
        /// <param name="parameters">Parameters for the method, This is not made selectable</param>
        /// <returns>The method call as a string</returns>
        public static string StaticCall(Type type, string methodName, bool fullName = true, params string[] parameters)
        {
            string typeName = type.As().CSharpName(false, fullName);
            string joinedParams = string.Join(", ", parameters);

            return $"{typeName}.{methodName}({joinedParams})";
        }

        public static string Highlight(string code, Color color)
        {
            string hex = UnityEngine.ColorUtility.ToHtmlStringRGB(color);
            return $"[BeginUAPreviewHighlight]<color=#{hex}>[EndUAPreviewHighlight]{code}[BeginUAPreviewHighlight]</color>[EndUAPreviewHighlight]";
        }

        public static string Highlight(string code, string hex)
        {
            return $"[BeginUAPreviewHighlight]<color=#{hex}>[EndUAPreviewHighlight]{code}[BeginUAPreviewHighlight]</color>[EndUAPreviewHighlight]";
        }

        public static string MakeRecommendation(string Message)
        {
            if (CSharpPreviewSettings.ShouldShowRecommendations) return $"/*(Recommendation) {Message}*/".RecommendationHighlight();
            else return "";
        }

        public static string WarningHighlight(this string code)
        {
            return Highlight(code, WarningColor);
        }

        public static string ConstructHighlight(this string code)
        {
            // Temporary compatibility with existing uses of "if" and "else"
            if (code == "if" || code == "else")
            {
                return code.ControlHighlight();
            }

            return Highlight(code, ConstructColor);
        }

        public static string NamespaceHighlight(this string code)
        {
            return Highlight(code, new Color(0.50f, 0.50f, 0.50f));
        }

        public static string InterfaceHighlight(this string code)
        {
            return Highlight(code, InterfaceColor);
        }

        public static string EnumHighlight(this string code)
        {

            return Highlight(code, EnumColor);
        }

        public static string TypeHighlight(this string code)
        {
            return Highlight(code, TypeColor);
        }

        public static string StringHighlight(this string code)
        {
            return Highlight(code, StringColor);
        }

        public static string NumericHighlight(this string code)
        {
            return Highlight(code, NumericColor);
        }

        public static string CommentHighlight(this string code)
        {
            return Highlight(code, CommentColor);
        }

        public static string SummaryHighlight(this string code)
        {
            return Highlight(code, SummaryColor);
        }

        public static string VariableHighlight(this string code)
        {
            return Highlight(code, VariableColor);
        }

        public static string ControlHighlight(this string code)
        {
            return Highlight(code, "FF6BE8");
        }

        public static string RecommendationHighlight(this string code)
        {
            return Highlight(code, RecommendationColor);
        }

        private static readonly Dictionary<string, string> RemoveHighlightsCache = new Dictionary<string, string>();
        public static string RemoveHighlights(this string code)
        {
            if (RemoveHighlightsCache.TryGetValue(code, out var result))
                return result;
            var _code = code.RemoveBetween("[BeginUAPreviewHighlight]", "[EndUAPreviewHighlight]");
            RemoveHighlightsCache[code] = _code;
            return _code;
        }

        public static string RemoveMarkdown(this string code)
        {
            var _code = code.Replace("[BeginUAPreviewHighlight]", string.Empty);
            _code = _code.Replace("[EndUAPreviewHighlight]", string.Empty);
            return _code;
        }
    }
}