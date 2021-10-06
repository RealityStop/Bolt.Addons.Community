using Unity.VisualScripting.Community.Libraries.Humility;
using System;
using System.Collections.Generic;

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
            }

            for (int i = 0; i < attributes.Count; i++)
            {
                output += attributes[i].Generate(indent);
                if (i < attributes.Count - 1) output += "\n";
            }

            var hasInterfaces = interfaces.Length > 0;
            output += CodeBuilder.Indent(indent) + scope.AsString().ConstructHighlight() + " interface ".ConstructHighlight() + typeName.LegalMemberName().InterfaceHighlight() + (hasInterfaces ? " : " : string.Empty);
            
            for (int i = 0; i < interfaces.Length; i++)
            {
                output += interfaces[i].Name.LegalMemberName().InterfaceHighlight();
                if (i < interfaces.Length - 1) output += ", ";
            }

            return output;
        }

        protected override string GenerateBody(int indent)
        {
            var output = string.Empty;
            
            for (int i = 0; i < properties.Count; i++)
            {
                output += CodeBuilder.Indent(indent) + properties[i].Generate(indent);
                if (i < properties.Count - 1) output += "\n";
            }

            if (methods.Count > 0 && properties.Count > 0) output += "\n";

            for (int i = 0; i < methods.Count; i++)
            {
                output += CodeBuilder.Indent(indent) + methods[i].Generate(indent);
                if (i < methods.Count - 1) output += "\n";
            }

            return output;
        }

        protected override string GenerateAfter(int indent)
        {
            return string.Empty;
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
                var @namespace = properties[i].type.Namespace;
                if (!usings.Contains(@namespace) && !properties[i].type.Is().PrimitiveStringOrVoid()) usings.Add(@namespace);
            }

            for (int i = 0; i < methods?.Count; i++)
            {
                var @namespace = methods[i].returnType.Namespace;
                if (!usings.Contains(@namespace) && !methods[i].returnType.Is().PrimitiveStringOrVoid()) usings.Add(@namespace);
            }

            return usings;
        }
    }
}