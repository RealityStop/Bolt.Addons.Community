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
        protected override TypeGenerator OnGenerateType(ref string output, NamespaceGenerator @namespace)
        {
            var @struct = StructGenerator.Struct(RootAccessModifier.Public, StructModifier.None, Data.title.LegalMemberName());
            @struct.beforeUsings = "#pragma warning disable\n".ConstructHighlight();
            if (Data.definedEvent) @struct.ImplementInterface(typeof(IDefinedEvent));
            if (Data.inspectable) @struct.AddAttribute(AttributeGenerator.Attribute<InspectableAttribute>());
            if (Data.serialized) @struct.AddAttribute(AttributeGenerator.Attribute<SerializableAttribute>());
            if (Data.includeInSettings) @struct.AddAttribute(AttributeGenerator.Attribute<IncludeInSettingsAttribute>().AddParameter(true));
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
                    var usings = new List<string>();
                    foreach (var _unit in Data.constructors[i].graph.GetUnitsRecursive(Recursion.New(Recursion.defaultMaxDepth)).Cast<Unit>())
                    {
<<<<<<< Updated upstream
                        if (!string.IsNullOrEmpty(NodeGenerator.GetSingleDecorator(_unit, _unit).NameSpaces))
                            usings.Add(NodeGenerator.GetSingleDecorator(_unit, _unit).NameSpaces);
=======
                        var generator = unit.GetGenerator();
                        if (generator.GetType().IsDefined(typeof(RequiresVariablesAttribute), true))
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
=======
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
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
>>>>>>> Stashed changes
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
>>>>>>> Stashed changes

                        HandleOtherGenerators(@struct, _unit.GetGenerator(), Data.constructors[i].GetReference());
                    }

                    @struct.AddUsings(usings);
                    var unit = Data.constructors[i].graph.units[0] as FunctionNode;
                    var data = new ControlGenerationData(Data.constructors[i].GetReference());
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
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
                            foreach (var _unit in Data.variables[i].getter.graph.GetUnitsRecursive(Recursion.New(Recursion.defaultMaxDepth)).Cast<Unit>())
                            {
                                if (!string.IsNullOrEmpty(NodeGenerator.GetSingleDecorator(_unit, _unit).NameSpaces))
                                    usings.Add(NodeGenerator.GetSingleDecorator(_unit, _unit).NameSpaces);

                                HandleOtherGenerators(@struct, _unit.GetGenerator(), Data.variables[i].getter.GetReference());
                            }
=======
>>>>>>> Stashed changes

=======
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes

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

>>>>>>> Stashed changes
                            @struct.AddUsings(usings);
                            var data = new ControlGenerationData(Data.variables[i].getter.GetReference())
                            {
                                returns = Data.variables[i].type
                            };
                            data.NewScope();
                            foreach (var variable in Data.variables.Where(variable => variable.FieldName != Data.variables[i].FieldName))
                            {
                                data.AddLocalNameInScope(variable.FieldName, variable.type);
                            }
                            property.MultiStatementGetter(AccessModifier.Public, (Data.variables[i].getter.graph.units[0] as Unit)
                            .GenerateControl(null, data, 0));
                            data.ExitScope();
                        }

                        if (Data.variables[i].set)
                        {
                            var usings = new List<string>();
                            foreach (var _unit in Data.variables[i].setter.graph.GetUnitsRecursive(Recursion.New(Recursion.defaultMaxDepth)).Cast<Unit>())
                            {
<<<<<<< Updated upstream
                                if (!string.IsNullOrEmpty(NodeGenerator.GetSingleDecorator(_unit, _unit).NameSpaces))
                                    usings.Add(NodeGenerator.GetSingleDecorator(_unit, _unit).NameSpaces);
=======
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
>>>>>>> Stashed changes

                                HandleOtherGenerators(@struct, _unit.GetGenerator(), Data.variables[i].setter.GetReference());
                            }

                            @struct.AddUsings(usings);
                            var data = new ControlGenerationData(Data.variables[i].setter.GetReference()) { returns = typeof(void) };
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
                        foreach (var _unit in Data.methods[i].graph.GetUnitsRecursive(Recursion.New(Recursion.defaultMaxDepth)).Cast<Unit>())
                        {
<<<<<<< Updated upstream
                            if (!string.IsNullOrEmpty(NodeGenerator.GetSingleDecorator(_unit, _unit).NameSpaces))
                                usings.Add(NodeGenerator.GetSingleDecorator(_unit, _unit).NameSpaces);
                            HandleOtherGenerators(@struct, _unit.GetGenerator(), Data.methods[i].GetReference());
                        }
=======
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
>>>>>>> Stashed changes

                        @struct.AddUsings(usings);
                        var unit = Data.methods[i].graph.units[0] as FunctionNode;
                        var data = new ControlGenerationData(Data.methods[i].GetReference()) { returns = Data.methods[i].returnType, mustReturn = Data.methods[i].returnType != typeof(void) || Data.methods[i].returnType != typeof(Libraries.CSharp.Void) };
                        data.NewScope();
                        for (int item = 0; item < Data.variables.Count; item++)
                        {
                            data.AddLocalNameInScope(Data.variables[item].name, Data.variables[item].type);
                        }
                        method.Body(unit.GenerateControl(null, data, 0));
                        data.ExitScope();
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

        private void HandleOtherGenerators(StructGenerator @struct, NodeGenerator generator, GraphPointer graphPointer)
        {
            if (generator is VariableNodeGenerator variableGenerator)
            {
                var existingFields = new HashSet<string>(@struct.GetFields().Select(f => f.name));
                variableGenerator.count = 0;

                while (existingFields.Contains(variableGenerator.Name))
                {
                    variableGenerator.count++;
                }

                @struct.AddField(FieldGenerator.Field(variableGenerator.AccessModifier, variableGenerator.FieldModifier, variableGenerator.Type, variableGenerator.Name));
            }
            else if (generator is MethodNodeGenerator methodGenerator && methodGenerator.unit is not IEventUnit)
            {
                var existingMethods = new HashSet<string>(@struct.GetMethods().Select(m => m.name));
                methodGenerator.count = 0;

                while (existingMethods.Contains(methodGenerator.Name))
                {
                    methodGenerator.count++;
                }
                var data = new ControlGenerationData(graphPointer) { ScriptType = typeof(ValueType) };
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
                        method.AddParameter(ParameterGenerator.Parameter(param.name, genericString, param.type, param.modifier));
                    }
                }

                foreach (var variable in Data.variables)
                {
                    methodGenerator.Data.AddLocalNameInScope(variable.FieldName, variable.type);
                }
                methodGenerator.Data = data;
                method.Body(string.IsNullOrEmpty(methodGenerator.MethodBody) ? methodGenerator.GenerateControl(methodGenerator.unit.controlInputs.Count == 0 ? null : methodGenerator.unit.controlInputs[0], data, 0) : methodGenerator.MethodBody); @struct.AddMethod(method);
            }
        }
    }
}
