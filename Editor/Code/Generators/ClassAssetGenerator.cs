using Unity.VisualScripting.Community.Libraries.CSharp;
using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

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

            var decorators = new Dictionary<Type, List<NodeGenerator>>();
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
                @class.AddAttribute(attrGenerator);
            }

            for (int i = 0; i < Data.constructors.Count; i++)
            {
                var constructor = ConstructorGenerator.Constructor(Data.constructors[i].scope, Data.constructors[i].modifier, Data.title.LegalMemberName());
                if (Data.constructors[i].graph.units.Count > 0)
                {
                    var usings = new List<string>();
                    foreach (var _unit in Data.constructors[i].graph.GetUnitsRecursive(Recursion.New(Recursion.defaultMaxDepth)).Cast<Unit>())
                    {
                        var generator = NodeGenerator.GetSingleDecorator(_unit, _unit);
                        if (decorators.TryGetValue(_unit.GetType(), out List<NodeGenerator> list))
                        {
                            decorators[_unit.GetType()].Add(generator);
                        }
                        else
                        {
                            decorators.Add(_unit.GetType(), new List<NodeGenerator>() { generator });
                        }
                        if (!string.IsNullOrEmpty(generator.NameSpace))
                            usings.Add(generator.NameSpace);
                    }
                    var generationData = new ControlGenerationData();
                    foreach (var variable in Data.variables)
                    {
                        generationData.AddLocalNameInScope(variable.FieldName);
                    }

                    generationData.returns = Data.variables[i].type;
                    var unit = Data.constructors[i].graph.units[0] as FunctionNode;
                    constructor.Body(FunctionNodeGenerator.GetSingleDecorator(unit, unit).GenerateControl(null, generationData, 0));
                    generationData.ExitScope();
                    for (int pIndex = 0; pIndex < Data.constructors[i].parameters.Count; pIndex++)
                    {
                        if (!string.IsNullOrEmpty(Data.constructors[i].parameters[pIndex].name)) constructor.AddParameter(false, ParameterGenerator.Parameter(Data.constructors[i].parameters[pIndex].name, Data.constructors[i].parameters[pIndex].type, ParameterModifier.None));
                    }
                    @class.AddUsings(usings);
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
                            var generationData = new ControlGenerationData();
                            foreach (var variable in Data.variables)
                            {
                                generationData.AddLocalNameInScope(variable.FieldName);
                            }

                            generationData.returns = Data.variables[i].type;
                            property.MultiStatementGetter(AccessModifier.Public, NodeGenerator.GetSingleDecorator(Data.variables[i].getter.graph.units[0] as Unit, Data.variables[i].getter.graph.units[0] as Unit)
                            .GenerateControl(null, generationData, 0));
                            generationData.ExitScope();
                            var usings = new List<string>();
                            foreach (var _unit in Data.variables[i].getter.graph.GetUnitsRecursive(Recursion.New(Recursion.defaultMaxDepth)).Cast<Unit>())
                            {
                                var generator = NodeGenerator.GetSingleDecorator(_unit, _unit);
                                if (decorators.TryGetValue(_unit.GetType(), out List<NodeGenerator> list))
                                {
                                    decorators[_unit.GetType()].Add(generator);
                                }
                                else
                                {
                                    decorators.Add(_unit.GetType(), new List<NodeGenerator>() { generator });
                                }
                                if (!string.IsNullOrEmpty(generator.NameSpace))
                                    usings.Add(generator.NameSpace);
                            }

                            @class.AddUsings(usings);
                        }

                        if (Data.variables[i].set)
                        {
                            var generationData = new ControlGenerationData();
                            foreach (var variable in Data.variables)
                            {
                                generationData.AddLocalNameInScope(variable.FieldName);
                            }

                            generationData.returns = Data.variables[i].type;
                            property.MultiStatementSetter(AccessModifier.Public, NodeGenerator.GetSingleDecorator(Data.variables[i].setter.graph.units[0] as Unit, Data.variables[i].setter.graph.units[0] as Unit)
                            .GenerateControl(null, generationData, 0));
                            generationData.ExitScope();
                            var usings = new List<string>();
                            foreach (var _unit in Data.variables[i].setter.graph.GetUnitsRecursive(Recursion.New(Recursion.defaultMaxDepth)).Cast<Unit>())
                            {
                                var generator = NodeGenerator.GetSingleDecorator(_unit, _unit);
                                if (decorators.TryGetValue(_unit.GetType(), out List<NodeGenerator> list))
                                {
                                    decorators[_unit.GetType()].Add(generator);
                                }
                                else
                                {
                                    decorators.Add(_unit.GetType(), new List<NodeGenerator>() { generator });
                                }
                                if (!string.IsNullOrEmpty(generator.NameSpace))
                                    usings.Add(generator.NameSpace);
                            }

                            @class.AddUsings(usings);
                        }

                        @class.AddProperty(property);
                    }
                    else
                    {
                        var field = FieldGenerator.Field(Data.variables[i].scope, Data.variables[i].fieldModifier, Data.variables[i].type, Data.variables[i].name, Data.variables[i].defaultValue);
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

                        @class.AddField(field);
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
                            var generator = NodeGenerator.GetSingleDecorator(_unit, _unit);
                            if (decorators.TryGetValue(_unit.GetType(), out List<NodeGenerator> list))
                            {
                                decorators[_unit.GetType()].Add(generator);
                            }
                            else
                            {
                                decorators.Add(_unit.GetType(), new List<NodeGenerator>() { generator });
                            }
                            if (!string.IsNullOrEmpty(generator.NameSpace))
                                usings.Add(generator.NameSpace);
                        }
                        var generationData = new ControlGenerationData();
                        foreach (var variable in Data.variables)
                        {
                            generationData.AddLocalNameInScope(variable.FieldName);
                        }
                        @class.AddUsings(usings);
                        var unit = Data.methods[i].graph.units[0] as FunctionNode;
                        method.Body(FunctionNodeGenerator.GetSingleDecorator(unit, unit).GenerateControl(null, generationData, 0));
                        generationData.ExitScope();

                        for (int pIndex = 0; pIndex < Data.methods[i].parameters.Count; pIndex++)
                        {
                            if (!string.IsNullOrEmpty(Data.methods[i].parameters[pIndex].name)) method.AddParameter(ParameterGenerator.Parameter(Data.methods[i].parameters[pIndex].name, Data.methods[i].parameters[pIndex].type, ParameterModifier.None));
                        }
                    }

                    @class.AddMethod(method);
                }
            }

            foreach (var key in decorators.Keys)
            {
                foreach (var generator in decorators[key])
                {
                    TriggerHandleOtherGenerators(key, @class, generator);
                }
            }

            @namespace.AddClass(@class);

            return @class;
        }

        private void TriggerHandleOtherGenerators(Type type, ClassGenerator @class, NodeGenerator generator)
        {
            GetType().GetMethod(nameof(HandleOtherGenerators), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.NonPublic).MakeGenericMethod(type).Invoke(this, new object[] { @class, generator });
        }

        private void HandleOtherGenerators<T>(ClassGenerator @class, NodeGenerator generator) where T : Unit
        {
            if (generator is VariableNodeGenerator<T> variableGenerator)
            {
                var count = 0;
                while (@class.fields.Any(field => field.name == variableGenerator.Name))
                {
                    variableGenerator.count = count;
                    count++;
                }
                @class.AddField(FieldGenerator.Field(variableGenerator.AccessModifier, variableGenerator.FieldModifier, variableGenerator.Type, variableGenerator.Name));
            }
            else if (generator is MethodNodeGenerator<T> methodGenerator)
            {
                var count = 0;
                while (@class.methods.Any(method => method.name == methodGenerator.Name))
                {
                    methodGenerator.count = count;
                    count++;
                }
                var method = MethodGenerator.Method(methodGenerator.AccessModifier, methodGenerator.MethodModifier, methodGenerator.Type, methodGenerator.Name);
                foreach (var param in methodGenerator.Parameters)
                {
                    method.AddParameter(ParameterGenerator.Parameter(param.name, param.type, ParameterModifier.None));
                }

                var generationData = new ControlGenerationData();
                foreach (var variable in Data.variables)
                {
                    generationData.AddLocalNameInScope(variable.FieldName);
                }
                method.Body(methodGenerator.MethodBody);
                @class.AddMethod(method);
                generationData.ExitScope();
            }
        }
    }
}