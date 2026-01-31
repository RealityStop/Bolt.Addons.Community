using Unity.VisualScripting.Community.Libraries.Humility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.VisualScripting.Community.CSharp;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    [RenamedFrom("Bolt.Addons.Community.Libraries.CSharp.MethodGenerator")]
    public sealed class MethodGenerator : BodyGenerator
    {
        public AccessModifier scope;
        public MethodModifier modifier;
        public string name;
        public Type returnType;
        public string stringReturnType;
        public string stringReturnTypeNamespace;
        public List<ParameterGenerator> parameters = new List<ParameterGenerator>();
        public List<AttributeGenerator> attributes = new List<AttributeGenerator>();
        public List<GenericDeclaration> generics = new List<GenericDeclaration>();
        private Queue<Action<CodeWriter>> bodyActions = new Queue<Action<CodeWriter>>();
        private Queue<Action<CodeWriter>> beforeBodyActions = new Queue<Action<CodeWriter>>();
        public event Action<CodeWriter> bodyAction
        {
            add
            {
                bodyActions.Enqueue(value);
            }
            remove
            {
                throw new NotSupportedException("Cannot remove bodyAction");
            }
        }
        public event Action<CodeWriter> beforeBodyAction
        {
            add
            {
                beforeBodyActions.Enqueue(value);
            }
            remove
            { throw new NotSupportedException("Cannot remove beforeBodyAction"); }
        }
        public string warning;
        public string summary;

        private MethodGenerator() { }

        public static MethodGenerator Method(AccessModifier scope, MethodModifier modifier, Type returnType, string name)
        {
            var method = new MethodGenerator();
            method.scope = scope;
            method.modifier = modifier;
            method.name = name;
            method.returnType = returnType;
            return method;
        }

        public static MethodGenerator Method(AccessModifier scope, MethodModifier modifier, string returnType, string returnTypeNamespace, string name)
        {
            var method = new MethodGenerator();
            method.scope = scope;
            method.modifier = modifier;
            method.name = name;
            method.stringReturnType = returnType;
            method.stringReturnTypeNamespace = returnTypeNamespace;
            return method;
        }



        protected override sealed void GenerateBefore(CodeWriter writer, ControlGenerationData data)
        {
            var attributes = string.Empty;
            foreach (AttributeGenerator attr in this.attributes)
            {
                attr.Generate(writer, data);
                writer.NewLine();
            }

            if (!string.IsNullOrEmpty(warning))
            {
                writer.WriteLine($"/* {warning} */".ErrorHighlight());
                writer.NewLine();
            }

            if (!string.IsNullOrEmpty(summary))
            {
                writer.WriteIndented("/// <summary>".CommentHighlight());

                //foreach (var line in summary.Split('\n', StringSplitOptions.RemoveEmptyEntries))

                foreach (var line in summary.Split('\n').Where(s => !string.IsNullOrEmpty(s)))
                {
                    writer.NewLine();
                    writer.WriteIndented($"/// {line}".CommentHighlight());
                }

                writer.NewLine();
                writer.WriteIndented($"/// </summary>".CommentHighlight());
                writer.NewLine();
            }

            var modSpace = modifier == MethodModifier.None ? string.Empty : " ";
            var genericTypes = generics.Count > 0 ? $"<{string.Join(", ", generics.Select(g => g.name.TypeHighlight()))}>" : string.Empty;
            var constraints = generics.Count > 0 && generics.Any(g => g.baseTypeConstraint.type != typeof(object) || g.interfaceConstraints.Count > 0 || g.typeParameterConstraints != TypeParameterConstraints.None) ? $" {"where".ConstructHighlight()} " : "";
            foreach (var generic in generics)
            {
                if (generic.baseTypeConstraint.type != typeof(object) || generic.interfaceConstraints.Count > 0 || generic.typeParameterConstraints != TypeParameterConstraints.None)
                {
                    if (generic.baseTypeConstraint.type != typeof(object))
                    {
                        constraints += $"{generic.name.TypeHighlight()} : {writer.GetTypeNameHighlighted(generic.baseTypeConstraint.type)}" + (generic.interfaceConstraints.Count > 0 ? ", " + string.Join(", ", generic.interfaceConstraints.Select(i => writer.GetTypeNameHighlighted(i.type))) : string.Empty) + (generic.typeParameterConstraints != TypeParameterConstraints.None ? ", " + GetSelectedConstraints(generic.typeParameterConstraints) : string.Empty);
                    }
                    else if (generic.interfaceConstraints.Count > 0)
                    {
                        constraints += $"{generic.name.TypeHighlight()} : {string.Join(", ", generic.interfaceConstraints.Select(i => writer.GetTypeNameHighlighted(i.type)))}" + (generic.typeParameterConstraints != TypeParameterConstraints.None ? ", " + GetSelectedConstraints(generic.typeParameterConstraints) : string.Empty);
                    }
                    else if (generic.typeParameterConstraints != TypeParameterConstraints.None)
                    {
                        constraints += $"{generic.name.TypeHighlight()} : {GetSelectedConstraints(generic.typeParameterConstraints)}";
                    }
                }
            }
            writer.WriteLine((scope == AccessModifier.None ? "" : scope.AsString().ToLower().ConstructHighlight() + " ") + modifier.AsString().ConstructHighlight() + modSpace + (string.IsNullOrEmpty(stringReturnType) ? writer.GetTypeNameHighlighted(returnType) : stringReturnType) + " " + name.LegalMemberName() + genericTypes + CodeBuilder.Parameters(parameters, writer, data) + constraints);
        }

        string GetSelectedConstraints(TypeParameterConstraints constraints)
        {
            List<string> selected = new List<string>();

            if (constraints == TypeParameterConstraints.None)
                return "";
            if ((constraints & TypeParameterConstraints.Class) != 0)
                selected.Add("class".ConstructHighlight());
            if ((constraints & TypeParameterConstraints.Struct) != 0)
                selected.Add("struct".ConstructHighlight());
            if ((constraints & TypeParameterConstraints.New) != 0)
                selected.Add("new".ConstructHighlight() + "()");

            return string.Join(", ", selected);
        }

        protected override sealed void GenerateBody(CodeWriter writer, ControlGenerationData data)
        {
            if (string.IsNullOrEmpty(name)) return;

            var indent = writer.IndentLevel;

            data.EnterMethod();

            while (beforeBodyActions.Count > 0)
                beforeBodyActions.Dequeue()?.Invoke(writer);
            while (bodyActions.Count > 0)
                bodyActions.Dequeue()?.Invoke(writer);

            data.ExitMethod();
        }

        protected override sealed void GenerateAfter(CodeWriter writer, ControlGenerationData data)
        {
        }

        public MethodGenerator AddAttribute(AttributeGenerator generator)
        {
            attributes.Add(generator);
            return this;
        }

        public MethodGenerator Body(Action<CodeWriter> bodyAction)
        {
            bodyActions.Clear();
            bodyActions.Enqueue(bodyAction);
            return this;
        }

        public MethodGenerator AddToBody(Action<CodeWriter> bodyAction)
        {
            this.bodyAction += bodyAction;
            return this;
        }

        public bool HasBody()
        {
            return beforeBodyActions.Count > 0 || bodyActions.Count > 0;
        }

        public MethodGenerator AppendBodyFrom(MethodGenerator other)
        {
            foreach (Action<CodeWriter> action in other.beforeBodyActions)
            {
                beforeBodyActions.Enqueue(action);
            }

            foreach (Action<CodeWriter> action in other.bodyActions)
            {
                bodyActions.Enqueue(action);
            }

            return this;
        }

        public MethodGenerator AddGeneric()
        {
            var count = generics.Count == 1 ? "" : (generics.Count - 1).ToString();
            generics.Add(new GenericDeclaration("T" + count));
            return this;
        }

        public MethodGenerator AddGenerics(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (i == 0)
                    generics.Add(new GenericDeclaration("T"));
                else
                    generics.Add(new GenericDeclaration("T" + i));
            }
            return this;
        }

        public MethodGenerator AddGenerics(params GenericParameter[] parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                generics.Add(new GenericDeclaration(parameters[i]));
            }
            return this;
        }

        public MethodGenerator AddParameter(ParameterGenerator parameter)
        {
            parameters.Add(parameter);
            return this;
        }

        public MethodGenerator SetWarning(string warning)
        {
            this.warning = warning;
            return this;
        }

        public MethodGenerator SetSummary(string summary)
        {
            this.summary = summary;
            return this;
        }

        public override List<string> Usings()
        {
            var usings = new List<string>();

            if (!string.IsNullOrEmpty(stringReturnType) && !string.IsNullOrEmpty(stringReturnTypeNamespace) && !usings.Contains(stringReturnTypeNamespace))
                usings.Add(stringReturnTypeNamespace);

            else if (returnType != null && !usings.Contains(returnType.Namespace) && !returnType.Is().PrimitiveStringOrVoid()) usings.Add(returnType.GetNamespace());

            for (int i = 0; i < attributes.Count; i++)
            {
                usings.MergeUnique(attributes[i].Usings());
            }

            foreach (var generic in generics)
            {
                if (!usings.Contains(generic.baseTypeConstraint.type.Namespace))
                    usings.AddRange(generic.baseTypeConstraint.type.GetAllNamespaces());

                foreach (var interfaceConstraint in generic.interfaceConstraints)
                {
                    if (!usings.Contains(interfaceConstraint.type.Namespace))
                        usings.AddRange(interfaceConstraint.type.GetAllNamespaces());
                }
            }

            for (int i = 0; i < parameters.Count; i++)
            {
                if (!parameters[i].useAssemblyQualifiedType && !parameters[i].type.Is().PrimitiveStringOrVoid()) usings.MergeUnique(parameters[i].Usings());
            }

            return usings;
        }
    }
}