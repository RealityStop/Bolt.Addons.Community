using Unity.VisualScripting.Community.Libraries.CSharp;
using System;
using Unity.VisualScripting;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    [CodeGenerator(typeof(StructAsset))]
    public sealed class StructAssetGenerator : MemberTypeAssetGenerator<StructAsset, StructFieldDeclaration, StructMethodDeclaration, StructConstructorDeclaration>
    {
        private ControlGenerationData data;
        protected override TypeGenerator OnGenerateType(ref string output, NamespaceGenerator @namespace)
        {
            var @struct = StructGenerator.Struct(RootAccessModifier.Public, StructModifier.None, Data.title.LegalMemberName());
            CreateGenerationData();
            @struct.beforeUsings = "#pragma warning disable\n".ConstructHighlight();
            if (Data.definedEvent) @struct.ImplementInterface(typeof(IDefinedEvent));
            if (Data.inspectable) @struct.AddAttribute(AttributeGenerator.Attribute<InspectableAttribute>());
            if (Data.serialized) @struct.AddAttribute(AttributeGenerator.Attribute<SerializableAttribute>());
            if (Data.includeInSettings) @struct.AddAttribute(AttributeGenerator.Attribute<IncludeInSettingsAttribute>().AddParameter(true));
            @namespace.beforeUsings = "#pragma warning disable".ConstructHighlight();
            foreach (var @interface in Data.interfaces.Select(i => i.type))
            {
                @struct.ImplementInterface(@interface);
            }
            Dictionary<string, int> methodIndex = new Dictionary<string, int>();
            foreach (var attribute in Data.attributes)
            {
                var attrGenerator = AttributeGenerator.Attribute(attribute.GetAttributeType());
                foreach (var param in attribute.parameters)
                {
                    if (param.defaultValue is IList list)
                    {
                        if (param.modifier == Libraries.CSharp.ParameterModifier.Params)
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
                var constructor = ConstructorGenerator.Constructor(Data.constructors[i].scope, Data.constructors[i].modifier, Data.constructors[i].initializerType, Data.title.LegalMemberName());
                if (Data.constructors[i].graph.units.Count > 0)
                {
                    data.EnterMethod();
                    data.SetReturns(typeof(void));
                    data.SetGraphPointer(Data.constructors[i].GetReference().AsReference());
                    var usings = new List<string>();
                    GraphTraversal.TraverseFlowGraph(Data.constructors[i].graph, unit =>
                    {
                        var generator = unit.GetGenerator();
                        if (generator.GetType().IsDefined(typeof(RequiresVariablesAttribute), true))
                        {
                            if (generator is IRequireVariables variables)
                            {
                                foreach (var variable in variables.GetRequiredVariables(data))
                                {
                                    @struct.AddField(variable);
                                }
                            }
                            else
                            {
                                Debug.LogError(generator.GetType().DisplayName() + "Requires Variables does not implement IRequiresVariables");
                            }
                        }

                        if (generator.GetType().IsDefined(typeof(RequiresMethodsAttribute), true))
                        {
                            if (generator is IRequireMethods methods)
                            {
                                foreach (var method in methods.GetRequiredMethods(data))
                                {
                                    var originalName = method.name;

                                    if (!methodIndex.TryGetValue(originalName, out var index))
                                    {
                                        methodIndex[originalName] = 0;
                                    }
                                    else
                                    {
                                        index++;
                                        methodIndex[originalName] = index;
                                        method.name = originalName + index;
                                    }

                                    @struct.AddMethod(method);
                                }
                            }
                            else
                            {
                                Debug.LogError(generator.GetType().DisplayName() + "Requires Methods but does not implement IRequiresMethods");
                            }
                        }

                        if (!string.IsNullOrEmpty(generator.NameSpaces))
                            usings.Add(generator.NameSpaces.Replace("`", ",").Trim());

                        if (generator is InterfaceNodeGenerator interfaceNodeGenerator)
                        {
                            foreach (var interfaceType in interfaceNodeGenerator.InterfaceTypes)
                            {
                                @struct.ImplementInterface(interfaceType);
                            }
                        }

                        HandleOtherGenerators(@struct, unit.GetGenerator());
                    });

                    @struct.AddUsings(usings);
                    var unit = Data.constructors[i].graph.units[0] as FunctionNode;
                    foreach (var param in Data.constructors[i].parameters)
                    {
                        data.AddLocalNameInScope(param.name, param.type);
                    }
                    constructor.Body(unit.GenerateControl(null, data, 0));
                    for (int pIndex = 0; pIndex < Data.constructors[i].parameters.Count; pIndex++)
                    {
                        if (!string.IsNullOrEmpty(Data.constructors[i].parameters[pIndex].name)) constructor.AddParameter(false, ParameterGenerator.Parameter(Data.constructors[i].parameters[pIndex].name, Data.constructors[i].parameters[pIndex].type, Libraries.CSharp.ParameterModifier.None));
                    }
                    data.ExitMethod();
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
                                    if (param.modifier == Libraries.CSharp.ParameterModifier.Params)
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

                            GraphTraversal.TraverseFlowGraph(Data.variables[i].getter.graph, unit =>
                            {
                                var generator = unit.GetGenerator();
                                if (generator.GetType().IsDefined(typeof(RequiresVariablesAttribute), true))
                                {
                                    if (generator is IRequireVariables variables)
                                    {
                                        foreach (var variable in variables.GetRequiredVariables(data))
                                        {
                                            @struct.AddField(variable);
                                        }
                                    }
                                    else
                                    {
                                        Debug.LogError(generator.GetType().DisplayName() + "Requires Variables does not implement IRequiresVariables");
                                    }
                                }

                                if (generator.GetType().IsDefined(typeof(RequiresMethodsAttribute), true))
                                {
                                    if (generator is IRequireMethods methods)
                                    {
                                        foreach (var method in methods.GetRequiredMethods(data))
                                        {
                                            var originalName = method.name;

                                            if (!methodIndex.TryGetValue(originalName, out var index))
                                            {
                                                methodIndex[originalName] = 0;
                                            }
                                            else
                                            {
                                                index++;
                                                methodIndex[originalName] = index;
                                                method.name = originalName + index;
                                            }

                                            @struct.AddMethod(method);
                                        }
                                    }
                                    else
                                    {
                                        Debug.LogError(generator.GetType().DisplayName() + "Requires Methods but does not implement IRequiresMethods");
                                    }
                                }
                                if (!string.IsNullOrEmpty(generator.NameSpaces))
                                    usings.Add(generator.NameSpaces.Replace("`", ",").Trim());

                                if (generator is InterfaceNodeGenerator interfaceNodeGenerator)
                                {
                                    foreach (var interfaceType in interfaceNodeGenerator.InterfaceTypes)
                                    {
                                        @struct.ImplementInterface(interfaceType);
                                    }
                                }

                                HandleOtherGenerators(@struct, unit.GetGenerator());
                            });

                            @struct.AddUsings(usings);
                            data.EnterMethod();
                            data.SetReturns(Data.variables[i].type);
                            data.SetGraphPointer(Data.variables[i].getter.GetReference().AsReference());
                            property.MultiStatementGetter(AccessModifier.Public, (Data.variables[i].getter.graph.units[0] as Unit)
                            .GenerateControl(null, data, 0));
                            data.ExitMethod();
                        }

                        if (Data.variables[i].set)
                        {
                            var usings = new List<string>();
                            GraphTraversal.TraverseFlowGraph(Data.variables[i].setter.graph, unit =>
                            {
                                var generator = unit.GetGenerator();

                                if (generator.GetType().IsDefined(typeof(RequiresVariablesAttribute), true))
                                {
                                    if (generator is IRequireVariables variables)
                                    {
                                        foreach (var variable in variables.GetRequiredVariables(data))
                                        {
                                            @struct.AddField(variable);
                                        }
                                    }
                                    else
                                    {
                                        Debug.LogError(generator.GetType().DisplayName() + "Requires Variables does not implement IRequiresVariables");
                                    }
                                }

                                if (generator.GetType().IsDefined(typeof(RequiresMethodsAttribute), true))
                                {
                                    if (generator is IRequireMethods methods)
                                    {
                                        foreach (var method in methods.GetRequiredMethods(data))
                                        {
                                            var originalName = method.name;

                                            if (!methodIndex.TryGetValue(originalName, out var index))
                                            {
                                                methodIndex[originalName] = 0;
                                            }
                                            else
                                            {
                                                index++;
                                                methodIndex[originalName] = index;
                                                method.name = originalName + index;
                                            }

                                            @struct.AddMethod(method);
                                        }
                                    }
                                    else
                                    {
                                        Debug.LogError(generator.GetType().DisplayName() + "Requires Methods but does not implement IRequiresMethods");
                                    }
                                }

                                if (!string.IsNullOrEmpty(generator.NameSpaces))
                                    usings.Add(generator.NameSpaces.Replace("`", ",").Trim());

                                if (generator is InterfaceNodeGenerator interfaceNodeGenerator)
                                {
                                    foreach (var interfaceType in interfaceNodeGenerator.InterfaceTypes)
                                    {
                                        @struct.ImplementInterface(interfaceType);
                                    }
                                }

                                HandleOtherGenerators(@struct, unit.GetGenerator());
                            });


                            @struct.AddUsings(usings);
                            data.EnterMethod();
                            data.SetReturns(typeof(void));
                            data.SetGraphPointer(Data.variables[i].setter.GetReference().AsReference());
                            data.AddLocalNameInScope("value", Data.variables[i].type);

                            property.MultiStatementSetter(AccessModifier.Public, (Data.variables[i].setter.graph.units[0] as Unit)
                            .GenerateControl(null, data, 0));
                            data.ExitMethod();
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
                                    if (param.modifier == Libraries.CSharp.ParameterModifier.Params)
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
                                if (param.modifier == Libraries.CSharp.ParameterModifier.Params)
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
                        GraphTraversal.TraverseFlowGraph(Data.methods[i].graph, unit =>
                        {
                            var generator = unit.GetGenerator();

                            if (generator.GetType().IsDefined(typeof(RequiresVariablesAttribute), true))
                            {
                                if (generator is IRequireVariables variables)
                                {
                                    foreach (var variable in variables.GetRequiredVariables(data))
                                    {
                                        @struct.AddField(variable);
                                    }
                                }
                                else
                                {
                                    Debug.LogError(generator.GetType().DisplayName() + "Requires Variables does not implement IRequiresVariables");
                                }
                            }

                            if (generator.GetType().IsDefined(typeof(RequiresMethodsAttribute), true))
                            {
                                if (generator is IRequireMethods methods)
                                {
                                    foreach (var method in methods.GetRequiredMethods(data))
                                    {
                                        var originalName = method.name;

                                        if (!methodIndex.TryGetValue(originalName, out var index))
                                        {
                                            methodIndex[originalName] = 0;
                                        }
                                        else
                                        {
                                            index++;
                                            methodIndex[originalName] = index;
                                            method.name = originalName + index;
                                        }

                                        @struct.AddMethod(method);
                                    }
                                }
                                else
                                {
                                    Debug.LogError(generator.GetType().DisplayName() + "Requires Methods but does not implement IRequiresMethods");
                                }
                            }

                            if (!string.IsNullOrEmpty(generator.NameSpaces))
                                usings.Add(generator.NameSpaces.Replace("`", ",").Trim());

                            if (generator is InterfaceNodeGenerator interfaceNodeGenerator)
                            {
                                foreach (var interfaceType in interfaceNodeGenerator.InterfaceTypes)
                                {
                                    @struct.ImplementInterface(interfaceType);
                                }
                            }

                            HandleOtherGenerators(@struct, unit.GetGenerator());
                        });

                        @struct.AddUsings(usings);
                        var unit = Data.methods[i].graph.units[0] as FunctionNode;
                        data.EnterMethod();
                        data.SetReturns(Data.methods[i].returnType);
                        data.SetGraphPointer(Data.constructors[i].GetReference().AsReference());
                        foreach (var param in Data.methods[i].parameters)
                        {
                            data.AddLocalNameInScope(param.name, param.type);
                        }
                        method.Body(unit.GenerateControl(null, data, 0));
                        data.ExitMethod();
                        for (int pIndex = 0; pIndex < Data.methods[i].parameters.Count; pIndex++)
                        {
                            if (!string.IsNullOrEmpty(Data.methods[i].parameters[pIndex].name)) method.AddParameter(ParameterGenerator.Parameter(Data.methods[i].parameters[pIndex].name, Data.methods[i].parameters[pIndex].type, ParameterModifier.None));
                        }
                    }

                    @struct.AddMethod(method);
                }
            }
            var values = CodeGeneratorValueUtility.GetAllValues(Data);
            var index = 0;
            foreach (var variable in values)
            {
                var field = FieldGenerator.Field(AccessModifier.Public, FieldModifier.None, variable.Value != null ? variable.Value.GetType() : typeof(UnityEngine.Object), variable.Key.LegalMemberName());

                var attribute = AttributeGenerator.Attribute(typeof(FoldoutAttribute));
                attribute.AddParameter("ObjectReferences");
                field.AddAttribute(attribute);

                @struct.AddField(field);
                index++;
            }
            @namespace?.AddStruct(@struct);

            return @struct;
        }

        Dictionary<Type, int> generatorCounts = new Dictionary<Type, int>();
        private void HandleOtherGenerators(StructGenerator @struct, NodeGenerator generator)
        {
            if (generator is VariableNodeGenerator variableGenerator)
            {
                var type = variableGenerator.unit.GetType();
                if (!generatorCounts.ContainsKey(type))
                {
                    generatorCounts[type] = 0;
                }
                variableGenerator.count = generatorCounts[type];
                generatorCounts[type]++;
                @struct.AddField(FieldGenerator.Field(variableGenerator.AccessModifier, variableGenerator.FieldModifier, variableGenerator.Type, variableGenerator.Name));
            }
            else if (generator is MethodNodeGenerator methodGenerator && methodGenerator.unit is not IEventUnit)
            {
                var type = methodGenerator.unit.GetType();
                if (!generatorCounts.ContainsKey(type))
                {
                    generatorCounts[type] = 0;
                }
                methodGenerator.count = generatorCounts[type];
                generatorCounts[type]++;
                foreach (var item in @struct.GetFields())
                {
                    data.AddLocalNameInScope(item.name, item.type);
                }
                var method = MethodGenerator.Method(methodGenerator.AccessModifier, methodGenerator.MethodModifier, methodGenerator.ReturnType, methodGenerator.Name);
                method.AddGenerics(methodGenerator.GenericCount);

                foreach (var param in methodGenerator.Parameters)
                {
                    if (methodGenerator.GenericCount == 0 || !param.usesGeneric)
                        method.AddParameter(ParameterGenerator.Parameter(param.name, param.type, param.modifier));
                    else if (methodGenerator.GenericCount > 0 && param.usesGeneric)
                    {
                        var genericString = method.generics[param.generic].name;
                        method.AddParameter(ParameterGenerator.Parameter(param.name, genericString.TypeHighlight(), param.type, param.modifier));
                    }
                }

                foreach (var variable in Data.variables)
                {
                    data.AddLocalNameInScope(variable.FieldName, variable.type);
                }
                methodGenerator.Data = data;
                methodGenerator.Data.EnterMethod();
                methodGenerator.Data.SetReturns(methodGenerator.ReturnType);
                var MethodBody = methodGenerator.MethodBody;
                method.Body(string.IsNullOrEmpty(MethodBody) ? methodGenerator.GenerateControl(null, data, 0) : MethodBody);
                methodGenerator.Data.ExitMethod();
                @struct.AddMethod(method);
            }
        }

        private void CreateGenerationData()
        {
            data = new ControlGenerationData(typeof(ValueType), null);
            foreach (var variable in Data.variables)
            {
                if (!string.IsNullOrEmpty(variable.FieldName))
                    data.AddLocalNameInScope(variable.FieldName, variable.type);
            }
        }
    }
}
