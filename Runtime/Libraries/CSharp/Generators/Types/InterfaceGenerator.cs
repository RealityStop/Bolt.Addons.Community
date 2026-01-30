using Unity.VisualScripting.Community.Libraries.Humility;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Community.CSharp;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    [RenamedFrom("Bolt.Addons.Community.Libraries.CSharp.InterfaceGenerator")]
    public sealed class InterfaceGenerator : TypeGenerator
    {
#pragma warning disable 0649
        public RootAccessModifier scope;
        public Type[] interfaces;
#pragma warning restore 0649
        public string typeName;
        public List<AttributeGenerator> attributes = new List<AttributeGenerator>();
        public List<InterfacePropertyGenerator> properties = new List<InterfacePropertyGenerator>();
        public List<InterfaceMethodGenerator> methods = new List<InterfaceMethodGenerator>();
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
            }

            for (int i = 0; i < attributes.Count; i++)
            {
                attributes[i].Generate(writer, data);
                writer.NewLine();
            }

            var hasInterfaces = interfaces.Length > 0;

            writer.WriteIndented((scope.AsString() + " interface ").ConstructHighlight() + typeName.LegalMemberName().InterfaceHighlight());

            if (hasInterfaces)
            {
                writer.Write(" : ");

                for (int i = 0; i < interfaces.Length; i++)
                {
                    writer.Write(interfaces[i].Name.LegalMemberName().InterfaceHighlight());

                    if (i < interfaces.Length - 1)
                    {
                        writer.Write(", ");
                    }
                }
            }
            writer.NewLine();
        }

        protected override void GenerateBody(CodeWriter writer, ControlGenerationData data)
        {
            for (int i = 0; i < properties.Count; i++)
            {
                properties[i].Generate(writer, data);

                if (i < properties.Count - 1)
                {
                    writer.NewLine();
                }
            }

            if (methods.Count > 0 && properties.Count > 0)
            {
                writer.NewLine();
            }

            for (int i = 0; i < methods.Count; i++)
            {
                methods[i].Generate(writer, data);

                if (i < methods.Count - 1)
                {
                    writer.NewLine();
                }
            }
        }


        protected override void GenerateAfter(CodeWriter writer, ControlGenerationData data)
        {
        }

        private InterfaceGenerator(string name, params Type[] interfaces) { this.typeName = name; this.interfaces = interfaces; }

        public static InterfaceGenerator Interface(string name, params Type[] interfaces)
        {
            return new InterfaceGenerator(name, interfaces);
        }

        public InterfaceGenerator AddMethod(InterfaceMethodGenerator generator)
        {
            methods.Add(generator);
            return this;
        }

        public InterfaceGenerator AddProperty(InterfacePropertyGenerator generator)
        {
            properties.Add(generator);
            return this;
        }

        public InterfaceGenerator AddAttribute(AttributeGenerator generator)
        {
            attributes.Add(generator);
            return this;
        }

        public override List<string> Usings()
        {
            var usings = new List<string>();

            for (int i = 0; i < attributes?.Count; i++)
            {
                usings.MergeUnique(attributes[i].Usings());
            }

            for (int i = 0; i < interfaces?.Length; i++)
            {
                if (!usings.Contains(interfaces[i].Namespace) && !interfaces[i].IsPrimitive) usings.Add(interfaces[i].Namespace);
            }

            for (int i = 0; i < properties?.Count; i++)
            {
                usings.AddRange(properties[i].Usings());
            }

            for (int i = 0; i < methods?.Count; i++)
            {
                usings.AddRange(methods[i].Usings());
            }

            return usings;
        }
    }
}