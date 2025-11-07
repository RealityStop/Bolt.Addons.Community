using Unity.VisualScripting.Community.Libraries.Humility;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
        private string warning;

        public List<AttributeGenerator> attributes = new List<AttributeGenerator>();
#pragma warning restore 0649

        private PropertyGenerator() { }

        public static PropertyGenerator Property(AccessModifier scope, PropertyModifier modifier, Type returnType, string name, bool hasDefault, object defaultValue, AccessModifier getterScope, AccessModifier setterScope)
        {
            var prop = new PropertyGenerator();
            prop.scope = scope;
            prop.modifier = modifier;
            prop.name = name;
            prop.returnType = returnType;
            prop.getterScope = getterScope;
            prop.setterScope = setterScope;
            prop.hasDefault = hasDefault;
            prop.defaultValue = defaultValue;
            return prop;
        }

        public static PropertyGenerator Property(AccessModifier scope, PropertyModifier modifier, Type returnType, string name, bool hasDefault)
        {
            var prop = new PropertyGenerator();
            prop.scope = scope;
            prop.modifier = modifier;
            prop.name = name;
            prop.returnType = returnType;
            prop.hasDefault = hasDefault;
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
            prop.hasDefault = hasDefault;
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
            var getterscope = getterScope != scope && getterScope != AccessModifier.None ? getterScope.AsString().ConstructHighlight() + " " : string.Empty;
            var setterscope = setterScope != scope && setterScope != AccessModifier.None ? setterScope.AsString().ConstructHighlight() + " " : string.Empty;
            var modifierStr = modifier.AsString().ConstructHighlight() + " ";
            var scopeStr = scope != AccessModifier.None ? scope.AsString().ConstructHighlight() + " " : string.Empty;
            var typeStr = useAssemblyQualifiedReturnType ? assemblyQualifiedReturnType : returnType.As().CSharpName();

            var attributesStr = string.Empty;
            foreach (AttributeGenerator attr in attributes)
            {
                attributesStr += attr.Generate(indent) + "\n";
            }

            // Handle auto-implemented override properties
            if (hasGetter && modifier == PropertyModifier.Override && string.IsNullOrWhiteSpace(getterBody))
            {
                SingleStatementGetter(getterScope, defaultValue.As().Code(true, true));
            }
            if (hasSetter && modifier == PropertyModifier.Override && string.IsNullOrWhiteSpace(setterBody))
            {
                SingleStatementSetter(setterScope, defaultValue.As().Code(true, true));
            }

            // Build property header
            var header = CodeBuilder.Indent(indent) + scopeStr + modifierStr + typeStr;
            if (hasIndexer)
            {
                header += " this".ConstructHighlight() + $"[{indexerBody}]";
            }
            else
            {
                header += " " + name.LegalMemberName().VariableHighlight();
            }

            // Property body
            var body = string.Empty;
            var warnings = new List<string>();

            if (!string.IsNullOrEmpty(warning)) warnings.Add(warning);

            // Validate accessibility modifiers
            if (hasGetter && hasSetter && getterScope != scope && setterScope != scope && getterScope != AccessModifier.None && setterScope != AccessModifier.None)
            {
                warnings.Add("Cannot specify accessibility modifiers for both getter and setter");
            }
            if (hasGetter && !hasSetter && getterScope != scope && getterScope != AccessModifier.None)
            {
                warnings.Add("Accessibility modifier on getter may only be used if the property has both a get and a set");
            }
            if (!hasGetter && hasSetter && setterScope != scope && setterScope != AccessModifier.None)
            {
                warnings.Add("Accessibility modifier on setter may only be used if the property has both a get and a set");
            }
            if (hasGetter && modifier == PropertyModifier.Abstract && getterScope == AccessModifier.Private)
            {
                warnings.Add("Abstract Properties cannot have a private getter");
            }
            if (hasSetter && modifier == PropertyModifier.Abstract && setterScope == AccessModifier.Private)
            {
                warnings.Add("Abstract Properties cannot have a private setter");
            }
            if (hasGetter && getterScope != scope && getterScope != AccessModifier.None && !CodeBuilder.IsMoreRestrictive(getterScope, scope))
            {
                warnings.Add("Accessibility modifier on getter must be more restrictive than the property itself");
            }
            if (hasSetter && setterScope != scope && setterScope != AccessModifier.None && !CodeBuilder.IsMoreRestrictive(setterScope, scope))
            {
                warnings.Add("Accessibility modifier on setter must be more restrictive than the property itself");
            }

            // Generate accessor body
            if (IsAutoImplemented())
            {
                body = " { ";
                if (hasGetter)
                {
                    body += getterscope + "get".ConstructHighlight() + "; ";
                }
                if (hasSetter)
                {
                    body += setterscope + "set".ConstructHighlight() + "; ";
                }
                body += "}";

                if (hasDefault && defaultValue != null && modifier != PropertyModifier.Override)
                {
                    body += " = " + defaultValue.As().Code(true, true, true, "", false, true, false) + ";";
                }
            }
            else
            {
                body = "\n" + CodeBuilder.Indent(indent) + "{";

                if (hasGetter)
                {
                    body += "\n" + CodeBuilder.Indent(indent + 1) + getterscope + "get".ConstructHighlight();
                    if (multiStatementGetter)
                    {
                        body += "\n" + CodeBuilder.Indent(indent + 1) + "{\n" +
                               CodeBuilder.Indent(indent + 2) + getterBody.Replace("\n", "\n" + CodeBuilder.Indent(indent + 2)) +
                               "\n" + CodeBuilder.Indent(indent + 1) + "}";
                    }
                    else if (!string.IsNullOrWhiteSpace(getterBody))
                    {
                        body += " => " + getterBody + ";";
                    }
                }

                if (hasSetter)
                {
                    if (hasGetter) body += "\n";
                    body += CodeBuilder.Indent(indent + 1) + setterscope + "set".ConstructHighlight();
                    if (multiStatementSetter)
                    {
                        body += "\n" + CodeBuilder.Indent(indent + 1) + "{\n" +
                               CodeBuilder.Indent(indent + 2) + setterBody.Replace("\n", "\n" + CodeBuilder.Indent(indent + 2)) +
                               "\n" + CodeBuilder.Indent(indent + 1) + "}";
                    }
                    else if (!string.IsNullOrWhiteSpace(setterBody))
                    {
                        body += " => " + setterBody + ";";
                    }
                }

                body += "\n" + CodeBuilder.Indent(indent) + "}";
            }

            // Add warnings
            var warningsStr = string.Empty;
            if (warnings.Count > 0)
            {
                warningsStr = string.Join("\n", warnings.Select(w =>
                    CodeBuilder.Indent(indent) + (" /* " + w + " */").WarningHighlight())) + "\n";
            }

            return warningsStr + attributesStr + header + body;
        }

        public bool IsAutoImplemented()
        {
            return (string.IsNullOrWhiteSpace(getterBody) && string.IsNullOrWhiteSpace(setterBody)) ||
                   (modifier == PropertyModifier.Abstract);
        }

        public PropertyGenerator SetWarning(string warning)
        {
            this.warning = warning;
            return this;
        }

        public override List<string> Usings()
        {
            var usings = new List<string>
            {
                useAssemblyQualifiedReturnType ? (Type.GetType(assemblyQualifiedReturnType).IsPrimitive ?  string.Empty : Type.GetType(assemblyQualifiedReturnType).Namespace) : (returnType.IsPrimitive ? string.Empty : returnType.Namespace)
            };

            for (int i = 0; i < attributes.Count; i++)
            {
                usings.MergeUnique(attributes[i].Usings());
            }

            return usings;
        }
    }
}
