using Unity.VisualScripting.Community.Libraries.Humility;
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting.Community.CSharp;

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

        protected override void GenerateBefore(CodeWriter writer, ControlGenerationData data)
        {
            if (generateUsings)
            {
                var usings = Usings();
                for (int i = 0; i < usings.Count; i++)
                {
                    writer.Write("using".ConstructHighlight() + " " + usings[i] + ";");

                    if (i < usings.Count - 1)
                    {
                        writer.NewLine();
                    }
                }

                writer.NewLine();
                writer.NewLine();
            }

            for (int i = 0; i < attributes.Count; i++)
            {
                attributes[i].Generate(writer, data);
                writer.NewLine();
            }

            writer.WriteLine(scope.AsString().ConstructHighlight() + " enum ".ConstructHighlight() + typeName.LegalMemberName().EnumHighlight());
        }

        protected override void GenerateBody(CodeWriter writer, ControlGenerationData data)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (string.IsNullOrEmpty(items[i].name))
                    continue;

                writer.WriteIndented(items[i].name.LegalMemberName().VariableHighlight());

                if (indexing)
                {
                    writer.Write(" = ");
                    writer.Write(items[i].index.ToString().NumericHighlight());
                }

                if (i < items.Count - 1)
                {
                    writer.Write(",");
                }
                writer.NewLine();
            }
        }
        
        protected override void GenerateAfter(CodeWriter writer, ControlGenerationData data)
        {
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