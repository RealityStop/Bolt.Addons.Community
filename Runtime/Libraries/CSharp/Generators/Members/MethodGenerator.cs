using Unity.VisualScripting.Community.Libraries.Humility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        public string body = "";
        public string beforeBody;
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
        protected override sealed string GenerateBefore(int indent)
        {
            var attributes = string.Empty;
            foreach (AttributeGenerator attr in this.attributes)
            {
                attributes += attr.Generate(indent) + "\n";
            }
            var _warning = !string.IsNullOrEmpty(warning) ? CodeBuilder.Indent(indent) + $"/* {warning} */\n".WarningHighlight() : string.Empty;
            var _summary = string.Empty;
            if (!string.IsNullOrEmpty(summary))
            {
                _summary = CodeBuilder.Indent(indent) + "/// <summary>".CommentHighlight();
                foreach (var line in summary.Split('\n', StringSplitOptions.RemoveEmptyEntries))
                {
                    _summary += "\n" + CodeBuilder.Indent(indent) + $"/// {line}".CommentHighlight();
                }
                _summary += "\n" + CodeBuilder.Indent(indent) + $"/// </summary>".CommentHighlight() + "\n";
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
                        constraints += $"{generic.name.TypeHighlight()} : {generic.baseTypeConstraint.type.As().CSharpName(false, true)}" + (generic.interfaceConstraints.Count > 0 ? ", " + string.Join(", ", generic.interfaceConstraints.Select(i => i.type.As().CSharpName(false, true))) : string.Empty) + (generic.typeParameterConstraints != TypeParameterConstraints.None ? ", " + GetSelectedConstraints(generic.typeParameterConstraints) : string.Empty);
                    }
                    else if (generic.interfaceConstraints.Count > 0)
                    {
                        constraints += $"{generic.name.TypeHighlight()} : {string.Join(", ", generic.interfaceConstraints.Select(i => i.type.As().CSharpName(false, true)))}" + (generic.typeParameterConstraints != TypeParameterConstraints.None ? ", " + GetSelectedConstraints(generic.typeParameterConstraints) : string.Empty);
                    }
                    else if (generic.typeParameterConstraints != TypeParameterConstraints.None)
                    {
                        constraints += $"{generic.name.TypeHighlight()} : {GetSelectedConstraints(generic.typeParameterConstraints)}";
                    }
                }
            }
            return attributes + _warning + _summary + CodeBuilder.Indent(indent) + (scope == AccessModifier.None ? "" : scope.AsString().ToLower().ConstructHighlight() + " ") + modifier.AsString().ConstructHighlight() + modSpace + (string.IsNullOrEmpty(stringReturnType) ? returnType.As().CSharpName(false, true) : stringReturnType) + " " + name.LegalMemberName() + genericTypes + CodeBuilder.Parameters(this.parameters) + constraints;
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

        protected override sealed string GenerateBody(int indent)
        {
            if (string.IsNullOrEmpty(name)) { return string.Empty; }
            return string.IsNullOrEmpty(body) ? string.Empty : body.Contains("\n") ? beforeBody + body.Replace("\n", "\n" + CodeBuilder.Indent(indent)).Insert(0, CodeBuilder.Indent(indent)) : CodeBuilder.Indent(indent) + beforeBody + "\n" + CodeBuilder.Indent(indent) + body;
        }

        protected override sealed string GenerateAfter(int indent)
        {
            return string.Empty;
        }

        public MethodGenerator AddAttribute(AttributeGenerator generator)
        {
            attributes.Add(generator);
            return this;
        }

        public MethodGenerator Body(string body)
        {
            this.body = body;
            return this;
        }

        public MethodGenerator AddToBody(string body)
        {
            this.body += body;
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
            else if (returnType != null && !usings.Contains(returnType.Namespace) && !returnType.Is().PrimitiveStringOrVoid()) usings.Add(returnType.Namespace);

            for (int i = 0; i < attributes.Count; i++)
            {
                usings.MergeUnique(attributes[i].Usings());
            }

            foreach (var generic in generics)
            {
                if (!usings.Contains(generic.baseTypeConstraint.type.Namespace))
                    usings.Add(generic.baseTypeConstraint.type.Namespace);

                foreach (var interfaceConstraint in generic.interfaceConstraints)
                {
                    if (!usings.Contains(interfaceConstraint.type.Namespace))
                        usings.Add(interfaceConstraint.type.Namespace);
                }
            }

            for (int i = 0; i < parameters.Count; i++)
            {
                if (!parameters[i].useAssemblyQualifiedType && !usings.Contains(parameters[i].Using()) && !parameters[i].type.Is().PrimitiveStringOrVoid()) usings.MergeUnique(parameters[i].Usings());
            }

            return usings;
        }
    }
}