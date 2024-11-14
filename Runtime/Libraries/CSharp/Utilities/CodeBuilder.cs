using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using Unity.VisualScripting.Community.Libraries.Humility;
using System.Linq;

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
        public static string MethodColor = "EBEB5B";
        #endregion

        public static bool ShowRecommendations = true;

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

        private static Dictionary<int, string> indentCache = new Dictionary<int, string>();
        /// <summary>
        /// Creates an indentation. The spacing is equal to 4 whitespaces.
        /// </summary>
        public static string Indent(int amount)
        {
            currentIndent = amount;
            if (indentCache.TryGetValue(amount, out var indent))
                return indent;

            var output = string.Empty;
            for (int i = 0; i < amount; i++)
            {
                output += "    ";
            }

            indentCache[amount] = output;

            return output;
        }

        private static Dictionary<(int, int), string> customIndentCache = new Dictionary<(int, int), string>();
        /// <summary>
        /// Creates an indentation with a custom amount of whitespaces per indent.
        /// </summary>
        public static string Indent(int amount, int spacing)
        {
            currentIndent = amount;
            if (customIndentCache.TryGetValue((amount, spacing), out var indent))
                return indent;
            var output = string.Empty;
            var space = string.Empty;
            for (int i = 0; i < spacing; i++)
            {
                space += " ";
            }

            for (int i = 0; i < amount; i++)
            {
                output += space;
            }

            customIndentCache[(amount, spacing)] = output;

            return output;
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
            var output = string.Empty;
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
            }

            for (int i = 0; i < generator.properties.Count; i++)
            {
                usings.MergeUnique(generator.properties[i].Usings());
            }

            for (int i = 0; i < generator.methods.Count; i++)
            {
                usings.MergeUnique(generator.methods[i].Usings());
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
            return !type.Is().NullOrVoid() ? type.As().CSharpName() + " " + CodeBuilder.Assign(name, HUMValue.Create().New(type).As().Code(false)) + "\n" : string.Empty;
        }

        public static string Qoute()
        {
            return @"""";
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

        public static string SingleLineLambda(string parameters, string body)
        {
            return "(" + parameters + ")=>{ " + body + " }";
        }

        public static string MultiLineLambda(string parameters, string body, int Indent)
        {
            return $"({parameters}) =>" + "\n" +
                   "{" + "\n" +
                    CodeBuilder.Indent(Indent) + body + "\n" +
                   "}";
        }

        public static string Assign(string member, string value, Type castedType)
        {
            return member + " = " + "(" + castedType.As().CSharpName() + ")" + value + ";";
        }

        public static string Assign(string member, string value)
        {
            return member + " = " + value + ";";
        }

        public static string Return(string value)
        {
            return "return ".ControlHighlight() + value + ";";
        }

        public static string CastAs(this string value, Type type)
        {
            return $"({type.As().CSharpName()}){value}";
        }

        public static string MethodCall(string name, Type[] generics, params string[] parameters)
        {
            return name.MethodHighlight() + $"{(generics.Length > 0 ? $"<{string.Join(", ", generics.Select(t => t.As().CSharpName(false, true)))}>" : "")}({string.Join(", ", parameters)})";
        }

        public static string TargetMethodCall(this string target, string name, Type[] generics = null, params string[] parameters)
        {
            return target + "." + name.MethodHighlight() + $"{(generics != null && generics.Length > 0 ? $"<{string.Join(", ", generics.Select(t => t.As().CSharpName(false, true)))}>" : "")}({string.Join(", ", parameters)})";
        }

        public static string LegalMemberName(this string memberName)
        {
            if (string.IsNullOrEmpty(memberName)) return string.Empty;

            var output = memberName;
            output = output.Replace(" ", string.Empty);

            var newCopy = output;

            for (int i = 0; i < newCopy.Length; i++)
            {
                if (!char.IsLetter(newCopy[i]) && !char.IsNumber(newCopy[i]) && newCopy[i] != "_".ToCharArray()[0])
                {
                    output = output.Replace(newCopy[i].ToString(), string.Empty);
                }
            }

            if (!string.IsNullOrEmpty(output) && char.IsNumber(output[0]))
            {
                output = "_" + output;
            }

            return output;
        }

        public static string Highlight(string code, Color color)
        {
            var output = string.Empty;
            output += "[BeginUAPreviewHighlight]" + $"<color=#{UnityEngine.ColorUtility.ToHtmlStringRGB(color)}>" + "[EndUAPreviewHighlight]";
            output += code;
            output += "[BeginUAPreviewHighlight]" + "</color>" + "[EndUAPreviewHighlight]";
            return output;
        }

        public static string Highlight(string code, string hex)
        {
            var output = string.Empty;
            output += "[BeginUAPreviewHighlight]" + $"<color=#{hex}>" + "[EndUAPreviewHighlight]";
            output += code;
            output += "[BeginUAPreviewHighlight]" + "</color>" + "[EndUAPreviewHighlight]";
            return output;
        }

        public static string MakeRecommendation(string Message)
        {
            if(ShowRecommendations) return $"/*(Recommendation) {Message}*/".RecommendationHighlight();
            else return "";
        }

        public static string WarningHighlight(this string code)
        {
            return Highlight(code, WarningColor);
        }

        public static string ConstructHighlight(this string code)
        {
            //I did this to avoid having to change the scripts that already used Construct Higlights for if
            //I will probably change this in the future though
            if(code == "if".Replace(" ", "") || code == "else".Replace(" ", ""))
            {
                return code.ControlHighlight();
            }
            return Highlight(code, ConstructColor);
        }

        public static string MethodHighlight(this string code)
        {
            return Highlight(code, MethodColor);
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

        private static Dictionary<string, string> RemoveHighlightsCache = new Dictionary<string, string>();
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