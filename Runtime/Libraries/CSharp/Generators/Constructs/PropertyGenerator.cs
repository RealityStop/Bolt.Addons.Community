using Unity.VisualScripting.Community.Libraries.Humility;
using System;
using System.Collections.Generic;
using UnityEngine;

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

        public static PropertyGenerator Property(AccessModifier scope, PropertyModifier modifier, Type returnType, string name, bool hasDefault, AccessModifier getterScope, AccessModifier setterScope)
        {
            var prop = new PropertyGenerator();
            prop.scope = scope;
            prop.modifier = modifier;
            prop.name = name;
            prop.returnType = returnType;
            prop.getterScope = getterScope;
            prop.setterScope = setterScope;
            return prop;
        }

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
            hasDefault = true;
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
            multiStatementSetter = false;
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
            var getterscope = getterScope != scope ? getterScope.AsString().ConstructHighlight() + " " : string.Empty;
            var setterscope = setterScope != scope ? setterScope.AsString().ConstructHighlight() + " " : string.Empty;

            var _attributes = string.Empty;
            foreach (AttributeGenerator attr in attributes)
            {
                _attributes += attr.Generate(indent) + "\n";
            }

            var modSpace = (modifier == PropertyModifier.None) ? string.Empty : " ";

            if (hasGetter && modifier == PropertyModifier.Override && (string.IsNullOrEmpty(getterBody) || string.IsNullOrWhiteSpace(getterBody)))
            {
                SingleStatementGetter(getterScope, defaultValue.As().Code(true, true));
            }

            if (hasSetter && modifier == PropertyModifier.Override && (string.IsNullOrEmpty(setterBody) || string.IsNullOrWhiteSpace(setterBody)))
            {
                SingleStatementSetter(setterScope, defaultValue.As().Code(true, true));
            }

            var definition = CodeBuilder.Indent(indent) + scope.AsString().ConstructHighlight() + " " + modifier.AsString().ConstructHighlight() + modSpace +
                             (useAssemblyQualifiedReturnType ? assemblyQualifiedReturnType : returnType.As().CSharpName().TypeHighlight()) +
                             (hasIndexer ? $"[{indexerBody}]" : string.Empty) + " " + name.LegalMemberName() + " " + GetterSetter();

            var output = defaultValue != null && hasDefault && GetAndSetDoesNotHaveBody() && modifier != PropertyModifier.Override ? " = " + defaultValue.As().Code(true, true) + ";" : string.Empty;

            if (hasGetter && hasSetter)
            {
                if (getterScope != scope && setterScope != scope)
                {
                    output += "\n" + CodeBuilder.Indent(indent) + " /* Cannot specify accessibility modifiers for both getter and setter */";
                }
            }

            if (hasGetter && !hasSetter && getterScope != scope)
            {
                output += "\n" + CodeBuilder.Indent(indent) + " /* Accessibility modifier on getter may only be used if the property has both a get and a set */";
            }

            if (!hasGetter && hasSetter && setterScope != scope)
            {
                output += "\n" + CodeBuilder.Indent(indent) + " /* Accessibility modifier on setter may only be used if the property has both a get and a set */";
            }

            if (hasGetter)
            {
                if (modifier == PropertyModifier.Abstract && getterScope == AccessModifier.Private)
                {
                    output += "\n" + CodeBuilder.Indent(indent) + " /* Abstract Properties cannot have a private getter */";
                }
            }

            if (hasSetter)
            {
                if (modifier == PropertyModifier.Abstract && setterScope == AccessModifier.Private)
                {
                    output += "\n" + CodeBuilder.Indent(indent) + " /* Abstract Properties cannot have a private setter */";
                }
            }

            return _attributes + definition + output;

            string GetterSetter()
            {
                var result = string.Empty;

                if (GetAndSetDoesNotHaveBody())
                {
                    return $"{{ {(hasGetter ? getterscope + "get".ConstructHighlight() + ";" : string.Empty)} {(hasSetter ? setterscope + "set".ConstructHighlight() + ";" : string.Empty)} }}";
                }

                if (multiStatementGetter || multiStatementSetter)
                {
                    return (string.IsNullOrEmpty(getterBody) ? "\n" + CodeBuilder.Indent(indent) + "{" : "\n" + CodeBuilder.Indent(indent) + "{\n" + Getter()) +
                           (string.IsNullOrEmpty(setterBody) ? "\n" : (multiStatementGetter ? "\n" : string.Empty) + "\n" + Setter() + "\n") +
                           CodeBuilder.Indent(indent) + "}";
                }
                else
                {
                    return " { " + (string.IsNullOrEmpty(getterBody) ? string.Empty : Getter() + " ") +
                           (string.IsNullOrEmpty(setterBody) ? string.Empty : (string.IsNullOrEmpty(getterBody) ? " " : string.Empty) + Setter() + " ") + "}";
                }
            }

            string Getter()
            {
                if (GetAndSetDoesNotHaveBody())
                {
                    return $"{{ {(hasGetter ? getterscope + "get".ConstructHighlight() + ";" : string.Empty)} }} " + hasDefault;
                }

                if (multiStatementGetter)
                {
                    return CodeBuilder.Indent(indent + 1) + getterscope + "get \n".ConstructHighlight() + CodeBuilder.Indent(indent + 1) + "{\n" +
                           CodeBuilder.Indent(indent + 2) + getterBody.Replace("\n", "\n" + CodeBuilder.Indent(indent + 2)) + "\n" + CodeBuilder.Indent(indent + 1) + "}";
                }
                else
                {
                    var _indent = multiStatementSetter ? CodeBuilder.Indent(indent + 1) : string.Empty;
                    return _indent + getterscope + "get".ConstructHighlight() + " => " + getterBody.Replace("\n", "\n" + CodeBuilder.Indent(indent + 1)) + ";";
                }
            }

            string Setter()
            {
                if (GetAndSetDoesNotHaveBody())
                {
                    return $"{{ {(hasGetter ? getterscope + "set".ConstructHighlight() + ";" : string.Empty)} }}";
                }

                if (multiStatementSetter)
                {
                    return CodeBuilder.Indent(indent + 1) + setterscope + "set \n".ConstructHighlight() + CodeBuilder.Indent(indent + 1) + "{\n" +
                           CodeBuilder.Indent(indent + 2) + setterBody.Replace("\n", "\n" + CodeBuilder.Indent(indent + 2)) + "\n" + CodeBuilder.Indent(indent + 1) + "}";
                }
                else
                {
                    var _indent = multiStatementGetter ? CodeBuilder.Indent(indent + 1) : string.Empty;
                    return _indent + setterscope + "set".ConstructHighlight() + " => " + setterBody.Replace("\n", "\n" + CodeBuilder.Indent(indent + 1)) + ";";
                }
            }
        }

        private bool GetAndSetDoesNotHaveBody()
        {
            return (string.IsNullOrEmpty(getterBody) || string.IsNullOrWhiteSpace(getterBody)) && (string.IsNullOrEmpty(setterBody) || string.IsNullOrWhiteSpace(setterBody));
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
