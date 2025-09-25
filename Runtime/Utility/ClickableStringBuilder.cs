using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// A helper for building C# code strings with optional clickable segments
    /// for integration with <see cref="CodeUtility.MakeClickable(Unit, string)"/>.
    /// 
    /// Designed for use in code preview windows to make parts of generated code
    /// interactive (clickable) while keeping other parts static.
    /// </summary>
    public class ClickableStringBuilder
    {
        private readonly Unit unit;
        private readonly List<(string value, bool clickable)> segments = new();
        private bool ignoreContextActive = false;

        private ClickableStringBuilder(Unit unit, string value, bool clickable)
        {
            this.unit = unit;
            segments.Add((value, clickable));
        }

        private ClickableStringBuilder(Unit unit)
        {
            this.unit = unit;
        }

        /// <summary>
        /// Creates a new <see cref="ClickableStringBuilder"/> starting with the specified text.
        /// </summary>
        /// <param name="unit">The owning unit this code is generated for.</param>
        /// <param name="initial">Initial text to add to the builder.</param>
        /// <param name="clickable">If <c>true</c>, the initial text will be clickable.</param>
        public static ClickableStringBuilder CreateString(Unit unit, string initial, bool clickable)
        {
            return new ClickableStringBuilder(unit, initial, clickable);
        }

        /// <summary>
        /// Creates a new <see cref="ClickableStringBuilder"/> starting with the specified text.
        /// </summary>
        /// <param name="unit">The owning unit this code is generated for.</param>
        /// <param name="initial">Initial text to add to the builder.</param>
        /// <param name="clickable">If <c>true</c>, the initial text will be clickable.</param>
        public static ClickableStringBuilder CreateString(Unit unit)
        {
            return new ClickableStringBuilder(unit);
        }

        /// <summary>
        /// Will invert all calls to use Ignore instead of Clickable unless explicitly called.
        /// </summary>
        public ClickableStringBuilder IgnoreContext()
        {
            ignoreContextActive = true;
            return this;
        }

        /// <summary>
        /// Disables ignore context mode and returns to the default behavior
        /// where added text follows the <c>Clickable</c> setting.
        /// </summary>
        public ClickableStringBuilder EndIgnoreContext()
        {
            ignoreContextActive = false;
            return this;
        }

        /// <summary>
        /// Adds a clickable code segment.
        /// </summary>
        public ClickableStringBuilder Clickable(string value)
        {
            segments.Add((value, true));
            return this;
        }

        /// <summary>
        /// Adds a non-clickable code segment.
        /// </summary>
        public ClickableStringBuilder Ignore(string value)
        {
            segments.Add((value, false));
            return this;
        }

        /// <summary>
        /// Adds text based on the current ignore context.
        /// </summary>
        private ClickableStringBuilder Add(string value)
        {
            return ignoreContextActive ? Ignore(value) : Clickable(value);
        }

        /// <summary>
        /// Adds one of two values based on a condition:
        /// If <paramref name="ignoreCondition"/> is true, uses <paramref name="ignore"/> as non-clickable.
        /// Otherwise, uses <paramref name="clickable"/> as clickable.
        /// </summary>
        public ClickableStringBuilder IgnoreIf(bool ignoreCondition, string ignore, string clickable)
        {
            return ignoreCondition ? Ignore(ignore) : Clickable(clickable);
        }

        #region Parentheses
        public ClickableStringBuilder OpenParentheses() => Add("(");
        public ClickableStringBuilder OpenParentheses(string before) => Add(before + "(");
        public ClickableStringBuilder CloseParentheses() => Add(")");
        public ClickableStringBuilder CloseParentheses(string after) => Add(")" + after);

        /// <summary>
        /// Creates empty parentheses: ()
        /// </summary>
        public ClickableStringBuilder Parentheses()
        {
            Add("(");
            Add(")");
            return this;
        }

        /// <summary>
        /// Wraps inner content in parentheses: ( ... )
        /// </summary>
        public ClickableStringBuilder Parentheses(Action<ClickableStringBuilder> inner)
        {
            Add("(");
            inner(this);
            Add(")");
            return this;
        }

        /// <summary>
        /// Wraps inner content in parentheses with optional prefix and suffix.
        /// </summary>
        public ClickableStringBuilder Parentheses(string before, Action<ClickableStringBuilder> inner, string after = "")
        {
            Add(before);
            Add("(");
            inner(this);
            Add(")");
            Add(after);
            return this;
        }
        #endregion

        /// <summary>
        /// Adds a member access expression: target.member
        /// Highlights '<paramref name="member"/>' with VariableHighlight
        /// </summary>
        public ClickableStringBuilder GetMember(string target, string member)
        {
            return Add(target).Dot().Add(member.VariableHighlight());
        }

        /// <summary>
        /// Adds a member access expression: target.member
        /// Highlights '<paramref name="member"/>' with VariableHighlight
        /// </summary>
        public ClickableStringBuilder GetMember(Action<ClickableStringBuilder> target, string member)
        {
            target?.Invoke(this);
            return Dot().Add(member.VariableHighlight());
        }

        /// <summary>
        /// Adds a member access expression: .member
        /// Highlights '<paramref name="member"/>' with VariableHighlight
        /// </summary>
        public ClickableStringBuilder GetMember(string member)
        {
            return Dot().Add(member.VariableHighlight());
        }

        /// <summary>
        /// Adds a member access expression: Type.member
        /// Highlights '<paramref name="member"/>' with VariableHighlight
        /// </summary>
        public ClickableStringBuilder GetMember(Type target, string member)
        {
            return Add(target.As().CSharpName(false, true)).Dot().Add(member.VariableHighlight());
        }

        /// <summary>
        /// Adds a set member expression: target.member = value
        /// Highlights '<paramref name="member"/>' with VariableHighlight
        /// </summary>
        public ClickableStringBuilder SetMember(string target, string member, string value)
        {
            return Add(target).Dot().Add(member.VariableHighlight()).Equal(true).Add(value);
        }

        /// <summary>
        /// Adds a set member expression: target.member = value
        /// Highlights '<paramref name="member"/>' with VariableHighlight
        /// </summary>
        public ClickableStringBuilder SetMember(string target, string member, Action<ClickableStringBuilder> value)
        {
            Add(target).Dot().Add(member.VariableHighlight()).Equal(true);
            value?.Invoke(this);
            return this;
        }

        /// <summary>
        /// Adds a set member expression: Type.member = value
        /// Highlights '<paramref name="member"/>' with VariableHighlight
        /// </summary>
        public ClickableStringBuilder SetMember(Type target, string member, Action<ClickableStringBuilder> value)
        {
            Add(target.As().CSharpName(false, true)).Dot().Add(member.VariableHighlight()).Equal(true);
            value?.Invoke(this);
            return this;
        }

        /// <summary>
        /// Adds a set member expression: Type.member = value
        /// Highlights '<paramref name="member"/>' with VariableHighlight
        /// </summary>
        public ClickableStringBuilder SetMember(Type target, string member, string value)
        {
            return Add(target.As().CSharpName(false, true)).Dot().Add(member.VariableHighlight()).Equal(true).Add(value);
        }

        /// <summary>
        /// Adds a invoke member expression: target.member()
        /// </summary>
        public ClickableStringBuilder InvokeMember(string target, string member, params string[] parameters)
        {
            return Add(target).Dot().Add(member).Parentheses(inner => inner.Add(string.Join(", ", parameters)));
        }

        /// <summary>
        /// Adds a invoke member expression: target.member()
        /// </summary>
        public ClickableStringBuilder InvokeMember(string target, string member, params Action<ClickableStringBuilder>[] parameters)
        {
            return Add(target).Dot().Add(member).Parentheses(inner =>
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    parameters[i](inner);
                    if (i < parameters.Length - 1)
                    {
                        inner.Add(", ");
                    }
                }
            });
        }

        /// <summary>
        /// Adds a invoke member expression: target.member()
        /// </summary>
        public ClickableStringBuilder InvokeMember(Action<ClickableStringBuilder> target, string member, params Action<ClickableStringBuilder>[] parameters)
        {
            target?.Invoke(this);
            return Dot().Add(member).Parentheses(inner =>
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    parameters[i](inner);
                    if (i < parameters.Length - 1)
                    {
                        inner.Add(", ");
                    }
                }
            });
        }

        /// <summary>
        /// Adds a invoke member expression: target.member()
        /// </summary>
        public ClickableStringBuilder InvokeMember(Action<ClickableStringBuilder> target, string member, Type[] generics, params Action<ClickableStringBuilder>[] parameters)
        {
            target?.Invoke(this);
            return Dot().Add(member).Add($"<{string.Join(", ", generics.Select(g => g.As().CSharpName(false, true)))}>").Parentheses(inner =>
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    parameters[i](inner);
                    if (i < parameters.Length - 1)
                    {
                        inner.Add(", ");
                    }
                }
            });
        }

        /// <summary>
        /// Adds a invoke member expression: target.member()
        /// </summary>
        public ClickableStringBuilder InvokeMember(Action<ClickableStringBuilder> target, string member, Type[] generics, params string[] parameters)
        {
            target?.Invoke(this);
            return Dot().Add(member).Add($"<{string.Join(", ", generics.Select(g => g.As().CSharpName(false, true)))}>").Parentheses(inner => inner.Add(string.Join(", ", parameters)));
        }

        /// <summary>
        /// Adds a invoke member expression: Type.member()
        /// </summary>
        public ClickableStringBuilder InvokeMember(Type target, string member, params Action<ClickableStringBuilder>[] parameters)
        {
            return Add(target.As().CSharpName(false, true)).Dot().Add(member).Parentheses(inner =>
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    parameters[i](inner);
                    if (i < parameters.Length - 1)
                    {
                        inner.Add(", ");
                    }
                }
            });
        }

        /// <summary>
        /// Adds a invoke member expression: Type.member()
        /// </summary>
        public ClickableStringBuilder InvokeMember(Type target, string member)
        {
            return Add(target.As().CSharpName(false, true)).Dot().Add(member).Parentheses();
        }

        /// <summary>
        /// Adds a invoke member expression: Type.member()
        /// </summary>
        public ClickableStringBuilder InvokeMember(Type target, string member, params Type[] generics)
        {
            return Add(target.As().CSharpName(false, true)).Dot().Add(member).Add($"<{string.Join(", ", generics.Select(g => g.As().CSharpName(false, true)))}>").Parentheses();
        }

        /// <summary>
        /// Adds a invoke member expression: Type.member()
        /// </summary>
        public ClickableStringBuilder InvokeMember(Type target, string member, Type[] generics, params string[] parameters)
        {
            return Add(target.As().CSharpName(false, true)).Dot().Add(member).Add($"<{string.Join(", ", generics.Select(g => g.As().CSharpName(false, true)))}>").Parentheses(inner => inner.Add(string.Join(", ", parameters)));
        }

        /// <summary>
        /// Adds a invoke member expression: Type.member()
        /// </summary>
        public ClickableStringBuilder InvokeMember(Type target, string member, Type[] generics, params Action<ClickableStringBuilder>[] parameters)
        {
            return Add(target.As().CSharpName(false, true)).Dot().Add(member).Add($"<{string.Join(", ", generics.Select(g => g.As().CSharpName(false, true)))}>").Parentheses(inner =>
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    parameters[i](inner);
                    if (i < parameters.Length - 1)
                    {
                        inner.Add(", ");
                    }
                }
            });
        }

        /// <summary>
        /// Adds a invoke member expression: Type.member()
        /// </summary>
        public ClickableStringBuilder InvokeMember(Type target, string member, Type[] generics, bool fullName, params Action<ClickableStringBuilder>[] parameters)
        {
            return Add(target.As().CSharpName(false, fullName)).Dot().Add(member).Add($"<{string.Join(", ", generics.Select(g => g.As().CSharpName(false, fullName)))}>").Parentheses(inner =>
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    parameters[i](inner);
                    if (i < parameters.Length - 1)
                    {
                        inner.Add(", ");
                    }
                }
            });
        }

        /// <summary>
        /// Adds a method call expression: Method()
        /// </summary>
        public ClickableStringBuilder MethodCall(string name, params string[] parameters)
        {
            return Add(name).Parentheses(inner => inner.Add(string.Join(", ", parameters)));
        }

        /// <summary>
        /// Adds a method call expression: Method()
        /// </summary>
        public ClickableStringBuilder MethodCall(string name, Type[] generics, params string[] parameters)
        {
            return Add(name).Add($"<{string.Join(", ", generics.Select(g => g.As().CSharpName(false, true)))}>").Parentheses(inner => inner.Add(string.Join(", ", parameters)));
        }

        /// <summary>
        /// Adds a method call expression: Method()
        /// </summary>
        public ClickableStringBuilder MethodCall(string name, params Type[] generics)
        {
            return Add(name).Add($"<{string.Join(", ", generics.Select(g => g.As().CSharpName(false, true)))}>").Parentheses();
        }

        /// <summary>
        /// Adds a method call expression: Method()
        /// </summary>
        public ClickableStringBuilder MethodCall(string name, params Action<ClickableStringBuilder>[] parameters)
        {
            return Add(name).Parentheses(inner =>
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    parameters[i](inner);
                    if (i < parameters.Length - 1)
                    {
                        inner.Add(", ");
                    }
                }
            });
        }

        /// <summary>
        /// Adds a method call expression: Method()
        /// </summary>
        public ClickableStringBuilder MethodCall(string name)
        {
            return Add(name).Parentheses();
        }

        /// <summary>
        /// Adds a constructor call expression: new Type()
        /// </summary>
        public ClickableStringBuilder Create(Type type)
        {
            return Add("new ".ConstructHighlight()).Add(type.As().CSharpName(false, true)).Parentheses();
        }

        /// <summary>
        /// Adds a constructor call expression: new Type()
        /// </summary>
        public ClickableStringBuilder Create(Type type, params string[] parameters)
        {
            return Add("new ".ConstructHighlight()).Add(type.As().CSharpName(false, true)).Parentheses(inner => inner.Add(string.Join(", ", parameters)));
        }

        /// <summary>
        /// Adds a constructor call expression: new Type()
        /// </summary>
        public ClickableStringBuilder Create(Type type, params Action<ClickableStringBuilder>[] parameters)
        {
            return Add("new ".ConstructHighlight()).Add(type.As().CSharpName(false, true)).Parentheses(inner =>
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    parameters[i](inner);
                    if (i < parameters.Length - 1)
                    {
                        inner.Add(", ");
                    }
                }
            });
        }

        public ClickableStringBuilder Select(Action<ClickableStringBuilder> condition, string ifTrue, string ifFalse)
        {
            condition(this);
            Add(" ? ");
            Ignore(ifTrue);
            Add(" : ");
            Ignore(ifFalse);
            return this;
        }

        public ClickableStringBuilder Select(Action<ClickableStringBuilder> condition, Action<ClickableStringBuilder> ifTrue, Action<ClickableStringBuilder> ifFalse)
        {
            condition(this);
            Add(" ? ");
            ifTrue(this);
            Add(" : ");
            ifFalse(this);
            return this;
        }

        public ClickableStringBuilder Select(string condition, string ifTrue, string ifFalse)
        {
            return Add($"{condition} ? {ifTrue} : {ifFalse}");
        }

        #region Brace
        public ClickableStringBuilder OpenBrace(int indent = 0)
        {
            Ignore(CodeBuilder.Indent(indent));
            return Add("{");
        }
        public ClickableStringBuilder OpenBrace(string before, int indent = 0)
        {
            Ignore(CodeBuilder.Indent(indent));
            return Add(before + "{");
        }
        public ClickableStringBuilder CloseBrace(int indent = 0)
        {
            Ignore(CodeBuilder.Indent(indent));
            return Add("}");
        }
        public ClickableStringBuilder CloseBrace(string after, int indent = 0)
        {
            Ignore(CodeBuilder.Indent(indent));
            return Add(after + "}");
        }

        /// <summary>
        /// Appends a code block in braces: { ... }
        /// </summary>
        public ClickableStringBuilder Braces(Action<ClickableStringBuilder> inner, bool newLine, int indent = 0)
        {
            OpenBrace(indent);
            if (newLine) NewLine();
            inner(this);
            if (newLine) NewLine();
            CloseBrace(indent);
            return this;
        }

        /// <summary>
        /// Appends a code block in braces with text before and after.
        /// </summary>
        public ClickableStringBuilder Braces(string before, Action<ClickableStringBuilder> inner, bool newLine, string after = "", int indent = 0)
        {
            OpenBrace(before, indent);
            if (newLine) NewLine();
            inner(this);
            if (newLine) NewLine();
            CloseBrace(after, indent);
            return this;
        }

        /// <summary>
        /// Appends a code block with a preceding section (e.g. if header) and inner body.
        /// </summary>
        public ClickableStringBuilder Body(Action<ClickableStringBuilder> before, Action<ClickableStringBuilder, int> inner, bool newLine, int indent = 0, bool newLineOnEnd = true)
        {
            Indent(indent);
            before?.Invoke(this);
            if (newLine) NewLine();
            OpenBrace(indent);
            if (newLine) NewLine();
            inner?.Invoke(this, indent + 1);
            if (newLine) NewLine();
            CloseBrace(indent);
            if (newLineOnEnd) NewLine();
            return this;
        }

        /// <summary>
        /// Appends a code block with a preceding section (e.g. if header) and inner body.
        /// </summary>
        public ClickableStringBuilder If(Action<ClickableStringBuilder> condition, Action<ClickableStringBuilder, int> inner, bool newLine, int indent = 0)
        {
            return Body(before => before.Clickable("if ".ControlHighlight()).Parentheses(condition), inner, newLine, indent);
        }

        /// <summary>
        /// Appends a code block with a preceding section (e.g. if header) and inner body.
        /// </summary>
        public ClickableStringBuilder If(string condition, Action<ClickableStringBuilder, int> inner, bool newLine, int indent = 0)
        {
            return If(c => c.Clickable(condition), inner, newLine, indent);
        }
        #endregion

        public ClickableStringBuilder OpenBracket(int indent = 0)
        {
            Ignore(CodeBuilder.Indent(indent));
            return Add("[");
        }
        public ClickableStringBuilder OpenBracket(string before, int indent = 0)
        {
            Ignore(CodeBuilder.Indent(indent));
            return Add(before + "[");
        }

        public ClickableStringBuilder CloseBracket(int indent = 0)
        {
            Ignore(CodeBuilder.Indent(indent));
            return Add("]");
        }
        public ClickableStringBuilder CloseBracket(string before, int indent = 0)
        {
            Ignore(CodeBuilder.Indent(indent));
            return Add(before + "]");
        }

        /// <summary>
        /// Appends brackets: [ ... ]
        /// </summary>
        public ClickableStringBuilder Brackets(Action<ClickableStringBuilder> inner, bool newLine, int indent = 0)
        {
            OpenBracket(indent);
            if (newLine) NewLine();
            inner(this);
            if (newLine) NewLine();
            CloseBracket(indent);
            return this;
        }

        public ClickableStringBuilder Space() => Add(" ");
        public ClickableStringBuilder Space(int count) => Add(new string(' ', count));
        public ClickableStringBuilder Comma(string after = "") => Add("," + after);
        public ClickableStringBuilder Dot() => Add(".");
        public ClickableStringBuilder Equal(bool spaceAround = false) => Add((spaceAround ? " " : "") + "=" + (spaceAround ? " " : ""));
        public ClickableStringBuilder Equals(bool spaceAround = false) => Add((spaceAround ? " " : "") + "==" + (spaceAround ? " " : ""));
        public ClickableStringBuilder NotEquals(bool spaceAround = false) => Add((spaceAround ? " " : "") + "!=" + (spaceAround ? " " : ""));
        public ClickableStringBuilder Null() => Add("null".ConstructHighlight());

        /// <summary>
        /// Adds a statement terminator ');'
        /// </summary>
        public ClickableStringBuilder EndStatement() => Add(CodeBuilder.End());

        /// <summary>
        /// Adds line end ';\n'
        /// </summary>
        public ClickableStringBuilder EndLine(bool newLine = true) => newLine ? Add(";").NewLine() : Add(";");
        public ClickableStringBuilder NewLine() => Ignore("\n");
        public ClickableStringBuilder Indent() => Ignore(CodeBuilder.Indent(1));
        public ClickableStringBuilder Indent(int indent) => Ignore(CodeBuilder.Indent(indent));

        /// <summary>
        /// Wraps the current built string in a cast expression, if required.
        /// </summary>
        /// <param name="castType">The type to cast to.</param>
        /// <param name="shouldCast">Whether to apply the cast.</param>
        /// <param name="convertType">If true, wraps the cast in parentheses; otherwise, uses direct cast syntax.</param>
        public ClickableStringBuilder Cast(Type castType, bool shouldCast, bool convertType = true)
        {
            if (castType == null || !shouldCast) return this;

            var code = Build();
            var builder = CreateString(unit, null, true)
                .Ignore(convertType
                    ? CodeBuilder.CastAs(code, castType, ignoreContextActive ? null : unit, true)
                    : CodeBuilder.Cast(code, castType, ignoreContextActive ? null : unit));

            builder.ignoreContextActive = ignoreContextActive;
            return builder;
        }


        /// <summary>
        /// Generate code for calling a extensition method in the CSharpUtility class
        /// </summary>
        /// <param name="target">Target for the method This is not made selectable</param>
        /// <param name="methodName">Method to call, This is not made selectable</param>
        /// <param name="parameters">Parameters for the method, This is not made selectable</param>
        /// <returns>The method call as a string</returns>
        public ClickableStringBuilder CallCSharpUtilityExtensitionMethod(string target, string methodName, params string[] parameters)
        {
            return Ignore(target).Dot().Ignore(methodName).Parentheses(inner => inner.Ignore(string.Join(CodeUtility.MakeClickable(unit, ", "), parameters)));
        }

        /// <summary>
        /// Generate code for calling a extensition method in the CSharpUtility class
        /// </summary>
        /// <param name="target">Target for the method This is not made selectable</param>
        /// <param name="methodName">Method to call, This is not made selectable</param>
        /// <param name="parameters">Parameters for the method, This is not made selectable</param>
        /// <returns>The method call as a string</returns>
        public ClickableStringBuilder CallCSharpUtilityExtensitionMethod(string target, string methodName)
        {
            return Ignore(target).Dot().Ignore(methodName);
        }

        /// <summary>
        /// Generate code for calling a extensition method in the CSharpUtility class
        /// </summary>
        /// <param name="target">Target for the method This is not made selectable</param>
        /// <param name="methodName">Method to call, This is not made selectable</param>
        /// <param name="parameters">Parameters for the method, This is not made selectable</param>
        /// <returns>The method call as a string</returns>
        public ClickableStringBuilder CallCSharpUtilityExtensitionMethod(string target, string methodName, params Action<ClickableStringBuilder>[] parameters)
        {
            return Ignore(target).Dot().Ignore(methodName).Parentheses(inner =>
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    parameters[i](inner);
                    if (i < parameters.Length - 1)
                    {
                        inner.Add(", ");
                    }
                }
            });
        }

        /// <summary>
        /// Generate code for calling a method in the CSharpUtility class
        /// </summary>
        /// <param name="target">Target for the method This is not made selectable</param>
        /// <param name="methodName">Method to call, This is not made selectable</param>
        /// <param name="parameters">Parameters for the method, This is not made selectable</param>
        /// <returns>The method call as a string</returns>
        public ClickableStringBuilder CallCSharpUtilityMethod(string methodName, params string[] parameters)
        {
            return Clickable(typeof(CSharpUtility).As().CSharpName(false, true)).Dot().Ignore(methodName).Parentheses(inner => inner.Ignore(string.Join(CodeUtility.MakeClickable(unit, ", "), parameters)));
        }

        /// <summary>
        /// Generate code for calling a method in the CSharpUtility class
        /// </summary>
        /// <param name="target">Target for the method This is not made selectable</param>
        /// <param name="methodName">Method to call, This is not made selectable</param>
        /// <param name="parameters">Parameters for the method, This is not made selectable</param>
        /// <returns>The method call as a string</returns>
        public ClickableStringBuilder CallCSharpUtilityMethod(string methodName)
        {
            return Clickable(typeof(CSharpUtility).As().CSharpName(false, true)).Dot().Ignore(methodName).Parentheses();
        }

        /// <summary>
        /// Generate code for calling a method in the CSharpUtility class
        /// </summary>
        /// <param name="target">Target for the method This is not made selectable</param>
        /// <param name="methodName">Method to call, This is not made selectable</param>
        /// <param name="parameters">Parameters for the method, This is not made selectable</param>
        /// <returns>The method call as a string</returns>
        public ClickableStringBuilder CallCSharpUtilityMethod(string methodName, params Action<ClickableStringBuilder>[] parameters)
        {
            return Clickable(typeof(CSharpUtility).As().CSharpName(false, true)).Dot().Ignore(methodName).Parentheses(inner =>
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    parameters[i](inner);
                    if (i < parameters.Length - 1)
                    {
                        inner.Add(", ");
                    }
                }
            });
        }

        /// <summary>
        /// Builds and returns the final string, applying clickable formatting to
        /// the appropriate segments.
        /// </summary>
        public string Build()
        {
            var result = new StringBuilder();
            var streak = new StringBuilder();
            bool? currentClickable = null;

            foreach (var (value, clickable) in segments)
            {
                if (currentClickable == null)
                {
                    currentClickable = clickable;
                    streak.Append(value);
                }
                else if (clickable == currentClickable)
                {
                    streak.Append(value);
                }
                else
                {
                    if (currentClickable.Value)
                        result.Append(CodeUtility.MakeClickable(unit, streak.ToString()));
                    else
                        result.Append(streak.ToString());

                    streak.Clear();
                    streak.Append(value);
                    currentClickable = clickable;
                }
            }

            if (streak.Length > 0)
            {
                if (currentClickable == true)
                    result.Append(CodeUtility.MakeClickable(unit, streak.ToString()));
                else
                    result.Append(streak.ToString());
            }

            return result.ToString();
        }

        public override string ToString() => Build();
        public static implicit operator string(ClickableStringBuilder builder) => builder.Build();
    }

    public static class XClickableStringBuilder
    {
        /// <summary>
        /// Creates a ClickableStringBuilder, should only be used for C# Preview.
        /// </summary>
        public static ClickableStringBuilder CreateClickableString(this Unit unit, string initial = "")
        {
            return ClickableStringBuilder.CreateString(unit, initial, true);
        }

        /// <summary>
        /// Creates a ClickableStringBuilder, should only be used for C# Preview.
        /// </summary>
        public static ClickableStringBuilder CreateClickableString(this Unit unit)
        {
            return ClickableStringBuilder.CreateString(unit);
        }

        /// <summary>
        /// Creates a ClickableStringBuilder with Ignore Context enabled, should only be used for C# Preview.
        /// </summary>
        public static ClickableStringBuilder CreateIgnoreString(this Unit unit, string initial = "")
        {
            return ClickableStringBuilder.CreateString(unit, initial, false).IgnoreContext();
        }
    }
}