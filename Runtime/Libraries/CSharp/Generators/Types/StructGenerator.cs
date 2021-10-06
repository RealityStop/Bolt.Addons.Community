using Unity.VisualScripting.Community.Libraries.Humility;
using System;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    [RenamedFrom("Bolt.Addons.Community.Libraries.CSharp.StructGenerator")]
    public sealed class StructGenerator : TypeGenerator
    {
        private RootAccessModifier scope;
#pragma warning disable 0414
        private bool isNested;
#pragma warning restore 0414
        private AccessModifier nestedScope;
        private StructModifier modifier;
        private string name;
        private List<AttributeGenerator> attributes = new List<AttributeGenerator>();
        private List<FieldGenerator> fields = new List<FieldGenerator>();
        private List<PropertyGenerator> properties = new List<PropertyGenerator>();
        private List<ConstructorGenerator> constructors = new List<ConstructorGenerator>();
        private List<MethodGenerator> methods = new List<MethodGenerator>();
        private List<ClassGenerator> classes = new List<ClassGenerator>();
        private List<StructGenerator> structs = new List<StructGenerator>();
        private List<EnumGenerator> enums = new List<EnumGenerator>();
        private List<InterfaceGenerator> subInterfaces = new List<InterfaceGenerator>();
        private List<Type> interfaces = new List<Type>();
        public bool generateUsings;

        private StructGenerator() { }

        public static StructGenerator Struct(RootAccessModifier scope, StructModifier modifier, string name)
        {
            var @struct = new StructGenerator();
            @struct.scope = scope;
            @struct.modifier = modifier;
            @struct.name = name.LegalMemberName();
            @struct.isNested = false;
            return @struct;
        }

        public static StructGenerator Struct(AccessModifier nestedScope, StructModifier modifier, string name)
        {
            var @struct = new StructGenerator();
            @struct.nestedScope = nestedScope;
            @struct.modifier = modifier;
            @struct.name = name.LegalMemberName();
            @struct.isNested = true;
            return @struct;
        }

        protected override string GenerateBefore(int indent)
        {
            var output = string.Empty;

            if (generateUsings)
            {
                var usings = Usings();
                var hasUsings = false;
                for (int i = 0; i < usings.Count; i++)
                {
                    if (!string.IsNullOrEmpty(usings[i]))
                    {
                        output += "using".ConstructHighlight() + " " + usings[i] + ";" + ((i < usings.Count - 1) ? "\n" : string.Empty);
                        hasUsings = true;
                    }
                }
                if (hasUsings) output += "\n\n";
            }

            for (int i = 0; i < attributes.Count; i++)
            {
                output += attributes[i].Generate(indent) + "\n";
            }

            output += CodeBuilder.Indent(indent) + scope.AsString().ConstructHighlight() + modifier.AsString().ConstructHighlight() + " struct ".ConstructHighlight() + name.LegalMemberName().TypeHighlight();
            output += interfaces.Count == 0 ? string.Empty : " : ";

            for (int i = 0; i < interfaces.Count; i++)
            {
                output += interfaces[i].As().CSharpName();
                output += i < interfaces.Count - 1 ? ", " : string.Empty;
            }

            return output;
        }

        protected override string GenerateBody(int indent)
        {
            var output = string.Empty;

            for (int i = 0; i < fields.Count; i++)
            {
                if (!string.IsNullOrEmpty(fields[i].name)) output += fields[i].Generate(indent) + (i < fields.Count - 1 ? "\n" : string.Empty);
            }

            output += fields.Count > 0 && (properties.Count > 0 || constructors.Count > 0 || methods.Count > 0 || classes.Count > 0 || structs.Count > 0 || enums.Count > 0) ? "\n\n" : string.Empty;

            for (int i = 0; i < properties.Count; i++)
            {
                if (!string.IsNullOrEmpty(properties[i].name)) output += properties[i].Generate(indent) + (i < properties.Count - 1 ? "\n\n" : string.Empty);
            }

            output += properties.Count > 0 && (constructors.Count > 0 || methods.Count > 0 || classes.Count > 0 || structs.Count > 0 || enums.Count > 0) ? "\n\n" : string.Empty;

            for (int i = 0; i < constructors.Count; i++)
            {
                output += constructors[i].Generate(indent) + (i < constructors.Count - 1 ? "\n\n" : string.Empty);
            }

            output += constructors.Count > 0 && (methods.Count > 0 || classes.Count > 0 || structs.Count > 0 || enums.Count > 0) ? "\n\n" : string.Empty;

            for (int i = 0; i < methods.Count; i++)
            {
                if (!string.IsNullOrEmpty(methods[i].name)) output += methods[i].Generate(indent) + (i < methods.Count - 1 ? "\n\n" : string.Empty);
            }

            output += methods.Count > 0 && (classes.Count > 0 || structs.Count > 0 || enums.Count > 0) ? "\n\n" : string.Empty;

            for (int i = 0; i < classes.Count; i++)
            {
                output += classes[i].Generate(indent);
                output += i < classes.Count - 1 ? "\n\n" : string.Empty;
            }

            output += classes.Count > 0 && (structs.Count > 0 || enums.Count > 0) ? "\n\n" : string.Empty;

            for (int i = 0; i < structs.Count; i++)
            {
                output += structs[i].Generate(indent);
                output += i < structs.Count - 1 ? "\n\n" : string.Empty;
            }

            output += structs.Count > 0 && enums.Count > 0 ? "\n\n" : string.Empty;

            for (int i = 0; i < enums.Count; i++)
            {
                output += enums[i].Generate(indent);
                output += i < enums.Count - 1 ? "\n\n" : string.Empty;
            }

            for (int i = 0; i < subInterfaces.Count; i++)
            {
                output += subInterfaces[i].Generate(indent);
                output += i < subInterfaces.Count - 1 ? "\n\n" : string.Empty;
            }

            return output;
        }

        protected override string GenerateAfter(int indent)
        {
            return string.Empty;
        }

        public StructGenerator ImplementInterface(Type type)
        {
            interfaces.Add(type);
            return this;
        }

        public StructGenerator AddAttribute(AttributeGenerator generator)
        {
            attributes.Add(generator);
            return this;
        }

        public StructGenerator AddMethod(MethodGenerator generator)
        {
            methods.Add(generator);
            return this;
        }

        public StructGenerator AddField(FieldGenerator generator)
        {
            fields.Add(generator);
            return this;
        }

        public StructGenerator AddProperty(PropertyGenerator generator)
        {
            properties.Add(generator);
            return this;
        }

        public StructGenerator AddConstructor(ConstructorGenerator generator)
        {
            constructors.Add(generator);
            return this;
        }

        public StructGenerator AddClass(ClassGenerator generator)
        {
            classes.Add(generator);
            return this;
        }

        public StructGenerator AddStruct(StructGenerator generator)
        {
            structs.Add(generator);
            return this;
        }

        public StructGenerator AddEnum(EnumGenerator generator)
        {
            enums.Add(generator);
            return this;
        }

        public StructGenerator AddInterface(InterfaceGenerator generator)
        {
            subInterfaces.Add(generator);
            return this;
        }

        public override List<string> Usings()
        {
            var usings = new List<string>();

            for (int i = 0; i < attributes.Count; i++)
            {
                usings.MergeUnique(attributes[i].Usings());
            }

            var interfaceList = new List<string>();

            for (int i = 0; i < interfaces.Count; i++)
            {
                if (!string.IsNullOrEmpty(interfaces[i].Namespace) && !interfaceList.Contains(interfaces[i].Namespace)) interfaceList.Add(interfaces[i].Namespace);
            }

            usings.MergeUnique(interfaceList);

            for (int i = 0; i < fields.Count; i++)
            {
                usings.MergeUnique(fields[i].Usings());
            }

            for (int i = 0; i < properties.Count; i++)
            {
                usings.MergeUnique(properties[i].Usings());
            }

            for (int i = 0; i < methods.Count; i++)
            {
                usings.MergeUnique(methods[i].Usings());
            }

            return usings;
        }

    }
}
