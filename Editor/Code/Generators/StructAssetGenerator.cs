using Unity.VisualScripting.Community.Libraries.CSharp;
using System;
using Unity.VisualScripting;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    [CodeGenerator(typeof(StructAsset))]
    public sealed class StructAssetGenerator : MemberTypeAssetGenerator<StructAsset, StructFieldDeclaration, StructMethodDeclaration, StructConstructorDeclaration>
    {
        protected override TypeGenerator OnGenerateType(ref string output, NamespaceGenerator @namespace)
        {
            var @struct = StructGenerator.Struct(RootAccessModifier.Public, StructModifier.None, Data.title.LegalMemberName());
            if (Data.definedEvent) @struct.ImplementInterface(typeof(IDefinedEvent));
            if (Data.inspectable) @struct.AddAttribute(AttributeGenerator.Attribute<InspectableAttribute>());
            if (Data.serialized) @struct.AddAttribute(AttributeGenerator.Attribute<SerializableAttribute>());
            if (Data.includeInSettings) @struct.AddAttribute(AttributeGenerator.Attribute<IncludeInSettingsAttribute>().AddParameter(true));

            foreach (var attribute in Data.attributes)
            {
                var attrGenerator = AttributeGenerator.Attribute(attribute.GetAttributeType());
                foreach (var param in attribute.parameters)
                {
                    if (param.defaultValue is IList list)
                    {
                        if (param.isParamsParameter)
                        {
                            foreach (var item in list)
                            {
                                attrGenerator.AddParameter(item);
                            }
                        }
                        else
                        {
                            if (!attrGenerator.parameterValues.Contains(param))
                            {
                                attrGenerator.AddParameter(param.defaultValue);
                            }
                        }
                    }
                    else
                    {
                        if (!attrGenerator.parameterValues.Contains(param))
                        {
                            attrGenerator.AddParameter(param.defaultValue);
                        }
                    }
                }
                @struct.AddAttribute(attrGenerator);
            }

            for (int i = 0; i < Data.constructors.Count; i++)
            {
                var constructor = ConstructorGenerator.Constructor(Data.constructors[i].scope, Data.constructors[i].modifier, Data.constructors[i].CallType, Data.title.LegalMemberName());
                if (Data.constructors[i].graph.units.Count > 0)
                {
                    var usings = new List<string>();
                    foreach (var _unit in Data.constructors[i].graph.GetUnitsRecursive(Recursion.New(Recursion.defaultMaxDepth)).Cast<Unit>())
                    {
                        if (!string.IsNullOrEmpty(NodeGenerator.GetSingleDecorator(_unit, _unit).NameSpace))
                            usings.Add(NodeGenerator.GetSingleDecorator(_unit, _unit).NameSpace);
                    }

                    @struct.AddUsings(usings);
                    var unit = Data.constructors[i].graph.units[0] as FunctionNode;
                    var data = new ControlGenerationData();
                    data.NewScope();
                    for (int item = 0; item < Data.variables.Count; item++)
                    {
                        data.AddLocalNameInScope(Data.variables[item].name, Data.variables[item].type);
                    }
                    constructor.Body(unit.GenerateControl(null, data, 0));
                    data.ExitScope();
                    for (int pIndex = 0; pIndex < Data.constructors[i].parameters.Count; pIndex++)
                    {
                        if (!string.IsNullOrEmpty(Data.constructors[i].parameters[pIndex].name)) constructor.AddParameter(false, ParameterGenerator.Parameter(Data.constructors[i].parameters[pIndex].name, Data.constructors[i].parameters[pIndex].type, Libraries.CSharp.ParameterModifier.None));
                    }
                }

                @struct.AddConstructor(constructor);
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
                            foreach (var param in attributes[attrIndex].parameters)
                            {
                                if (param.defaultValue is IList list)
                                {
                                    if (param.isParamsParameter)
                                    {
                                        foreach (var item in list)
                                        {
                                            attrGenerator.AddParameter(item);
                                        }
                                    }
                                    else
                                    {
                                        attrGenerator.AddParameter(param.defaultValue);
                                    }
                                }
                                else
                                {
                                    if (!attrGenerator.parameterValues.Contains(param))
                                    {
                                        attrGenerator.AddParameter(param.defaultValue);
                                    }
                                }
                            }
                            property.AddAttribute(attrGenerator);
                        }

                        if (Data.variables[i].get)
                        {
                            var usings = new List<string>();
                            foreach (var _unit in Data.variables[i].getter.graph.GetUnitsRecursive(Recursion.New(Recursion.defaultMaxDepth)).Cast<Unit>())
                            {
                                if (!string.IsNullOrEmpty(NodeGenerator.GetSingleDecorator(_unit, _unit).NameSpace))
                                    usings.Add(NodeGenerator.GetSingleDecorator(_unit, _unit).NameSpace);
                            }

                            @struct.AddUsings(usings);
                            var data = new ControlGenerationData();
                            data.NewScope();
                            foreach (var variable in Data.variables.Where(variable => variable.FieldName != Data.variables[i].FieldName))
                            {
                                data.AddLocalNameInScope(variable.FieldName, variable.type);
                            }
                            property.MultiStatementGetter(AccessModifier.Public, (Data.variables[i].getter.graph.units[0] as Unit)
                            .GenerateControl(null, new ControlGenerationData() { returns = Data.variables[i].type }, 0));
                            data.ExitScope();
                        }

                        if (Data.variables[i].set)
                        {
                            var usings = new List<string>();
                            foreach (var _unit in Data.variables[i].setter.graph.GetUnitsRecursive(Recursion.New(Recursion.defaultMaxDepth)).Cast<Unit>())
                            {
                                if (!string.IsNullOrEmpty(NodeGenerator.GetSingleDecorator(_unit, _unit).NameSpace))
                                    usings.Add(NodeGenerator.GetSingleDecorator(_unit, _unit).NameSpace);
                            }

                            @struct.AddUsings(usings);
                            var data = new ControlGenerationData();
                            data.NewScope();
                            foreach (var variable in Data.variables.Where(variable => variable.FieldName != Data.variables[i].FieldName))
                            {
                                data.AddLocalNameInScope(variable.FieldName, variable.type);
                            }
                            property.MultiStatementSetter(AccessModifier.Public, (Data.variables[i].setter.graph.units[0] as Unit)
                            .GenerateControl(null, data, 0));
                            data.ExitScope();
                        }

                        @struct.AddProperty(property);
                    }
                    else
                    {
                        var field = FieldGenerator.Field(Data.variables[i].scope, Data.variables[i].fieldModifier, Data.variables[i].type, Data.variables[i].name);

                        if (Data.serialized)
                        {
                            if (Data.variables[i].inspectable) field.AddAttribute(AttributeGenerator.Attribute<InspectableAttribute>());
                            if (!Data.variables[i].serialized) field.AddAttribute(AttributeGenerator.Attribute<NonSerializedAttribute>());
                        }

                        for (int attrIndex = 0; attrIndex < attributes.Count; attrIndex++)
                        {
                            AttributeGenerator attrGenerator = AttributeGenerator.Attribute(attributes[attrIndex].GetAttributeType());
                            foreach (var param in attributes[attrIndex].parameters)
                            {
                                if (param.defaultValue is IList list)
                                {
                                    if (param.isParamsParameter)
                                    {
                                        foreach (var item in list)
                                        {
                                            attrGenerator.AddParameter(item);
                                        }
                                    }
                                    else
                                    {
                                        attrGenerator.AddParameter(param.defaultValue);
                                    }
                                }
                                else
                                {
                                    if (!attrGenerator.parameterValues.Contains(param))
                                    {
                                        attrGenerator.AddParameter(param.defaultValue);
                                    }
                                }
                            }
                            field.AddAttribute(attrGenerator);
                        }

                        @struct.AddField(field);
                    }
                }
            }

            for (int i = 0; i < Data.methods.Count; i++)
            {
                if (!string.IsNullOrEmpty(Data.methods[i].name) && Data.methods[i].returnType != null)
                {
                    var method = MethodGenerator.Method(Data.methods[i].scope, Data.methods[i].modifier, Data.methods[i].returnType, Data.methods[i].name);
                    var attributes = Data.methods[i].attributes;

                    for (int attrIndex = 0; attrIndex < attributes.Count; attrIndex++)
                    {
                        AttributeGenerator attrGenerator = AttributeGenerator.Attribute(attributes[attrIndex].GetAttributeType());
                        foreach (var param in attributes[attrIndex].parameters)
                        {
                            if (param.defaultValue is IList list)
                            {
                                if (param.isParamsParameter)
                                {
                                    foreach (var item in list)
                                    {
                                        attrGenerator.AddParameter(item);
                                    }
                                }
                                else
                                {
                                    attrGenerator.AddParameter(param.defaultValue);
                                }
                            }
                            else
                            {
                                if (!attrGenerator.parameterValues.Contains(param))
                                {
                                    attrGenerator.AddParameter(param.defaultValue);
                                }
                            }
                        }
                        method.AddAttribute(attrGenerator);
                    }
                    if (Data.methods[i].graph.units.Count > 0)
                    {
                        var usings = new List<string>();
                        foreach (var _unit in Data.methods[i].graph.GetUnitsRecursive(Recursion.New(Recursion.defaultMaxDepth)).Cast<Unit>())
                        {
                            if (!string.IsNullOrEmpty(NodeGenerator.GetSingleDecorator(_unit, _unit).NameSpace))
                                usings.Add(NodeGenerator.GetSingleDecorator(_unit, _unit).NameSpace);
                        }

                        @struct.AddUsings(usings);
                        var unit = Data.methods[i].graph.units[0] as FunctionNode;
                        var data = new ControlGenerationData();
                        data.NewScope();
                        for (int item = 0; item < Data.variables.Count; item++)
                        {
                            data.AddLocalNameInScope(Data.variables[item].name, Data.variables[item].type);
                        }
                        method.Body(unit.GenerateControl(null, data, 0));
                        data.ExitScope();
                        for (int pIndex = 0; pIndex < Data.methods[i].parameters.Count; pIndex++)
                        {
                            if (!string.IsNullOrEmpty(Data.methods[i].parameters[pIndex].name)) method.AddParameter(ParameterGenerator.Parameter(Data.methods[i].parameters[pIndex].name, Data.methods[i].parameters[pIndex].type, Libraries.CSharp.ParameterModifier.None));
                        }
                    }

                    @struct.AddMethod(method);
                }
            }

            if (@namespace != null)
            {
                @namespace.AddStruct(@struct);
            }

            return @struct;
        }
    }
}
