using Unity.VisualScripting.Community.Libraries.Humility;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.CSharp
{
    /// <summary>
    /// A generator that retains data for creating a new class as a string.
    /// </summary>
    [RenamedFrom("Bolt.Addons.Community.Libraries.CSharp.ClassGenerator")]
    public sealed class ClassGenerator : TypeGenerator
    {
        public RootAccessModifier scope;
        public AccessModifier nestedScope;
        public ClassModifier modifier;
#pragma warning disable 0414
        public bool isNested;
#pragma warning restore 0414
        public string name;
        public List<AttributeGenerator> attributes = new List<AttributeGenerator>();
        public List<FieldGenerator> fields = new List<FieldGenerator>();
        public List<PropertyGenerator> properties = new List<PropertyGenerator>();
        public List<MethodGenerator> methods = new List<MethodGenerator>();
        public List<ConstructorGenerator> constructors = new List<ConstructorGenerator>();
        public List<ClassGenerator> classes = new List<ClassGenerator>();
        public List<StructGenerator> structs = new List<StructGenerator>();
        public List<EnumGenerator> enums = new List<EnumGenerator>();
        public List<InterfaceGenerator> subInterfaces = new List<InterfaceGenerator>();
        public List<Type> interfaces = new List<Type>();
        public Type inherits;
        public bool generateUsings;
        private bool useAssemblyQualifiedNameForInheritance;
        public string assemblyQualifiedInheritanceNamespace;
        public string assemblyQualifiedInheritanceType;

        private ClassGenerator() { }

        /// <summary>
        /// Create a root class generator based on required parameters.
        /// </summary>
        public static ClassGenerator Class(RootAccessModifier scope, ClassModifier modifier, string name, Type inherits)
        {
            var @class = new ClassGenerator();
            @class.scope = scope;
            @class.modifier = modifier;
            @class.name = name;
            @class.inherits = inherits == null ? typeof(object) : inherits;
            @class.isNested = false;
            return @class;
        }

        /// <summary>
        /// Create a root class generator based on required parameters.
        /// </summary>
        public static ClassGenerator Class(RootAccessModifier scope, ClassModifier modifier, string name, string inherits, string inheritsNamespace, List<string> usings = null)
        {
            var @class = new ClassGenerator();
            @class.scope = scope;
            @class.modifier = modifier;
            @class.name = name;
            @class.assemblyQualifiedInheritanceNamespace = inheritsNamespace;
            @class.assemblyQualifiedInheritanceType = inherits;
            @class.isNested = false;
            @class.usings = usings;
            @class.useAssemblyQualifiedNameForInheritance = true;
            return @class;
        }

        /// <summary>
        /// Create a nested class generator based on required parameters.
        /// </summary>
        public static ClassGenerator Class(AccessModifier nestedScope, ClassModifier modifier, string name, Type inherits)
        {
            var @class = new ClassGenerator();
            @class.nestedScope = nestedScope;
            @class.modifier = modifier;
            @class.name = name;
            @class.inherits = inherits == null ? typeof(object) : inherits;
            @class.isNested = true;
            return @class;
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

            var canShowInherits = !(inherits == null && string.IsNullOrEmpty(assemblyQualifiedInheritanceType) || inherits == typeof(object) && inherits.BaseType == null);
            output += CodeBuilder.Indent(indent) + scope.AsString().ConstructHighlight() + (modifier == ClassModifier.None ? string.Empty : " " + modifier.AsString().ConstructHighlight()) + " class ".ConstructHighlight() + name.LegalMemberName().TypeHighlight();
            output += !canShowInherits && interfaces.Count == 0 ? string.Empty : " : ";
            output += canShowInherits ? (inherits == null ? assemblyQualifiedInheritanceType : inherits.As().CSharpName()) + (interfaces.Count > 0 ? ", " : string.Empty) : string.Empty;

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
            return "\n";
        }

        public override List<string> Usings()
        {
            var usings = new List<string>();

            if (this.usings != null) usings.MergeUnique(this.usings);

            if (useAssemblyQualifiedNameForInheritance)
            {
                if (!string.IsNullOrEmpty(assemblyQualifiedInheritanceNamespace) && assemblyQualifiedInheritanceNamespace + "." + assemblyQualifiedInheritanceType != "System.Void") usings.Add(assemblyQualifiedInheritanceNamespace);
            }
            else
            {
                if (!inherits.Is().PrimitiveStringOrVoid()) usings.Add(inherits.Namespace);
            }

            var interfaceList = new List<string>();

            for (int i = 0; i < interfaces.Count; i++)
            {
                if (!string.IsNullOrEmpty(interfaces[i].Namespace) && !interfaceList.Contains(interfaces[i].Namespace)) interfaceList.Add(interfaces[i].Namespace);
            }

            usings.MergeUnique(interfaceList);

            for (int i = 0; i < attributes.Count; i++)
            {
                usings.MergeUnique(attributes[i].Usings());
            }

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

        

        /// <summary>
        /// Add an interface to this class.
        /// </summary>
        public ClassGenerator ImplementInterface(Type type)
        {
            interfaces.Add(type);
            return this;
        }

        /// <summary>
        /// Add an interface to this class.
        /// </summary>
        public ClassGenerator AddConstructor(ConstructorGenerator constructor)
        {
            constructors.Add(constructor);
            return this;
        }

        /// <summary>
        /// Add an attribute above this class.
        /// </summary>
        public ClassGenerator AddAttribute(AttributeGenerator generator)
        {
            attributes.Add(generator);
            return this;
        }

        /// <summary>
        /// Add a method to this class.
        /// </summary>
        public ClassGenerator AddMethod(MethodGenerator generator)
        {
            methods.Add(generator);
            return this;
        }

        /// <summary>
        /// Add a field to this class.
        /// </summary>
        public ClassGenerator AddField(FieldGenerator generator)
        {
            fields.Add(generator);
            return this;
        }

        /// <summary>
        /// Add a property to this class.
        /// </summary>
        /// <param name="generator"></param>
        /// <returns></returns>
        public ClassGenerator AddProperty(PropertyGenerator generator)
        {
            properties.Add(generator);
            return this;
        }

        /// <summary>
        /// Adds a nested class to this class.
        /// </summary>
        /// <param name="generator"></param>
        /// <returns></returns>
        public ClassGenerator AddClass(ClassGenerator generator)
        {
            classes.Add(generator);
            return this;
        }

        /// <summary>
        /// Add a nested struct to this class.
        /// </summary>
        public ClassGenerator AddStruct(StructGenerator generator)
        {
            structs.Add(generator);
            return this;
        }

        /// <summary>
        /// Add a nested enum to this class.
        /// </summary>
        public ClassGenerator AddEnum(EnumGenerator generator)
        {
            enums.Add(generator);
            return this;
        }

        /// <summary>
        /// Add an interface to this class.
        /// </summary>
        public ClassGenerator AddInterface(InterfaceGenerator generator)
        {
            subInterfaces.Add(generator);
            return this;
        }
    }
}
