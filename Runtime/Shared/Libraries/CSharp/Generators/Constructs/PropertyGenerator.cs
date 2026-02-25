using Unity.VisualScripting.Community.Libraries.Humility;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting.Community.CSharp;

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

        private Action<CodeWriter> getterBodyAction;
        private Action<CodeWriter> setterBodyAction;
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

        public PropertyGenerator SingleStatementGetter(AccessModifier scope, Action<CodeWriter> bodyAction)
        {
            this.getterScope = scope;
            multiStatementGetter = false;
            getterBodyAction = bodyAction;
            hasGetter = true;
            return this;
        }


        public PropertyGenerator MultiStatementGetter(AccessModifier scope, Action<CodeWriter> bodyAction)
        {
            this.getterScope = scope;
            multiStatementGetter = true;
            getterBodyAction = bodyAction;
            hasGetter = true;
            return this;
        }

        public PropertyGenerator SingleStatementSetter(AccessModifier scope, Action<CodeWriter> bodyAction)
        {
            this.setterScope = scope;
            multiStatementSetter = false;
            setterBodyAction = bodyAction;
            hasSetter = true;
            return this;
        }


        public PropertyGenerator MultiStatementSetter(AccessModifier scope, Action<CodeWriter> bodyAction)
        {
            this.setterScope = scope;
            multiStatementSetter = true;
            setterBodyAction = bodyAction;
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

        IDisposable getterNodeScope = null;
        IDisposable setterNodeScope = null;
        Unit getterOwner = null;
        Unit setterOwner = null;

        public override void Generate(CodeWriter writer, ControlGenerationData data)
        {
            var getterscope = getterScope != scope && getterScope != AccessModifier.None
                ? getterScope.AsString().ConstructHighlight() + " "
                : string.Empty;

            var setterscope = setterScope != scope && setterScope != AccessModifier.None
                ? setterScope.AsString().ConstructHighlight() + " "
                : string.Empty;

            var modifierStr = modifier != PropertyModifier.None ? modifier.AsString().ConstructHighlight() + " " : string.Empty;
            var scopeStr = scope != AccessModifier.None ? scope.AsString().ConstructHighlight() + " " : string.Empty;
            var typeStr = useAssemblyQualifiedReturnType ? assemblyQualifiedReturnType : writer.GetTypeNameHighlighted(returnType);

            foreach (AttributeGenerator attr in attributes)
            {
                attr.Generate(writer, data);
                writer.NewLine();
            }

            // Handle auto-implemented override properties
            if (hasGetter && modifier == PropertyModifier.Override && getterBodyAction == null)
            {
                SingleStatementGetter(getterScope, w => w.Object(defaultValue));
            }

            if (hasSetter && modifier == PropertyModifier.Override && setterBodyAction == null)
            {
                SingleStatementSetter(setterScope, w => w.Object(defaultValue));
            }

            var header = scopeStr + modifierStr + typeStr;

            if (hasIndexer)
            {
                header += " this".ConstructHighlight() + $"[{indexerBody}]";
            }
            else
            {
                var legalName = name.LegalVariableName();
                header += " " + legalName.VariableHighlight();
                data.AddLocalNameInScope(legalName, returnType);
            }

            var warnings = new List<string>();

            if (!string.IsNullOrEmpty(warning))
            {
                warnings.Add(warning);
            }

            // Validate accessibility modifiers
            if (hasGetter && hasSetter && getterScope != scope && setterScope != scope &&
                getterScope != AccessModifier.None && setterScope != AccessModifier.None)
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

            if (hasGetter && getterScope != scope && getterScope != AccessModifier.None &&
                !CodeBuilder.IsMoreRestrictive(getterScope, scope))
            {
                warnings.Add("Accessibility modifier on getter must be more restrictive than the property itself");
            }

            if (hasSetter && setterScope != scope && setterScope != AccessModifier.None &&
                !CodeBuilder.IsMoreRestrictive(setterScope, scope))
            {
                warnings.Add("Accessibility modifier on setter must be more restrictive than the property itself");
            }

            if (warnings.Count > 0)
            {
                for (int i = 0; i < warnings.Count; i++)
                {
                    writer.WriteIndented((" /* " + warnings[i] + " */").ErrorHighlight());
                    writer.NewLine();
                }
            }

            writer.WriteIndented(header);

            // Generate accessor body
            if (IsAutoImplemented())
            {
                writer.Write(" { ");

                if (hasGetter)
                {
                    if (getterOwner != null)
                        getterNodeScope = writer.BeginNode(getterOwner);

                    writer.Write(getterscope + "get".ConstructHighlight() + "; ");

                    getterNodeScope?.Dispose();
                }

                if (hasSetter)
                {
                    if (setterOwner != null)
                        setterNodeScope = writer.BeginNode(setterOwner);

                    writer.Write(setterscope + "set".ConstructHighlight() + "; ");

                    setterNodeScope?.Dispose();
                }

                writer.Write("}");

                if (hasDefault && defaultValue != null && modifier != PropertyModifier.Override)
                {
                    writer.Write(" = " + defaultValue.As().Code(true, true, true, "", false, true, false) + ";");
                }
            }
            else
            {
                writer.NewLine();
                writer.WriteLine("{");


                if (hasGetter)
                {
                    if (getterOwner != null)
                        getterNodeScope = writer.BeginNode(getterOwner);

                    using (writer.Indented())
                    {
                        writer.WriteIndented(getterscope + "get".ConstructHighlight());

                        if (multiStatementGetter)
                        {
                            writer.NewLine();

                            writer.WriteLine("{");
                            if (getterBodyAction != null)
                            {
                                using (writer.Indented())
                                {
                                    getterBodyAction.Invoke(writer);
                                }
                            }
                            writer.WriteLine("}");
                        }
                        else if (getterBodyAction != null)
                        {
                            writer.Write(" => ");
                            getterBodyAction?.Invoke(writer);
                            writer.WriteEnd(EndWriteOptions.LineEnd);
                        }
                    }

                    getterNodeScope?.Dispose();
                }


                if (hasSetter)
                {
                    if (setterOwner != null)
                        setterNodeScope = writer.BeginNode(setterOwner);

                    using (writer.Indented())
                    {
                        writer.WriteIndented(getterscope + "set".ConstructHighlight());

                        if (multiStatementSetter)
                        {
                            writer.NewLine();

                            writer.WriteLine("{");
                            if (getterBodyAction != null)
                            {
                                using (writer.Indented())
                                {
                                    setterBodyAction.Invoke(writer);
                                }
                            }
                            writer.WriteLine("}");
                        }
                        else if (setterBodyAction != null)
                        {
                            writer.Write(" => ");
                            setterBodyAction?.Invoke(writer);
                            writer.WriteEnd(EndWriteOptions.LineEnd);
                        }
                    }
                    setterNodeScope?.Dispose();
                }

                writer.WriteIndented("}");
            }
        }

        public void SetGetterOwner(Unit getterOwner)
        {
            this.getterOwner = getterOwner;
        }

        public void SetSetterOwner(Unit setterOwner)
        {
            this.setterOwner = setterOwner;
        }

        public bool IsAutoImplemented()
        {
            return (getterBodyAction == null && setterBodyAction == null) || (modifier == PropertyModifier.Abstract);
        }

        public PropertyGenerator SetWarning(string warning)
        {
            this.warning = warning;
            return this;
        }

        public override List<string> Usings()
        {
            HashSet<string> result = new HashSet<string>();

            Type resolvedType = useAssemblyQualifiedReturnType ? Type.GetType(assemblyQualifiedReturnType) : returnType;

            if (resolvedType != null && !resolvedType.IsPrimitive)
            {
                foreach (string ns in resolvedType.GetAllNamespaces())
                {
                    if (!string.IsNullOrEmpty(ns))
                        result.Add(ns);
                }
            }

            for (int i = 0; i < attributes.Count; i++)
            {
                List<string> attrUsings = attributes[i].Usings();
                for (int u = 0; u < attrUsings.Count; u++)
                {
                    if (!string.IsNullOrEmpty(attrUsings[u]))
                        result.Add(attrUsings[u]);
                }
            }

            return result.ToList();
        }
    }
}
