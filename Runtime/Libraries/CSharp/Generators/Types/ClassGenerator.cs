using Unity.VisualScripting.Community.Libraries.Humility;
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.CSharp;
using System.Linq;

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
        public (string message, Func<List<FieldGenerator>>) deferredFields;
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
        public string beforeUsings;
        public Action<CodeWriter> beforeUsingsAction;
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

        protected override void GenerateBefore(CodeWriter writer, ControlGenerationData data)
        {
            if (!string.IsNullOrEmpty(beforeUsings))
            {
                writer.WriteLine(beforeUsings);
            }

            if (beforeUsingsAction != null)
            {
                beforeUsingsAction?.Invoke(writer);
            }

            if (generateUsings)
            {
                var usings = Usings().ToHashSetPooled().ToListPooled();
                var hasUsings = false;
                for (int i = 0; i < usings.Count; i++)
                {
                    if (!string.IsNullOrEmpty(usings[i]))
                    {
                        writer.Write("using".ConstructHighlight() + " " + usings[i] + ";" + ((i < usings.Count - 1) ? "\n" : string.Empty));
                        hasUsings = true;
                    }
                }
                if (hasUsings) writer.Write("\n\n");
            }

            for (int i = 0; i < attributes.Count; i++)
            {
                attributes[i].Generate(writer, data);
                writer.NewLine();
            }

            var canShowInherits = !(inherits == null && string.IsNullOrEmpty(assemblyQualifiedInheritanceType) || inherits == typeof(object) && inherits.BaseType == null);
            writer.WriteIndented(scope.AsString().ConstructHighlight() + (modifier == ClassModifier.None ? string.Empty : " " + modifier.AsString().ConstructHighlight()) + " class ".ConstructHighlight() + name.LegalMemberName().TypeHighlight());
            if ((canShowInherits || interfaces.Count > 0) && SupportsInheritance())
            {
                writer.Write(" : ");
                writer.Write(inherits == null ? assemblyQualifiedInheritanceType : inherits != typeof(object) ? writer.GetTypeNameHighlighted(inherits) + (interfaces.Count > 0 ? ", " : string.Empty) : string.Empty);

                for (int i = 0; i < interfaces.Count; i++)
                {
                    writer.Write(interfaces[i]);
                    if (i < interfaces.Count - 1) writer.Write(", ");
                }
            }
            writer.NewLine();
        }

        public bool SupportsInheritance()
        {
            return modifier != ClassModifier.Static && modifier != ClassModifier.StaticPartial;
        }

        protected override void GenerateBody(CodeWriter writer, ControlGenerationData data)
        {
            for (int i = 0; i < fields.Count; i++)
            {
                if (!string.IsNullOrEmpty(fields[i].name))
                {
                    fields[i].Generate(writer, data);
                    if (i < fields.Count - 1) writer.Write("\n\n");
                }
            }

            if (fields.Count > 0 && (properties.Count > 0 || constructors.Count > 0 || methods.Count > 0 || classes.Count > 0 || structs.Count > 0 || enums.Count > 0))
                writer.Write("\n\n");
            else if (fields.Count > 0)
                writer.Write("\n");

            for (int i = 0; i < properties.Count; i++)
            {
                if (!string.IsNullOrEmpty(properties[i].name))
                {
                    properties[i].Generate(writer, data);
                    if (i < properties.Count - 1) writer.Write("\n\n");
                }
            }

            if (properties.Count > 0 && (constructors.Count > 0 || methods.Count > 0 || classes.Count > 0 || structs.Count > 0 || enums.Count > 0))
                writer.Write("\n\n");
            else if (properties.Count > 0)
                writer.Write("\n");

            for (int i = 0; i < constructors.Count; i++)
            {
                constructors[i].Generate(writer, data);
                if (i < constructors.Count - 1) writer.Write("\n\n");
            }

            if (constructors.Count > 0 && (methods.Count > 0 || classes.Count > 0 || structs.Count > 0 || enums.Count > 0))
                writer.Write("\n\n");
            else if (constructors.Count > 0)
                writer.Write("\n");

            for (int i = 0; i < methods.Count; i++)
            {
                if (!string.IsNullOrEmpty(methods[i].name))
                {
                    methods[i].Generate(writer, data);
                    if (i < methods.Count - 1) writer.Write("\n\n");
                }
            }

            var defferedFieldCount = 0;
            if (deferredFields.Item2 != null)
            {
                var message = deferredFields.message;
                var _fields = deferredFields.Item2();
                if (_fields.Count > 0)
                {
                    using (writer.CodeDiagnosticScope(message, CodeDiagnosticKind.Info))
                    {
                        if (!string.IsNullOrEmpty(message))
                        {
                            writer.Comment("(Hover for more info)", WriteOptions.IndentedNewLineAfter);
                        }
                        else
                        {
                            writer.NewLine();
                        }

                        defferedFieldCount = _fields.Count;
                        for (int i = 0; i < defferedFieldCount; i++)
                        {
                            if (!string.IsNullOrEmpty(_fields[i].name))
                            {
                                _fields[i].Generate(writer, data);
                                if (i < _fields.Count - 1) writer.Write("\n\n");
                            }
                        }
                    }
                }
            }

            if (methods.Count > 0 && (defferedFieldCount > 0 || classes.Count > 0 || structs.Count > 0 || enums.Count > 0))
                writer.Write("\n\n");
            else if (methods.Count > 0)
                writer.Write("\n");

            for (int i = 0; i < classes.Count; i++)
            {
                classes[i].Generate(writer, data);
                if (i < classes.Count - 1) writer.Write("\n\n");
            }

            if (classes.Count > 0 && (structs.Count > 0 || enums.Count > 0))
                writer.Write("\n\n");
            else if (classes.Count > 0)
                writer.Write("\n");

            for (int i = 0; i < structs.Count; i++)
            {
                structs[i].Generate(writer, data);
                if (i < structs.Count - 1) writer.Write("\n\n");
            }

            if (structs.Count > 0 && enums.Count > 0)
                writer.Write("\n\n");
            else if (structs.Count > 0)
                writer.Write("\n");

            for (int i = 0; i < enums.Count; i++)
            {
                enums[i].Generate(writer, data);
                if (i < enums.Count - 1) writer.Write("\n\n");
            }

            for (int i = 0; i < subInterfaces.Count; i++)
            {
                subInterfaces[i].Generate(writer, data);
                if (i < subInterfaces.Count - 1) writer.Write("\n\n");
            }
        }

        protected override void GenerateAfter(CodeWriter writer, ControlGenerationData data)
        {
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
                if (inherits != null && !inherits.Is().PrimitiveStringOrVoid()) usings.Add(inherits.Namespace);
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


        public ClassGenerator Inherit(Type type)
        {
            inherits = type;
            return this;
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
        /// Add a field to this class that will generate after the methods.
        /// </summary>
        public ClassGenerator DeferredFields(string message, Func<List<FieldGenerator>> fieldGeneratorsFactory)
        {
            deferredFields = (message, fieldGeneratorsFactory);
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
