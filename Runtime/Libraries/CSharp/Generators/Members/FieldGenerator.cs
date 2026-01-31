using Unity.VisualScripting.Community.Libraries.Humility;
using System;
using System.Collections.Generic;
using static Unity.VisualScripting.Community.Libraries.Humility.HUMType_Children;
using Unity.VisualScripting.Community.CSharp;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    [RenamedFrom("Bolt.Addons.Community.Libraries.CSharp.FieldGenerator")]
    public sealed class FieldGenerator : ConstructGenerator
    {
        public AccessModifier scope;
        public FieldModifier modifier;
        public string name;
        public object defaultValue;
        public string stringType;
        public string stringTypeNamespace;
        public bool typeIsString;
        public bool isString;
        public string stringDefault;
        public HighlightType highlightType;
        public Type type;
        public bool hasDefault;
        public List<AttributeGenerator> attributes = new List<AttributeGenerator>();
        private bool isLiteral = true;
        private bool isNewlineLiteral = true;
        private bool isNew = true;

        private FieldGenerator() { }

        public static FieldGenerator Field(AccessModifier scope, FieldModifier modifier, Type type, string name, object defaultValue, bool hasDefault = false)
        {
            var field = new FieldGenerator();
            field.scope = scope;
            field.modifier = modifier;
            field.type = type;
            field.name = name.LegalMemberName();
            field.defaultValue = defaultValue;
            field.hasDefault = hasDefault;
            return field;
        }

        public static FieldGenerator Field(AccessModifier scope, FieldModifier modifier, Type type, string name)
        {
            var field = new FieldGenerator();
            field.scope = scope;
            field.modifier = modifier;
            field.type = type;
            field.name = name.LegalMemberName();
            field.defaultValue = type.Default();
            return field;
        }

        public static FieldGenerator Field(AccessModifier scope, FieldModifier modifier, string typeName, string typeNamespace, string name, string defaultValue = null, HighlightType highlightType = HighlightType.None)
        {
            var field = new FieldGenerator();
            field.scope = scope;
            field.modifier = modifier;
            field.typeIsString = true;
            field.stringTypeNamespace = typeNamespace.SlashesToPeriods();
            field.stringType = typeName;
            field.name = name.LegalMemberName();
            field.highlightType = highlightType;
            field.defaultValue = defaultValue;
            return field;
        }

        public FieldGenerator Default(object value)
        {
            defaultValue = value;
            isString = false;
            return this;
        }

        public FieldGenerator CustomDefault(string value)
        {
            isString = true;
            stringDefault = value;
            return this;
        }

        public FieldGenerator AddAttribute(AttributeGenerator attributeGenerator)
        {
            attributes.Add(attributeGenerator);
            return this;
        }

        IDisposable nodeScope = null;

        public override void Generate(CodeWriter writer, ControlGenerationData data)
        {
            if (string.IsNullOrEmpty(name)) return;

            if (owner != null)
                nodeScope = writer.BeginNode(owner);

            var _attributes = string.Empty;
            var count = 0;

            foreach (AttributeGenerator attr in attributes)
            {
                attr.Generate(writer, data);
                if (count < attributes.Count - 1) writer.NewLine();
                count++;
            }

            if (attributes.Count > 0) writer.NewLine();

            var modSpace = (modifier == FieldModifier.None) ? string.Empty : " ";

            writer.WriteIndented();

            if (scope != AccessModifier.None)
            {
                writer.Write(scope.AsString().ConstructHighlight() + " ");
            }

            writer.Write(modifier.AsString().ConstructHighlight() + modSpace);

            if (typeIsString)
            {
                writer.Write(stringType.WithHighlight(highlightType));
            }
            else
            {
                writer.Write(type);
            }

            var legalName = name.LegalVariableName();
            writer.Write(" " + data.AddLocalNameInScope(legalName).VariableHighlight());

            if (!isString && (!hasDefault || defaultValue == null || (!(typeIsString ? stringType == nameof(Type) : type == typeof(Type)) && defaultValue.Equals(type.PseudoDefault()))))
            {
                writer.Write(";");
            }
            else
            {
                writer.Write(" = ");
                if (isString)
                {
                    writer.Write(stringDefault + ";");
                }
                else
                {
                    writer.Write(defaultValue.As().Code(isNew, isLiteral, true, "", isNewlineLiteral, true, false) + ";");
                }
            }
            nodeScope?.Dispose();
        }

        public FieldGenerator SetLiteral(bool value)
        {
            isLiteral = value;
            return this;
        }
        public FieldGenerator SetNewlineLiteral(bool value)
        {
            isNewlineLiteral = value;
            return this;
        }
        public FieldGenerator SetNew(bool value)
        {
            isNew = value;
            return this;
        }

        public override List<string> Usings()
        {
            var usings = new List<string>();

            if (typeIsString)
            {
                if (!string.IsNullOrEmpty(stringTypeNamespace)) usings.Add(stringTypeNamespace);
            }
            else
            {
                if (type != null && !type.Is().PrimitiveStringOrVoid()) usings.AddRange(type.GetAllNamespaces());
            }

            for (int i = 0; i < attributes.Count; i++)
            {
                usings.MergeUnique(attributes[i].Usings());
            }

            return usings;
        }
    }
}