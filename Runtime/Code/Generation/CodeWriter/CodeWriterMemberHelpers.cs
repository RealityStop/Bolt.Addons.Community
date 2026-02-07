using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    public sealed partial class CodeWriter
    {
        /// <summary>
        /// A useful Shortcut for using Actions for a Parameter in the Member Methods.
        /// To avoid needing to do: <code>(Action&lt;CodeWriter&gt;)(writer => ActionCode)</code>
        /// </summary>
        public Action<CodeWriter> Action(Action<CodeWriter> action)
        {
            return action;
        }

        /// <summary>
        /// A useful Shortcut for using Actions for a Parameter in the Member Methods.
        /// To avoid needing to do: <code>(Action&lt;CodeWriter&gt;)(writer => ActionCode)</code>
        /// </summary>
        public Action<CodeWriter> Action(Action action)
        {
            return Action(w => action());
        }

        /// <summary>
        /// A helper to write a GetMember expression.
        /// <list>
        /// <listheader><tt><paramref name="member"/></tt> allows:</listheader>
        /// <item>String</item>
        /// <item>Action&lt;CodeWriter&gt; - (see <see cref="Action"/>)</item>
        /// </list>
        /// </summary>
        public CodeWriter GetMember(MemberParameter member)
        {
            if (member == null) throw new InvalidOperationException();

            Dot();

            if (member.Write != null)
            {
                member.Write(this);
            }
            else
            {
                Write(member.StringValue.VariableHighlight());
            }

            return this;
        }

        /// <summary>
        /// A helper to write a GetMember expression.
        /// <list>
        /// <listheader><tt><paramref name="target"/></tt> allows:</listheader>
        /// <item>String</item>
        /// <item>Type</item>
        /// <item>Action&lt;CodeWriter&gt; - (see <see cref="Action"/>)</item>
        /// </list>
        /// <list>
        /// <listheader><tt><paramref name="member"/></tt> allows:</listheader>
        /// <item>String</item>
        /// <item>Action&lt;CodeWriter&gt; - (see <see cref="Action"/>)</item>
        /// </list>
        /// </summary>
        public CodeWriter GetMember(TargetParameter target, MemberParameter member)
        {
            if (target != null)
            {
                if (target.Write != null)
                {
                    target.Write(this);
                }
                else if (target.TypeValue != null)
                {
                    Write(target.TypeValue);
                }
                else
                {
                    Write(target.StringValue);
                }
            }

            if (member == null) throw new InvalidOperationException();

            Dot();

            if (member.Write != null)
            {
                member.Write(this);
            }
            else
            {
                Write(member.StringValue.VariableHighlight());
            }

            return this;
        }

        /// <summary>
        /// A helper to write a InvokeMember expression.
        /// <list>
        /// <listheader><tt><paramref name="target"/></tt> allows:</listheader>
        /// <item>String</item>
        /// <item>Type</item>
        /// <item>Action&lt;CodeWriter&gt; - (see <see cref="Action"/>)</item>
        /// </list>
        /// <list>
        /// <listheader><tt><paramref name="member"/></tt> allows:</listheader>
        /// <item>String</item>
        /// <item>Action&lt;CodeWriter&gt; - (see <see cref="Action"/>)</item>
        /// </list>
        /// <list>
        /// <listheader><tt><paramref name="parameters"/></tt> allows:</listheader>
        /// <item>String</item>
        /// <item>Action&lt;CodeWriter&gt; - (see <see cref="Action"/>)</item>
        /// </list>
        /// </summary>
        public CodeWriter InvokeMember(TargetParameter target, MemberParameter member, params MethodParameter[] parameters)
        {
            InvokeMember(target, member, null, parameters);
            return this;
        }

        /// <summary>
        /// A helper to write a GetComponent&lt;T&gt;() call.
        /// <list>
        /// <listheader><tt><paramref name="target"/></tt> allows:</listheader>
        /// <item>String</item>
        /// <item>Type</item>
        /// <item>Action&lt;CodeWriter&gt; - (see <see cref="Action"/>)</item>
        /// </list>
        /// <list>
        /// <listheader><tt><paramref name="type"/></tt> allows:</listheader>
        /// <item>String</item>
        /// <item>Type</item>
        /// <item>Action&lt;CodeWriter&gt; - (see <see cref="Action"/>)</item>
        /// </list>
        /// </summary>
        public CodeWriter GetComponent(TargetParameter target, TypeParameter type)
        {
            InvokeMember(target, "GetComponent", new TypeParameter[] { type }, new MethodParameter[0]);
            return this;
        }

        /// <summary>
        /// A helper to write a GetComponent&lt;T&gt;() call.
        /// <list>
        /// <listheader><tt><paramref name="target"/></tt> allows:</listheader>
        /// <item>String</item>
        /// <item>Type</item>
        /// <item>Action&lt;CodeWriter&gt; - (see <see cref="Action"/>)</item>
        /// </list>
        /// <list>
        /// <listheader><tt><paramref name="type"/></tt> allows:</listheader>
        /// <item>String</item>
        /// <item>Type</item>
        /// <item>Action&lt;CodeWriter&gt; - (see <see cref="Action"/>)</item>
        /// </list>
        /// </summary>
        public CodeWriter GetComponent(TypeParameter type)
        {
            type.fullName = true;
            InvokeMember(null, "GetComponent", new TypeParameter[] { type }, new MethodParameter[0]);
            return this;
        }

        /// <summary>
        /// A helper to write a InvokeMember expression.
        /// <list>
        /// <listheader><tt><paramref name="target"/></tt> allows:</listheader>
        /// <item>String</item>
        /// <item>Type</item>
        /// <item>Action&lt;CodeWriter&gt; - (see <see cref="Action"/>)</item>
        /// </list>
        /// <list>
        /// <listheader><tt><paramref name="member"/></tt> allows:</listheader>
        /// <item>String</item>
        /// <item>Action&lt;CodeWriter&gt; - (see <see cref="Action"/>)</item>
        /// </list>
        /// <list>
        /// <listheader><tt><paramref name="generics"/></tt> allows:</listheader>
        /// <item>String</item>
        /// <item>Type</item>
        /// <item>Action&lt;CodeWriter&gt; - (see <see cref="Action"/>)</item>
        /// </list>
        /// <list>
        /// <listheader><tt><paramref name="parameters"/></tt> allows:</listheader>
        /// <item>String</item>
        /// <item>Action&lt;CodeWriter&gt; - (see <see cref="Action"/>)</item>
        /// </list>
        /// </summary>
        public CodeWriter InvokeMember(TargetParameter target, MemberParameter member, TypeParameter[] generics, params MethodParameter[] parameters)
        {
            if (target != null)
            {
                if (target.Write != null)
                {
                    target.Write(this);
                }
                else if (target.TypeValue != null)
                {
                    Write(target.TypeValue);
                }
                else
                {
                    Write(target.StringValue);
                }
            }

            if (member == null) throw new InvalidOperationException();

            Dot();

            if (member.Write != null)
            {
                member.Write(this);
            }
            else
            {
                Write(member.StringValue);
            }

            if (generics != null && generics.Length > 0)
            {
                Write("<");
                for (int i = 0; i < generics.Length; i++)
                {
                    var generic = generics[i];
                    if (i != 0) ParameterSeparator();
                    if (generic.Write != null)
                    {
                        generic.Write(this);
                    }
                    else if (generic.TypeValue != null)
                    {
                        Write(GetTypeNameHighlighted(generic.TypeValue, generic.fullName));
                    }
                    else
                    {
                        Write(generic.StringValue);
                    }
                }
                Write(">");
            }

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
                        inner.Write(param.StringValue);
                    }

                    if (i < parameters.Length - 1)
                    {
                        inner.ParameterSeparator();
                    }
                }
            });

            return this;
        }

        /// <summary>
        /// A helper to write a Object Creation expression.
        /// <list>
        /// <listheader><tt><paramref name="target"/></tt> allows:</listheader>
        /// <item>String</item>
        /// <item>Type</item>
        /// <item>Action&lt;CodeWriter&gt; - (see <see cref="Action"/>)</item>
        /// </list>
        /// <list>
        /// <listheader><tt><paramref name="parameters"/></tt> allows:</listheader>
        /// <item>String</item>
        /// <item>Action&lt;CodeWriter&gt; - (see <see cref="Action"/>)</item>
        /// </list>
        /// </summary>
        public CodeWriter New(TargetParameter target, params MethodParameter[] parameters)
        {
            Write("new".ConstructHighlight()).Space();

            if (target != null)
            {
                if (target.Write != null)
                {
                    target.Write(this);
                }
                else if (target.TypeValue != null)
                {
                    Write(target.TypeValue);
                }
                else
                {
                    Write(target.StringValue);
                }
            }

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
                        inner.Write(param.StringValue);
                    }

                    if (i < parameters.Length - 1)
                    {
                        inner.ParameterSeparator();
                    }
                }
            });

            return this;
        }

        /// <summary>
        /// A helper to write a Array Creation expression.
        /// <list>
        /// <listheader><tt><paramref name="target"/></tt> allows:</listheader>
        /// <item>String</item>
        /// <item>Type</item>
        /// <item>Action&lt;CodeWriter&gt; - (see <see cref="Action"/>)</item>
        /// </list>
        /// <list>
        /// <listheader><tt><paramref name="parameters"/></tt> allows:</listheader>
        /// <item>String</item>
        /// <item>Action&lt;CodeWriter&gt; - (see <see cref="Action"/>)</item>
        /// </list>
        /// </summary>
        public CodeWriter NewArray(TargetParameter target, params DimensionParameter[] parameters)
        {
            Write("new".ConstructHighlight()).Space();

            if (target != null)
            {
                if (target.Write != null)
                {
                    target.Write(this);
                }
                else if (target.TypeValue != null)
                {
                    Write(target.TypeValue);
                }
                else
                {
                    Write(target.StringValue);
                }
            }

            Brackets(inner =>
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
                        inner.Write(param.StringValue);
                    }

                    if (i < parameters.Length - 1)
                    {
                        inner.ParameterSeparator();
                    }
                }
            });

            return this;
        }

        /// <summary>
        /// A helper to write a variable creation expression: Type name = Value;\n.
        /// <list>
        /// <listheader><tt><paramref name="type"/></tt> allows:</listheader>
        /// <item>Type</item>
        /// <item>String</item>
        /// <item>Action&lt;CodeWriter&gt; - (see <see cref="Action"/>)</item>
        /// </list>
        /// <list>
        /// <listheader><tt><paramref name="name"/></tt> allows:</listheader>
        /// <item>String</item>
        /// <item>Action&lt;CodeWriter&gt; - (see <see cref="Action"/>)</item>
        /// </list>
        /// <list>
        /// <listheader><tt><paramref name="value"/></tt> allows:</listheader>
        /// <item>String</item>
        /// <item>Action&lt;CodeWriter&gt; - (see <see cref="Action"/>)</item>
        /// </list>
        /// </summary>
        public CodeWriter CreateVariable(TypeParameter type, MemberParameter name, ValueParameter value, WriteOptions writeOptions = WriteOptions.Indented, EndWriteOptions endWriteOptions = EndWriteOptions.LineEnd)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (name == null) throw new ArgumentNullException("name");

            if ((writeOptions & WriteOptions.NewLineBefore) != 0)
                NewLine();

            if ((writeOptions & WriteOptions.Indented) != 0)
                WriteIndented();

            if (type.Write != null)
            {
                type.Write(this);
            }
            else if (type.TypeValue != null)
            {
                Write(GetTypeNameHighlighted(type.TypeValue, !type.fullName));
            }
            else
            {
                Write(type.StringValue);
            }

            Space();

            if (name.Write != null)
            {
                name.Write(this);
            }
            else
            {
                Write(name.StringValue.VariableHighlight());
            }

            Equal();

            if (value != null)
            {
                if (value.Write != null)
                {
                    value.Write(this);
                }
                else
                {
                    Write(value.StringValue);
                }
            }
            else
            {
                Null();
            }

            WriteEnd(endWriteOptions);

            if ((writeOptions & WriteOptions.NewLineAfter) != 0)
                NewLine();

            return this;
        }

        /// <summary>
        /// A helper to write a variable creation expression: var name = Value;\n.
        /// <list>
        /// <listheader><tt><paramref name="type"/></tt> allows:</listheader>
        /// <item>Type</item>
        /// <item>String</item>
        /// <item>Action&lt;CodeWriter&gt; - (see <see cref="Action"/>)</item>
        /// </list>
        /// <list>
        /// <listheader><tt><paramref name="name"/></tt> allows:</listheader>
        /// <item>String</item>
        /// <item>Action&lt;CodeWriter&gt; - (see <see cref="Action"/>)</item>
        /// </list>
        /// <list>
        /// <listheader><tt><paramref name="value"/></tt> allows:</listheader>
        /// <item>String</item>
        /// <item>Action&lt;CodeWriter&gt; - (see <see cref="Action"/>)</item>
        /// </list>
        /// </summary>
        public CodeWriter CreateVariable(MemberParameter name, ValueParameter value, WriteOptions writeOptions = WriteOptions.None, EndWriteOptions endWriteOptions = EndWriteOptions.LineEnd)
        {
            CreateVariable("var".ConstructHighlight(), name, value, writeOptions, endWriteOptions);
            return this;
        }

        /// <summary>
        /// A helper to write a set variable expression: name = Value;\n.
        /// <list>
        /// <listheader><tt><paramref name="name"/></tt> allows:</listheader>
        /// <item>String</item>
        /// <item>Action&lt;CodeWriter&gt; - (see <see cref="Action"/>)</item>
        /// </list>
        /// <list>
        /// <listheader><tt><paramref name="value"/></tt> allows:</listheader>
        /// <item>String</item>
        /// <item>Action&lt;CodeWriter&gt; - (see <see cref="Action"/>)</item>
        /// </list>
        /// </summary>
        public CodeWriter SetVariable(MemberParameter name, ValueParameter value, WriteOptions writeOptions = WriteOptions.Indented, EndWriteOptions endWriteOptions = EndWriteOptions.LineEnd)
        {
            if (name == null) throw new ArgumentNullException("name");

            if ((writeOptions & WriteOptions.NewLineBefore) != 0)
                NewLine();

            if ((writeOptions & WriteOptions.Indented) != 0)
                WriteIndented();

            if (name.Write != null)
            {
                name.Write(this);
            }
            else
            {
                Write(name.StringValue.VariableHighlight());
            }

            Equal();

            if (value != null)
            {
                if (value.Write != null)
                {
                    value.Write(this);
                }
                else
                {
                    Write(value.StringValue);
                }
            }
            else
            {
                Null();
            }

            WriteEnd(endWriteOptions);

            if ((writeOptions & WriteOptions.NewLineAfter) != 0)
                NewLine();

            return this;
        }

        /// <summary>
        /// A helper to write a Get Variable: name.
        /// <list>
        /// <listheader><tt><paramref name="name"/></tt> allows:</listheader>
        /// <item>String</item>
        /// <item>Action&lt;CodeWriter&gt; - (see <see cref="Action"/>)</item>
        /// </list>
        /// </summary>
        public CodeWriter GetVariable(MemberParameter name)
        {
            if (name == null) throw new ArgumentNullException("name");

            if (name.Write != null)
            {
                name.Write(this);
            }
            else
            {
                Write(name.StringValue.VariableHighlight());
            }

            return this;
        }

        /// <summary>
        /// A helper to write .ConvertTo&lt;T&gt;().
        /// <list>
        /// <listheader><tt><paramref name="targetType"/></tt> allows:</listheader>
        /// <item>Type</item>
        /// <item>String</item>
        /// <item>Action&lt;CodeWriter&gt; - (see <see cref="Action"/>)</item>
        /// </list>
        /// <tt><paramref name="shouldConvert"/></tt>: Should it write the convert or not.
        /// </summary>
        public CodeWriter WriteConvertTo(TypeParameter targetType, bool shouldConvert)
        {
            if (targetType == null || !shouldConvert) return this;

            using (CaptureWrite(out _))
            {
                Dot().Write("ConvertTo<");
                if (targetType.Write != null)
                {
                    targetType.Write(this);
                }
                else if (targetType.TypeValue != null)
                {
                    Write(GetTypeNameHighlighted(targetType.TypeValue, !targetType.fullName));
                }
                else
                {
                    Write(targetType.StringValue);
                }
                Write(">").Parentheses();
            }

            return this;
        }

        public sealed class TargetParameter
        {
            public Action<CodeWriter> Write;
            public string StringValue;
            public Type TypeValue;

            public static implicit operator TargetParameter(Action<CodeWriter> Write)
            {
                return new TargetParameter { Write = Write };
            }

            public static implicit operator TargetParameter(string StringValue)
            {
                return new TargetParameter { StringValue = StringValue ?? "" };
            }

            public static implicit operator TargetParameter(Type TypeValue)
            {
                return new TargetParameter { TypeValue = TypeValue };
            }
        }

        public sealed class MemberParameter
        {
            public Action<CodeWriter> Write;
            public string StringValue;


            public static implicit operator MemberParameter(Action<CodeWriter> Write)
            {
                return new MemberParameter { Write = Write };
            }

            public static implicit operator MemberParameter(string StringValue)
            {
                return new MemberParameter { StringValue = StringValue ?? "" };
            }
        }

        public sealed class MethodParameter
        {
            public Action<CodeWriter> Write;
            public string StringValue;


            public static implicit operator MethodParameter(Action<CodeWriter> Write)
            {
                return new MethodParameter { Write = Write };
            }

            public static implicit operator MethodParameter(string StringValue)
            {
                return new MethodParameter { StringValue = StringValue ?? "" };
            }
        }

        public sealed class DimensionParameter
        {
            public Action<CodeWriter> Write;
            public string StringValue;

            public static implicit operator DimensionParameter(Action<CodeWriter> Write)
            {
                return new DimensionParameter { Write = Write };
            }

            public static implicit operator DimensionParameter(string StringValue)
            {
                return new DimensionParameter { StringValue = StringValue ?? "" };
            }
        }

        public sealed class ArrayValueParameter
        {
            public Action<CodeWriter> Write;
            public string StringValue;


            public static implicit operator ArrayValueParameter(Action<CodeWriter> Write)
            {
                return new ArrayValueParameter { Write = Write };
            }

            public static implicit operator ArrayValueParameter(string StringValue)
            {
                return new ArrayValueParameter { StringValue = StringValue ?? "" };
            }
        }

        public sealed class TypeParameter
        {
            public Action<CodeWriter> Write;
            public Type TypeValue;
            public string StringValue;

            public bool fullName;

            public TypeParameter() { }

            public TypeParameter(bool fullName)
            {
                this.fullName = fullName;
            }

            public static implicit operator TypeParameter(Action<CodeWriter> Write)
            {
                return new TypeParameter { Write = Write };
            }

            public static implicit operator TypeParameter(Type TypeValue)
            {
                return new TypeParameter { TypeValue = TypeValue };
            }

            public static implicit operator TypeParameter(string StringValue)
            {
                return new TypeParameter { StringValue = StringValue ?? "" };
            }
        }

        public sealed class ValueParameter
        {
            public Action<CodeWriter> Write;
            public string StringValue;

            public static implicit operator ValueParameter(Action<CodeWriter> Write)
            {
                return new ValueParameter { Write = Write };
            }

            public static implicit operator ValueParameter(string StringValue)
            {
                return new ValueParameter { StringValue = StringValue ?? "" };
            }
        }
    }
}