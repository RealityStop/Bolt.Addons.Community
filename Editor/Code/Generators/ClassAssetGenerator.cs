using Unity.VisualScripting.Community.Libraries.CSharp;
using System;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    [CodeGenerator(typeof(ClassAsset))]
    public sealed class ClassAssetGenerator : MemberTypeAssetGenerator<ClassAsset, ClassFieldDeclaration, ClassMethodDeclaration, ClassConstructorDeclaration>
    {
        protected override TypeGenerator OnGenerateType(ref string output, NamespaceGenerator @namespace)
        {
            var @class = ClassGenerator.Class(RootAccessModifier.Public, ClassModifier.None, Data.title.LegalMemberName(), Data.scriptableObject ? typeof(ScriptableObject) : typeof(object));
            if (Data.definedEvent) @class.ImplementInterface(typeof(IDefinedEvent));
            if (Data.inspectable) @class.AddAttribute(AttributeGenerator.Attribute<InspectableAttribute>());
            if (Data.serialized) @class.AddAttribute(AttributeGenerator.Attribute<SerializableAttribute>());
            if (Data.includeInSettings) @class.AddAttribute(AttributeGenerator.Attribute<IncludeInSettingsAttribute>().AddParameter(true));
            if (Data.scriptableObject) @class.AddAttribute(AttributeGenerator.Attribute<CreateAssetMenuAttribute>().AddParameter("menuName", Data.menuName).AddParameter("fileName", Data.fileName).AddParameter("order", Data.order));

            for (int i = 0; i < Data.constructors.Count; i++)
            {
                var constructor = ConstructorGenerator.Constructor(Data.constructors[i].scope, Data.constructors[i].modifier, Data.title.LegalMemberName());
                if (Data.constructors[i].graph.units.Count > 0)
                {
                    var unit = Data.constructors[i].graph.units[0] as FunctionNode;
                    constructor.Body(FunctionNodeGenerator.GetSingleDecorator(unit, unit).GenerateControl(null, new ControlGenerationData(), 0));

                    for (int pIndex = 0; pIndex < Data.constructors[i].parameters.Count; pIndex++)
                    {
                        if (!string.IsNullOrEmpty(Data.constructors[i].parameters[pIndex].name)) constructor.AddParameter(false, ParameterGenerator.Parameter(Data.constructors[i].parameters[pIndex].name, Data.constructors[i].parameters[pIndex].type, ParameterModifier.None));
                    }
                }

                @class.AddConstructor(constructor);
            }

            for (int i = 0; i < Data.variables.Count; i++)
            {
                if (!string.IsNullOrEmpty(Data.variables[i].name) && Data.variables[i].type != null)
                {
                    var attributes = Data.variables[i].attributes;

                    if (Data.variables[i].isProperty)
                    {
                        var property = PropertyGenerator.Property(Data.variables[i].scope, Data.variables[i].propertyModifier, Data.variables[i].type, Data.variables[i].name, false);

                        for (int attrIndex = 0; attrIndex < attributes.Count; attrIndex++)
                        {
                            AttributeGenerator attrGenerator = AttributeGenerator.Attribute(attributes[attrIndex].GetAttributeType());
                            property.AddAttribute(attrGenerator);
                        }

                        if (Data.variables[i].get)
                        {
                            property.MultiStatementGetter(AccessModifier.Public, NodeGenerator.GetSingleDecorator(Data.variables[i].getter.graph.units[0] as Unit, Data.variables[i].getter.graph.units[0] as Unit)
                            .GenerateControl(null, new ControlGenerationData() { returns = Data.variables[i].type }, 0));
                        }

                        if (Data.variables[i].set)
                        {
                            property.MultiStatementSetter(AccessModifier.Public, NodeGenerator.GetSingleDecorator(Data.variables[i].setter.graph.units[0] as Unit, Data.variables[i].setter.graph.units[0] as Unit)
                            .GenerateControl(null, new ControlGenerationData(), 0));
                        }

                        @class.AddProperty(property);
                    }
                    else
                    {
                        var field = FieldGenerator.Field(Data.variables[i].scope, Data.variables[i].fieldModifier, Data.variables[i].type, Data.variables[i].name);


                        for (int attrIndex = 0; attrIndex < attributes.Count; attrIndex++)
                        {
                            AttributeGenerator attrGenerator = AttributeGenerator.Attribute(attributes[attrIndex].GetAttributeType());
                            field.AddAttribute(attrGenerator);
                        }

                        @class.AddField(field);
                    }
                }
            }

            for (int i = 0; i < Data.methods.Count; i++)
            {
                if (!string.IsNullOrEmpty(Data.methods[i].name) && Data.methods[i].returnType != null)
                {
                    var method = MethodGenerator.Method(Data.methods[i].scope, Data.methods[i].modifier, Data.methods[i].returnType, Data.methods[i].name);
                    if (Data.methods[i].graph.units.Count > 0)
                    {
                        var unit = Data.methods[i].graph.units[0] as FunctionNode;
                        method.Body(FunctionNodeGenerator.GetSingleDecorator(unit, unit).GenerateControl(null, new ControlGenerationData(), 0));
                        
                        for (int pIndex = 0; pIndex < Data.methods[i].parameters.Count; pIndex++)
                        {
                            if (!string.IsNullOrEmpty(Data.methods[i].parameters[pIndex].name)) method.AddParameter(ParameterGenerator.Parameter(Data.methods[i].parameters[pIndex].name, Data.methods[i].parameters[pIndex].type, ParameterModifier.None));
                        }
                    }

                    @class.AddMethod(method);
                }
            }

            @namespace.AddClass(@class);

            return @class;
        }
    }
}
