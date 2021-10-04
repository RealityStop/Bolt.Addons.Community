using Unity.VisualScripting.Community.Libraries.Humility;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    [RenamedFrom("Bolt.Addons.Community.Libraries.CSharp.EnumGenerator")]
    public sealed class EnumGenerator : TypeGenerator
    {
#pragma warning disable 0649
        private RootAccessModifier scope;
#pragma warning restore 0649
        private string typeName;
        private List<AttributeGenerator> attributes = new List<AttributeGenerator>();
        private List<EnumValueGenerator> items = new List<EnumValueGenerator>();
        public bool indexing;
        public bool generateUsings;

        protected override string GenerateBefore(int indent)
        {
            var output = string.Empty;

            if (generateUsings)
            {
                var usings = Usings();
                for (int i = 0; i < usings.Count; i++)
                {
                    output += "using".ConstructHighlight() + " " + usings[i] + ";" + ((i < usings.Count - 1) ? "\n" : string.Empty);
                }
                output += "\n\n";
            }

            for (int i = 0; i < attributes.Count; i++)
            {
                output += attributes[i].Generate(indent) + "\n";
            }

            output += CodeBuilder.Indent(indent) + scope.AsString().ConstructHighlight() + " enum ".ConstructHighlight() + typeName.LegalMemberName().EnumHighlight();

            return output;
        }

        protected override string GenerateBody(int indent)
        {
            var output = string.Empty;

            for (int i = 0; i < items.Count; i++)
            {
                if (string.IsNullOrEmpty(items[i].name)) { continue; }
                output += CodeBuilder.Indent(indent) + items[i].name.LegalMemberName() + (indexing ? " = " + items[i].index.ToString().NumericHighlight() : string.Empty);
                if (i < items.Count - 1)
                {
                    output += ",";
                    output += "\n";
                }
            }

            return output;
        }

        protected override string GenerateAfter(int indent)
        {
            return string.Empty;
        }

        private EnumGenerator(string name) { this.typeName = name; }

        public static EnumGenerator Enum(string name)
        {
            return new EnumGenerator(name);
        }

        public override List<string> Usings()
        {
            var usings = new List<string>();

            for (int i = 0; i < attributes.Count; i++)
            {
                usings.MergeUnique(attributes[i].Usings());
            }

            return usings;
        }

        public EnumGenerator AddItem(string itemName, int index)
        {
            var enumValue = new EnumValueGenerator();
            enumValue.name = itemName;
            enumValue.index = index;
            items.Add(enumValue);
            return this;
        }

        public EnumGenerator AddAttribute(AttributeGenerator generator)
        {
            attributes.Add(generator);
            return this;
        }
    }
}