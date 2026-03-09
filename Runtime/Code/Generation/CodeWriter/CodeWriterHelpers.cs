using System;
using Unity.VisualScripting.Community.CSharp;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public sealed partial class CodeWriter
    {
        public CodeWriter Dot()
        {
            Write(".");
            return this;
        }

        public CodeWriter ParameterSeparator()
        {
            Write(", ");
            return this;
        }

        public CodeWriter Space()
        {
            Write(" ");
            return this;
        }

        public CodeWriter NewLine()
        {
            Write("\n");
            return this;
        }

        /// <summary>
        /// Writes );\n depending on the options inputted
        /// </summary>
        public CodeWriter WriteEnd(EndWriteOptions options = EndWriteOptions.All)
        {
            string newText = string.Empty;

            if ((options & EndWriteOptions.CloseParentheses) != 0)
                newText += ")";

            if ((options & EndWriteOptions.Semicolon) != 0)
                newText += ";";

            if ((options & EndWriteOptions.Newline) != 0)
                newText += "\n";

            Write(newText);
            return this;
        }

        public CodeWriter Null()
        {
            Write("null".ConstructHighlight());
            return this;
        }

        /// <summary>
        /// Write: -
        /// </summary>
        public CodeWriter Subtract(bool spaceAround = true)
        {
            var symbol = spaceAround ? " - " : "-";
            Write(symbol);
            return this;
        }
        /// <summary>
        /// Write: +
        /// </summary>
        public CodeWriter Add(bool spaceAround = true)
        {
            var symbol = spaceAround ? " + " : "+";
            Write(symbol);
            return this;
        }
        /// <summary>
        /// Write: *
        /// </summary>
        public CodeWriter Multiply(bool spaceAround = true)
        {
            var symbol = spaceAround ? " * " : "*";
            Write(symbol);
            return this;
        }
        /// <summary>
        /// Write: /
        /// </summary>
        public CodeWriter Divide(bool spaceAround = true)
        {
            var symbol = spaceAround ? " / " : "/";
            Write(symbol);
            return this;
        }

        /// <summary>
        /// Write: int value
        /// </summary>
        public CodeWriter Int(int value)
        {
            Write(value.As().Code(false, true, true, "", false, false));
            return this;
        }

        /// <summary>
        /// Write: float value
        /// </summary>
        public CodeWriter Float(float value)
        {
            Write(value.As().Code(false, true, true, "", false, false));
            return this;
        }

        /// <summary>
        /// Write: bool value
        /// </summary>
        public CodeWriter Bool(bool value)
        {
            Write(value.As().Code(false, true, true, "", false, false));
            return this;
        }

        /// <summary>
        /// Return: int value
        /// </summary>
        public string IntString(int value)
        {
            return value.As().Code(false, true, true, "", false, false);
        }

        /// <summary>
        /// Return: float value
        /// </summary>
        public string FloatString(float value)
        {
            return value.As().Code(false, true, true, "", false, false);
        }

        /// <summary>
        /// Return: bool value
        /// </summary>
        public string BoolString(bool value)
        {
            return value.As().Code(false, true, true, "", false, false);
        }

        /// <summary>
        /// Write: ==
        /// </summary>
        public CodeWriter Equals(bool spaceAround = true)
        {
            var equal = spaceAround ? " == " : "==";
            Write(equal);
            return this;
        }

        /// <summary>
        /// Write: =
        /// </summary>
        public CodeWriter Equal(bool spaceAround = true)
        {
            var equal = spaceAround ? " = " : "=";
            Write(equal);
            return this;
        }

        /// <summary>
        /// Write: !=
        /// </summary>
        public CodeWriter NotEqual(bool spaceAround = true)
        {
            var equal = spaceAround ? " != " : "!=";
            Write(equal);
            return this;
        }

        public CodeWriter Parentheses(Action<CodeWriter> inside = null)
        {
            // Capture write so that LastGeneratedCode returns all the code writen from this method
            using (CaptureWrite(out _))
            {
                Write("(");
                inside?.Invoke(this);
                Write(")");
            }
            return this;
        }

        /// <summary>
        /// Adds { } to the code
        /// </summary>
        /// <param name="inside">The code that goes inside the braces</param>
        public CodeWriter Braces(Action<CodeWriter> inside = null)
        {
            // Capture write so that LastGeneratedCode returns all the code writen from this method
            using (CaptureWrite(out _))
            {
                Write("{");

                inside?.Invoke(this);

                Write("}");
            }
            return this;
        }

        /// <summary>
        /// Adds \n{\n \n} to the code
        /// </summary>
        /// <param name="inside">The code that goes inside the braces</param>
        public CodeWriter Braces(Action<CodeWriter, int> inside = null)
        {
            // Capture write so that LastGeneratedCode returns all the code writen from this method
            using (CaptureWrite(out _))
            {
                NewLine();

                WriteLine("{");

                using (Indented())
                {
                    inside?.Invoke(this, IndentLevel);
                }

                WriteIndented("}");
            }
            return this;
        }

        /// <summary>
        /// Adds [ ] around the code generated inside the action
        /// </summary>
        /// <param name="inside">The code to generate inside the brackets</param>
        public CodeWriter Brackets(Action<CodeWriter> inside = null)
        {
            using (CaptureWrite(out _))
            {
                Write("[");
                inside?.Invoke(this);
                Write("]");
            }
            return this;
        }

        /// <summary>
        /// Adds [code] directly from a string or expression
        /// </summary>
        /// <param name="content">The content to place inside the brackets</param>
        public CodeWriter Brackets(string content)
        {
            using (CaptureWrite(out _))
            {
                Write("[");
                Write(content);
                Write("]");
            }
            return this;
        }

        public CodeWriter MultilineLambda(ValueParameter body, params MethodParameter[] parameters)
        {
            Parentheses(inner =>
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    var param = parameters[i];

                    if (param.Write != null)
                    {
                        param.Write(inner);
                    }
                    else
                    {
                        inner.Write(param.StringValue.VariableHighlight());
                    }

                    if (i < parameters.Length - 1)
                    {
                        inner.ParameterSeparator();
                    }
                }
            });

            Write(" => ");
            Braces((inner, indent) =>
            {
                if (body.Write != null)
                {
                    body.Write(inner);
                }
                else
                {
                    inner.Write(body.StringValue);
                }
            });
            return this;
        }

        public CodeWriter CallCSharpUtilityMethod(MemberParameter method, params MethodParameter[] parameters)
        {
            if (method == null) throw new ArgumentNullException("method");
            if (parameters == null) throw new ArgumentNullException("parameters");

            InvokeMember(Action(() => CSharpUtilityType()), Action(() =>
            {
                if (method.Write != null)
                {
                    method.Write(this);
                }
                else
                {
                    Write(method.StringValue);
                }
            }), parameters);

            return this;
        }

        public CodeWriter CallCSharpUtilityGenericMethod(MemberParameter method, TypeParameter[] generics, params MethodParameter[] parameters)
        {
            if (method == null) throw new ArgumentNullException("method");
            if (parameters == null) throw new ArgumentNullException("parameters");

            InvokeMember(Action(() => CSharpUtilityType()), Action(() =>
            {
                if (method.Write != null)
                {
                    method.Write(this);
                }
                else
                {
                    Write(method.StringValue);
                }
            }), generics, parameters);

            return this;
        }

        /// <summary>
        /// Adds /* {<tt>message</tt>} */ with a error highlight
        /// </summary>
        public CodeWriter Error(string message, WriteOptions options = WriteOptions.None)
        {
            if (string.IsNullOrEmpty(message))
                return this;

            string[] lines = message.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);


            if (lines.Length == 1)
            {
                Write(("/* " + lines[0] + " */").ErrorHighlight(), options);
                return this;
            }

            WriteOptions innerOptions = options & WriteOptions.Indented;

            Write(("/* " + lines[0]).ErrorHighlight(), options & ~WriteOptions.NewLineAfter);

            for (int i = 1; i < lines.Length - 1; i++)
                Write(lines[i].ErrorHighlight(), innerOptions | WriteOptions.NewLineAfter);

            Write((lines[lines.Length - 1] + " */").ErrorHighlight(),
                (options & WriteOptions.NewLineAfter) | innerOptions);

            return this;
        }

        /// <summary>
        /// Adds /* {<tt>message</tt>} */ with a Recommendation highlight
        /// </summary>
        public CodeWriter Recommendation(string message, WriteOptions options = WriteOptions.None)
        {
            if (string.IsNullOrEmpty(message))
                return this;

            string[] lines = message.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);


            if (lines.Length == 1)
            {
                Write(("/* " + lines[0] + " */").RecommendationHighlight(), options);
                return this;
            }

            WriteOptions innerOptions = options & WriteOptions.Indented;

            Write(("/* " + lines[0]).RecommendationHighlight(), options & ~WriteOptions.NewLineAfter);

            for (int i = 1; i < lines.Length - 1; i++)
                Write(lines[i].RecommendationHighlight(), innerOptions | WriteOptions.NewLineAfter);

            Write((lines[lines.Length - 1] + " */").RecommendationHighlight(),
                (options & WriteOptions.NewLineAfter) | innerOptions);

            return this;
        }

        /// <summary>
        /// Adds /* {<tt>message</tt>} */ with a warning highlight
        /// </summary>
        public CodeWriter Warning(string message, WriteOptions options = WriteOptions.None)
        {
            if (string.IsNullOrEmpty(message))
                return this;

            string[] lines = message.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);


            if (lines.Length == 1)
            {
                Write(("/* " + lines[0] + " */").WarningHighlight(), options);
                return this;
            }

            WriteOptions innerOptions = options & WriteOptions.Indented;

            Write(("/* " + lines[0]).WarningHighlight(), options & ~WriteOptions.NewLineAfter);

            for (int i = 1; i < lines.Length - 1; i++)
                Write(lines[i].WarningHighlight(), innerOptions | WriteOptions.NewLineAfter);

            Write((lines[lines.Length - 1] + " */").WarningHighlight(),
                (options & WriteOptions.NewLineAfter) | innerOptions);

            return this;
        }

        /// <summary>
        /// Adds 
        /// <list>
        /// <item>// {<tt>message</tt>}</item> 
        /// <item>or</item> 
        /// <item>/* {<tt>message</tt>} */</item>
        /// </list> 
        /// with a comment highlight
        /// </summary>
        public CodeWriter Comment(string message, WriteOptions options = WriteOptions.None)
        {
            if (string.IsNullOrEmpty(message))
                return this;

            string[] lines = message.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);


            if (lines.Length == 1)
            {
                Write($"// {lines[0]}".CommentHighlight(), options);
                return this;
            }

            WriteOptions innerOptions = options & WriteOptions.Indented;

            Write(("/* " + lines[0]).CommentHighlight(), options & ~WriteOptions.NewLineAfter);

            for (int i = 1; i < lines.Length - 1; i++)
                Write(lines[i].CommentHighlight(), innerOptions | WriteOptions.NewLineAfter);

            Write((lines[lines.Length - 1] + " */").CommentHighlight(),
                (options & WriteOptions.NewLineAfter) | innerOptions);

            return this;
        }

        public CodeWriter WriteDiagnosticComment(string message, CodeDiagnosticKind kind, WriteOptions options = WriteOptions.None)
        {
            switch (kind)
            {
                case CodeDiagnosticKind.Info:
                    Comment(message, options);
                    return this;
                case CodeDiagnosticKind.Warning:
                    Warning(message, options);
                    return this;
                case CodeDiagnosticKind.Recommendation:
                    Recommendation(message, options);
                    return this;
                default:
                    Error(message, options);
                    return this;
            }
        }

        public CodeWriter Break(WriteOptions options = WriteOptions.None)
        {
            Write("break".ControlHighlight(), options);
            WriteEnd(EndWriteOptions.LineEnd);
            return this;
        }

        public CodeWriter Return(ValueParameter value, WriteOptions options = WriteOptions.None)
        {
            var newOptions = options & ~WriteOptions.NewLineAfter;

            Write("return ".ControlHighlight(), newOptions);

            if (value.Write != null)
            {
                value.Write(this);
            }
            else if (value.StringValue != null)
            {
                Write(value.StringValue);
            }

            Write(";");

            if ((options & WriteOptions.NewLineAfter) != 0)
                NewLine();

            return this;
        }

        public CodeWriter YieldReturn(ValueParameter value, WriteOptions options = WriteOptions.None)
        {
            var newOptions = options & ~WriteOptions.NewLineAfter;

            Write("yield return ".ControlHighlight(), newOptions);

            if (value.Write != null)
            {
                value.Write(this);
            }
            else if (value.StringValue != null)
            {
                Write(value.StringValue);
            }

            Write(";");

            if ((options & WriteOptions.NewLineAfter) != 0)
                NewLine();

            return this;
        }

        /// <summary>
        /// Casts the code that was written in this scope.
        /// </summary>
        /// <remarks>
        /// This only works for code that is written immediately inside this scope.
        /// Do not use this for values that are generated later (for example, wrapping
        /// GenerateValue), because the cast will cause the C# Preview highlights to
        /// mismatch when clicked
        /// If you need to cast a value that is generated later, use
        /// NodeGenerator.GenerateNextValueCasted instead.
        /// </remarks>
        /// <param name="castType">The type to cast to.</param>
        /// <param name="shouldCast">Determines whether the cast should be applied.</param>
        /// <param name="wrapInParentheses">If true, generates ((Type)Code) instead of (Type)Code.</param>
        public IDisposable Cast(Type castType, Func<bool> shouldCast, bool wrapInParentheses)
        {
            return new CastScope(this, castType, shouldCast, wrapInParentheses);
        }

        /// <summary>
        /// Casts the code that was written in this scope.
        /// </summary>
        /// <remarks>
        /// This only works for code that is written immediately inside this scope.
        /// Do not use this for values that are generated later (for example, wrapping
        /// GenerateValue), because the cast will cause the C# Preview highlights to
        /// mismatch when clicked. 
        /// If you need to cast a value that is generated later, use
        /// NodeGenerator.GenerateNextValueCasted instead.
        /// </remarks>
        /// <param name="castType">The type to cast as.</param>
        /// <param name="wrapInParentheses">Generate ((Type)Code) instead of (Type)Code</param>
        public IDisposable Cast(Type castType, bool wrapInParentheses)
        {
            return new CastScope(this, castType, () => true, wrapInParentheses);
        }

        public CodeWriter WriteDiagnostic(string diagnosticMessage, string displayText, CodeDiagnosticKind kind, WriteOptions writeOptions = WriteOptions.None)
        {
            return WriteDiagnostic(diagnosticMessage, displayText, kind, true, writeOptions);
        }

        public CodeWriter WriteErrorDiagnostic(string diagnosticMessage, string displayText, WriteOptions writeOptions = WriteOptions.None)
        {
            return WriteDiagnostic(diagnosticMessage, displayText, CodeDiagnosticKind.Error, true, writeOptions);
        }

        public CodeWriter WriteRecommendationDiagnostic(string diagnosticMessage, string displayText, WriteOptions writeOptions = WriteOptions.None)
        {
            return WriteDiagnostic(diagnosticMessage, displayText, CodeDiagnosticKind.Recommendation, true, writeOptions);
        }

        public CodeWriter WriteInfoDiagnostic(string diagnosticMessage, string displayText, WriteOptions writeOptions = WriteOptions.None)
        {
            return WriteDiagnostic(diagnosticMessage, displayText, CodeDiagnosticKind.Info, true, writeOptions);
        }

        public CodeWriter WriteWarningDiagnostic(string diagnosticMessage, string displayText, WriteOptions writeOptions = WriteOptions.None)
        {
            return WriteDiagnostic(diagnosticMessage, displayText, CodeDiagnosticKind.Warning, true, writeOptions);
        }
    }
}