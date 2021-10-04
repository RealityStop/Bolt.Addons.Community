using Unity.VisualScripting.Community.Libraries.Humility;
using System;
using System.Collections.Generic;
using static Unity.VisualScripting.Community.Libraries.Humility.HUMType_Children;

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
        public List<AttributeGenerator> attributes = new List<AttributeGenerator>();

        private FieldGenerator() { }

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

        public override string Generate(int indent)
        {
            if (string.IsNullOrEmpty(name)) { return string.Empty; }

            var _attributes = string.Empty;
            var count = 0;

            foreach (AttributeGenerator attr in attributes)
            {
                _attributes += attr.Generate(indent) + (count < attributes.Count - 1 ? "\n" : string.Empty);
                count++;
            }

            if (attributes.Count > 0) _attributes += "\n";
            var modSpace = (modifier == FieldModifier.None) ? string.Empty : " ";
            var definition = CodeBuilder.Indent(indent) + scope.AsString().ConstructHighlight() + " " + modifier.AsString().ConstructHighlight() + modSpace + (typeIsString ? stringType.WithHighlight(highlightType) : type.As().CSharpName()) + " " + name.LegalMemberName();
            var output = !isString && (defaultValue == null || defaultValue.Equals(HUMValue.Create().New(type))) ? ";" : " = " + (isString ? stringDefault : defaultValue.As().Code(true) + ";");
            return _attributes + definition + output;
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
                if (!type.Is().PrimitiveStringOrVoid() && !string.IsNullOrEmpty(type.Namespace)) usings.Add(type.Namespace);
            }

            for (int i = 0; i < attributes.Count; i++)
            {
                usings.MergeUnique(attributes[i].Usings());
            }

            return usings;
        }
    }
}