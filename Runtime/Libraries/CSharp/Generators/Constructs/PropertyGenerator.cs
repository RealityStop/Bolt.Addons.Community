using Unity.VisualScripting.Community.Libraries.Humility;
using System;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    [RenamedFrom("Bolt.Addons.Community.Libraries.CSharp.PropertyGenerator")]
    public sealed class PropertyGenerator : MemberGenerator
    {
#pragma warning disable 0649
        public PropertyModifier modifier;
        public AccessModifier getterScope;
        public AccessModifier setterScope;
        public bool hasGetter;
        public bool hasSetter;
        private string getterBody;
        private string setterBody;
        private bool multiStatementGetter;
        private bool multiStatementSetter;
        public object defaultValue;
        private bool hasDefault;
        private string assemblyQualifiedReturnType;
        private bool useAssemblyQualifiedReturnType;
        private bool assemblyQualifiedIsValueType;
        private bool assemblyQualifiedIsPrimitive;
        private bool hasBackingField;
        private bool hasIndexer;
        private string indexerBody;

        public List<AttributeGenerator> attributes = new List<AttributeGenerator>();
#pragma warning restore 0649

        private PropertyGenerator() { }

        public static PropertyGenerator Property(AccessModifier scope, PropertyModifier modifier, Type returnType, string name, bool hasDefault)
        {
            var prop = new PropertyGenerator();
            prop.scope = scope;
            prop.modifier = modifier;
            prop.name = name;
            prop.returnType = returnType;
            return prop;
        }

        public static PropertyGenerator Property(AccessModifier scope, PropertyModifier modifier, string assemblyQualifiedReturnType, string name, bool hasDefault, bool isPrimitive, bool isValueType)
        {
            var prop = new PropertyGenerator();
            prop.scope = scope;
            prop.modifier = modifier;
            prop.name = name;
            prop.assemblyQualifiedReturnType = assemblyQualifiedReturnType;
            prop.useAssemblyQualifiedReturnType = true;
            prop.assemblyQualifiedIsPrimitive = isPrimitive;
            prop.assemblyQualifiedIsValueType = isValueType;
            return prop;
        }

        public PropertyGenerator Default(object value)
        {
            defaultValue = value;
            return this;
        }

        public PropertyGenerator SingleStatementGetter(AccessModifier scope, string body)
        {
            this.getterScope = scope;
            multiStatementGetter = false;
            getterBody = body;
            hasGetter = true;
            return this;
        }


        public PropertyGenerator MultiStatementGetter(AccessModifier scope, string body)
        {
            this.getterScope = scope;
            multiStatementGetter = true;
            getterBody = body;
            hasGetter = true;
            return this;
        }

        public PropertyGenerator SingleStatementSetter(AccessModifier scope, string body)
        {
            this.setterScope = scope;
            multiStatementGetter = false;
            setterBody = body;
            hasSetter = true;
            return this;
        }


        public PropertyGenerator MultiStatementSetter(AccessModifier scope, string body)
        {
            this.setterScope = scope;
            multiStatementSetter = true;
            setterBody = body;
            hasSetter = true;
            return this;
        }

        public PropertyGenerator AddAttribute(AttributeGenerator attributeGenerator)
        {
            attributes.Add(attributeGenerator);
            return this;
        }

        public PropertyGenerator AddTypeIndexer(string contents)
        {
            hasIndexer = true;
            indexerBody = contents;
            return this;
        }

        public override string Generate(int indent)
        {
            if (useAssemblyQualifiedReturnType)
            {
                var _attributes = string.Empty;

                foreach (AttributeGenerator attr in attributes)
                {
                    _attributes += attr.Generate(indent) + "\n";
                }

                var modSpace = (modifier == PropertyModifier.None) ? string.Empty : " ";
                var definition = CodeBuilder.Indent(indent) + scope.AsString().ConstructHighlight() + " " + modifier.AsString().ConstructHighlight() + modSpace + assemblyQualifiedReturnType + (hasIndexer ? $"[{indexerBody}]" : string.Empty) + " " + name.LegalMemberName() + " " + GetterSetter();
                var output = defaultValue == null && returnType.IsValueType && returnType.IsPrimitive ? (hasGetter || hasSetter ? string.Empty : (!hasDefault ? ";" : string.Empty)) : hasDefault ? " = " + defaultValue.As().Code(true) + ";" : string.Empty;
                return _attributes + definition + output;
            }
            else
            {
                var _attributes = string.Empty;

                foreach (AttributeGenerator attr in attributes)
                {
                    _attributes += attr.Generate(indent) + "\n";
                }

                var modSpace = (modifier == PropertyModifier.None) ? string.Empty : " ";
                var definition = CodeBuilder.Indent(indent) + scope.AsString().ConstructHighlight() + " " + modifier.AsString().ConstructHighlight() + modSpace + returnType.As().CSharpName().TypeHighlight() + (hasIndexer ? $"[{indexerBody}]" : string.Empty) + " " + name + " " + GetterSetter();
                var output = defaultValue == null && assemblyQualifiedIsValueType && assemblyQualifiedIsPrimitive ? (hasGetter || hasSetter ? string.Empty : (!hasDefault ? ";" : string.Empty)) : hasDefault ? " = " + defaultValue.As().Code(true) + ";" : string.Empty;
                return _attributes + definition + output;
            }

            string GetterSetter()
            {
                var result = string.Empty;

                if (multiStatementGetter || multiStatementSetter)
                {
                    return (string.IsNullOrEmpty(getterBody) ? "\n" + CodeBuilder.Indent(indent) + "{" : "\n" + CodeBuilder.Indent(indent) + "{\n" + Getter()) + (string.IsNullOrEmpty(setterBody) ? "\n" : (multiStatementGetter ? "\n" : string.Empty) + "\n" + Setter() + "\n") +  CodeBuilder.Indent(indent) + "}";
                }
                else
                {
                    return CodeBuilder.Indent(indent) + "{ " + (string.IsNullOrEmpty(getterBody) ? string.Empty : Getter() + " ") + (string.IsNullOrEmpty(setterBody) ? string.Empty : (string.IsNullOrEmpty(getterBody) ? " " : string.Empty) + Setter() + " ") + "}";
                }
            }

            string Getter()
            {
                if (multiStatementGetter)
                {
                    return CodeBuilder.Indent(indent) + CodeBuilder.Indent(indent) + "get \n".ConstructHighlight() + CodeBuilder.Indent(indent + 1) + "{\n" + CodeBuilder.Indent(indent + 2) + getterBody.Replace("\n", "\n" + CodeBuilder.Indent(indent + 2)) + "\n" + CodeBuilder.Indent(indent + 1) + "}";
                }
                else
                {
                    return "get".ConstructHighlight() + " => " + getterBody.Replace("\n", "\n" + CodeBuilder.Indent(indent + 1)) + ";";
                }
            }

            string Setter()
            {
                if (multiStatementSetter)
                {
                    return CodeBuilder.Indent(indent) + CodeBuilder.Indent(indent) + "set \n".ConstructHighlight() + CodeBuilder.Indent(indent + 1) + "{\n" + CodeBuilder.Indent(indent + 2) + setterBody.Replace("\n", "\n" + CodeBuilder.Indent(indent + 2)) + "\n" + CodeBuilder.Indent(indent + 1) + "}";
                }
                else
                {
                    return "set".ConstructHighlight() + " => " + setterBody.Replace("\n", "\n" + CodeBuilder.Indent(indent + 1)) + ";";
                }
            }
        }

        public override List<string> Usings()
        {
            var usings = new List<string>();

            usings.Add(useAssemblyQualifiedReturnType ? (Type.GetType(assemblyQualifiedReturnType).IsPrimitive ? Type.GetType(assemblyQualifiedReturnType).Namespace : string.Empty) : (returnType.IsPrimitive ? string.Empty : returnType.Namespace));

            for (int i = 0; i < attributes.Count; i++)
            {
                usings.MergeUnique(attributes[i].Usings());
            }

            return usings;
        }
    }
}