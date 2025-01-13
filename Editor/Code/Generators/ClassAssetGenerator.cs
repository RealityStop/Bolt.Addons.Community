using Unity.VisualScripting.Community.Libraries.CSharp;
using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.Humility;
using System.Reflection;
using Unity.VisualScripting.Community.Utility;
using UnityEngine.InputSystem;
using Unity.VisualScripting.InputSystem;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    [CodeGenerator(typeof(ClassAsset))]
    public sealed class ClassAssetGenerator : MemberTypeAssetGenerator<ClassAsset, ClassFieldDeclaration, ClassMethodDeclaration, ClassConstructorDeclaration>
    {
        /// <summary>
        /// Units that require the Update method in a monobehaviour to function
        /// </summary>
        private readonly List<Type> specialUnitTypes = new List<Type>() { typeof(Timer), typeof(Cooldown),
#if PACKAGE_INPUT_SYSTEM_EXISTS
            typeof(OnInputSystemEventButton), typeof(OnInputSystemEventVector2), typeof(OnInputSystemEventFloat)
#endif
        };
        private List<Unit> specialUnits = new List<Unit>();
        protected override TypeGenerator OnGenerateType(ref string output, NamespaceGenerator @namespace)
        {
            if (Data == null)
                return ClassGenerator.Class(RootAccessModifier.Public, ClassModifier.None, "", null);
            
            string className = Data.title.LegalMemberName();
            Type baseType = Data.scriptableObject
                ? typeof(ScriptableObject)
                : (Data.inheritsType && Data.inherits.type != null ? Data.GetInheritedType() : typeof(object));

            var @class = ClassGenerator.Class(RootAccessModifier.Public, ClassModifier.None, className, baseType);
            @class.beforeUsings = "#pragma warning disable\n".ConstructHighlight();

            if (Data.definedEvent)
                @class.ImplementInterface(typeof(IDefinedEvent));

            if (Data.inspectable)
                @class.AddAttribute(AttributeGenerator.Attribute<InspectableAttribute>());

            if (Data.serialized)
                @class.AddAttribute(AttributeGenerator.Attribute<SerializableAttribute>());

            if (Data.includeInSettings)
                @class.AddAttribute(AttributeGenerator.Attribute<IncludeInSettingsAttribute>().AddParameter(true));

            if (Data.scriptableObject)
            {
                @class.AddAttribute(AttributeGenerator.Attribute<CreateAssetMenuAttribute>()
                    .AddParameter("menuName", Data.menuName)
                    .AddParameter("fileName", Data.fileName)
                    .AddParameter("order", Data.order));
            }

            if (Data.inheritsType && Data.inherits.type != null)
            {
                @class.AddUsings(new List<string> { Data.inherits.type.Namespace });
            }

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
                        else if (!attrGenerator.parameterValues.Contains(param))
                        {
                            attrGenerator.AddParameter(param.defaultValue);
                        }
                    }
                    else if (!attrGenerator.parameterValues.Contains(param))
                    {
                        attrGenerator.AddParameter(param.defaultValue);
                    }
                }
                @class.AddAttribute(attrGenerator);
            }

            foreach (var @interface in Data.interfaces)
            {
                @class.ImplementInterface(@interface.type);
            }

            foreach (var constructorData in Data.constructors)
            {
                var parameters = constructorData.parameters;
                if (@class.constructors.Any(f => f.parameters.Select(param => param.generator.type).SequenceEqual(parameters.Select(param => param.type))))
                {
                    continue;
                }

                var constructor = ConstructorGenerator.Constructor(constructorData.scope, constructorData.modifier, constructorData.initalizerType, className);

                if (constructorData.graph.units.Count > 0)
                {
                    @class.AddUsings(ProcessGraphUnits(constructorData.graph, @class));
                    var generationData = CreateGenerationData(typeof(void));
                    constructor.Body((constructorData.graph.units[0] as Unit).GenerateControl(null, generationData, 0));

                    foreach (var param in parameters)
                    {
                        param.showInitalizer = true;
                        if (!string.IsNullOrEmpty(param.name))
                        {
                            constructor.AddParameter(param.useInInitalizer, CreateParameter(param));
                        }
                    }

                    @class.AddConstructor(constructor);
                }
            }

            foreach (var variableData in Data.variables)
            {
                if (!string.IsNullOrEmpty(variableData.name) && variableData.type != null)
                {
                    if (@class.fields.Any(f => f.name == variableData.FieldName) || @class.properties.Any(p => p.name == variableData.FieldName))
                    {
                        continue;
                    }

                    if (variableData.isProperty)
                    {
                        ProcessProperty(variableData, @class);
                    }
                    else
                    {
                        ProcessField(variableData, @class);
                    }
                }
            }

            foreach (var methodData in Data.methods)
            {
                if (!string.IsNullOrEmpty(methodData.name) && methodData.returnType != null)
                {
                    if (@class.methods.Any(m => m.name == methodData.name && m.parameters.Select(p => p.type).SequenceEqual(methodData.parameters.Select(p => p.type))))
                    {
                        continue;
                    }

                    var method = MethodGenerator.Method(methodData.scope, methodData.modifier, methodData.returnType, methodData.name);
                    AddMethodAttributes(method, methodData);

                    if (methodData.graph.units.Count > 0)
                    {
                        var generationData = CreateGenerationData(methodData.returnType);
                        @class.AddUsings(ProcessGraphUnits(methodData.graph, @class));
                        var unit = methodData.graph.units[0] as FunctionNode;
                        method.Body(unit.GenerateControl(null, generationData, 0));

                        foreach (var param in methodData.parameters)
                        {
                            if (!string.IsNullOrEmpty(param.name))
                            {
                                method.AddParameter(CreateParameter(param));
                            }
                        }
                    }
                    @class.AddMethod(method);
                }
            }

            if (specialUnits.Count > 0)
            {
                HashSet<Unit> visited = new HashSet<Unit>();
                if (@class.methods.Any(m => m.name.Replace(" ", "") == "Update"))
                {
                    var method = @class.methods.First(m => m.name.Replace(" ", "") == "Update");
                    foreach (var specialUnit in specialUnits)
                    {
                        if (!visited.Add(specialUnit))
                            continue;
                        var generator = NodeGenerator.GetSingleDecorator(specialUnit, specialUnit);
                        if (Data.inheritsType && typeof(MonoBehaviour).IsAssignableFrom(Data.GetInheritedType()))
                        {
                            if (generator is VariableNodeGenerator && !string.IsNullOrEmpty(generator.variableName))
                            {
#if PACKAGE_INPUT_SYSTEM_EXISTS
                                if (specialUnit is OnInputSystemEvent) continue;
#endif
                                method.beforeBody += string.Join("\n", specialUnits.Select(t => CodeBuilder.Indent(2) + CodeUtility.MakeSelectable(t, (NodeGenerator.GetSingleDecorator(t, t) as VariableNodeGenerator).Name.VariableHighlight() + ".Update();")));
                            }
                        }
                    }
#if PACKAGE_INPUT_SYSTEM_EXISTS
                    if (UnityEngine.InputSystem.InputSystem.settings.updateMode == InputSettings.UpdateMode.ProcessEventsInDynamicUpdate && Data.inheritsType && typeof(MonoBehaviour).IsAssignableFrom(Data.GetInheritedType()))
                    {
                        foreach (var unit in specialUnits.Where(unit => unit is OnInputSystemEvent).Cast<OnInputSystemEvent>())
                        {
                            if (!unit.trigger.hasValidConnection) continue;
                            method.beforeBody += CodeBuilder.Indent(2) + CodeUtility.MakeSelectable(unit, MethodNodeGenerator.GetSingleDecorator<MethodNodeGenerator>(unit, unit).Name + "();") + "\n";
                        }
                    }
#endif
                }
                else
                {
                    var method = MethodGenerator.Method(AccessModifier.None, MethodModifier.None, typeof(void), "Update");
                    foreach (var specialUnit in specialUnits)
                    {
                        if (!visited.Add(specialUnit))
                            continue;
                        var generator = NodeGenerator.GetSingleDecorator(specialUnit, specialUnit);
                        if (Data.inheritsType && typeof(MonoBehaviour).IsAssignableFrom(Data.GetInheritedType()))
                        {
                            if (generator is VariableNodeGenerator && !string.IsNullOrEmpty(generator.variableName))
                            {
#if PACKAGE_INPUT_SYSTEM_EXISTS
                                if (specialUnit is OnInputSystemEvent) continue;
#endif
                                method.beforeBody += string.Join("\n", specialUnits.Select(t => CodeBuilder.Indent(2) + CodeUtility.MakeSelectable(t, (NodeGenerator.GetSingleDecorator(t, t) as VariableNodeGenerator).Name.VariableHighlight() + ".Update();")));
                            }
                        }
                    }
#if PACKAGE_INPUT_SYSTEM_EXISTS
                    if (UnityEngine.InputSystem.InputSystem.settings.updateMode == InputSettings.UpdateMode.ProcessEventsInDynamicUpdate && Data.inheritsType && typeof(MonoBehaviour).IsAssignableFrom(Data.GetInheritedType()))
                    {
                        foreach (var unit in specialUnits.Where(unit => unit is OnInputSystemEvent).Cast<OnInputSystemEvent>())
                        {
                            if (!unit.trigger.hasValidConnection) continue;
                            method.beforeBody += CodeBuilder.Indent(2) + CodeUtility.MakeSelectable(unit, MethodNodeGenerator.GetSingleDecorator<MethodNodeGenerator>(unit, unit).Name + "();") + "\n";
                        }
                    }
#endif
                    @class.AddMethod(method);
                }
#if PACKAGE_INPUT_SYSTEM_EXISTS
                if (UnityEngine.InputSystem.InputSystem.settings.updateMode != InputSettings.UpdateMode.ProcessEventsInDynamicUpdate && Data.inheritsType && typeof(MonoBehaviour).IsAssignableFrom(Data.GetInheritedType()))
                {
                    if (@class.methods.Any(m => m.name.Replace(" ", "") == "FixedUpdate"))
                    {
                        var method = @class.methods.First(m => m.name.Replace(" ", "") == "FixedUpdate");
                        foreach (var unit in specialUnits.Where(unit => unit is OnInputSystemEvent).Cast<OnInputSystemEvent>())
                        {
                            if (!unit.trigger.hasValidConnection) continue;
                            method.beforeBody += CodeBuilder.Indent(2) + CodeUtility.MakeSelectable(unit, unit.GetMethodGenerator().Name + "();") + "\n";
                        }
                    }
                    else
                    {
                        var method = MethodGenerator.Method(AccessModifier.None, MethodModifier.None, typeof(void), "FixedUpdate");
                        foreach (var unit in specialUnits.Where(unit => unit is OnInputSystemEvent).Cast<OnInputSystemEvent>())
                        {
                            if (!unit.trigger.hasValidConnection) continue;
                            method.beforeBody += CodeBuilder.Indent(2) + CodeUtility.MakeSelectable(unit, MethodNodeGenerator.GetSingleDecorator<MethodNodeGenerator>(unit, unit).Name + "();") + "\n";
                        }
                        @class.AddMethod(method);
                    }
                }
            }
#endif

            @namespace.AddClass(@class);
            return @class;
        }

        private List<string> ProcessGraphUnits(FlowGraph graph, ClassGenerator @class)
        {
            var usings = new List<string>();
            foreach (var _unit in graph.GetUnitsRecursive(Recursion.New(Recursion.defaultMaxDepth)).Cast<Unit>())
            {
                var generator = NodeGenerator.GetSingleDecorator(_unit, _unit);
                HandleOtherGenerators(@class, generator);
                if (specialUnitTypes.Contains(_unit.GetType()))
                {
                    specialUnits.Add(_unit);
                }

                AddNamespacesToUsings(generator, usings);
            }
            return usings;
        }

        private void ProcessProperty(ClassFieldDeclaration variableData, ClassGenerator @class)
        {
            var property = PropertyGenerator.Property(variableData.scope, variableData.propertyModifier, variableData.type, variableData.name, variableData.defaultValue != null, variableData.getterScope, variableData.setterScope);
            property.Default(variableData.defaultValue);
            AddAttributesToProperty(property, variableData.attributes);

            // Handle getter
            if (variableData.get)
            {
                var generationData = CreateGenerationData(variableData.type);
                @class.AddUsings(ProcessGraphUnits(variableData.getter.graph, @class));
                property.MultiStatementGetter(variableData.getterScope, (variableData.getter.graph.units[0] as Unit).GenerateControl(null, generationData, 0));
            }

            // Handle setter
            if (variableData.set)
            {
                @class.AddUsings(ProcessGraphUnits(variableData.setter.graph, @class));
                var generationData = CreateGenerationData(typeof(void));
                property.MultiStatementSetter(variableData.setterScope, (variableData.setter.graph.units[0] as Unit).GenerateControl(null, generationData, 0));
            }

            @class.AddProperty(property);
        }

        private void ProcessField(ClassFieldDeclaration variableData, ClassGenerator @class)
        {
            var field = FieldGenerator.Field(variableData.scope, variableData.fieldModifier, variableData.type, variableData.name, variableData.defaultValue);
            AddAttributesToField(field, variableData.attributes);
            @class.AddField(field);
        }

        private void AddAttributesToProperty(PropertyGenerator property, List<AttributeDeclaration> attributes)
        {
            foreach (var attribute in attributes)
            {
                var attrGenerator = AttributeGenerator.Attribute(attribute.GetAttributeType());
                AddParametersToAttribute(attrGenerator, attribute.parameters);
                property.AddAttribute(attrGenerator);
            }
        }

        private void AddAttributesToField(FieldGenerator field, List<AttributeDeclaration> attributes)
        {
            foreach (var attribute in attributes)
            {
                var attrGenerator = AttributeGenerator.Attribute(attribute.GetAttributeType());
                AddParametersToAttribute(attrGenerator, attribute.parameters);
                field.AddAttribute(attrGenerator);
            }
        }

        private void AddParametersToAttribute(AttributeGenerator attrGenerator, List<TypeParam> parameters)
        {
            foreach (var param in parameters)
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
                else if (!attrGenerator.parameterValues.Contains(param))
                {
                    attrGenerator.AddParameter(param.defaultValue);
                }
            }
        }

        private ParameterGenerator CreateParameter(TypeParam parameter)
        {
            return ParameterGenerator.Parameter(
                parameter.name,
                parameter.type,
                parameter.modifier,
                parameter.attributes,
                parameter.hasDefault,
                parameter.isParamsParameter,
                parameter.defaultValue
            );
        }

        private ControlGenerationData CreateGenerationData(Type returnType)
        {
            var generationData = new ControlGenerationData();
            if (Data.inheritsType)
            {
                generationData.ScriptType = Data.GetInheritedType();
            }
            generationData.returns = returnType;
            foreach (var variable in Data.variables)
            {
                if (!string.IsNullOrEmpty(variable.FieldName))
                    generationData.AddLocalNameInScope(variable.FieldName, variable.type);
            }
            return generationData;
        }

        private void AddNamespacesToUsings(NodeGenerator generator, List<string> usings)
        {
            if (!string.IsNullOrEmpty(generator.NameSpaces))
            {
                usings.Add(generator.NameSpaces);
            }
        }

        private void AddMethodAttributes(MethodGenerator method, ClassMethodDeclaration methodData)
        {
            foreach (var attribute in methodData.attributes)
            {
                var attrGenerator = AttributeGenerator.Attribute(attribute.GetAttributeType());
                foreach (var param in attribute.parameters)
                {
                    attrGenerator.AddParameter(param.defaultValue);
                }
                method.AddAttribute(attrGenerator);
            }
        }

        private void HandleOtherGenerators(ClassGenerator @class, NodeGenerator generator)
        {
            if (generator is VariableNodeGenerator variableGenerator)
            {
                var existingFields = new HashSet<string>(@class.fields.Select(f => f.name));
                variableGenerator.count = 0;

                while (existingFields.Contains(variableGenerator.Name))
                {
                    variableGenerator.count++;
                }

                @class.AddField(FieldGenerator.Field(variableGenerator.AccessModifier, variableGenerator.FieldModifier, variableGenerator.Type, variableGenerator.Name));
            }
            else if (generator is MethodNodeGenerator methodGenerator && methodGenerator.unit is not IEventUnit)
            {
                var existingMethods = new HashSet<string>(@class.methods.Select(m => m.name));
                methodGenerator.count = 0;

                while (existingMethods.Contains(methodGenerator.Name))
                {
                    methodGenerator.count++;
                }
                var data = new ControlGenerationData() { ScriptType = Data.GetInheritedType() };
                foreach (var item in @class.fields)
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
                        var genericString = method.generics[param.generic];
                        method.AddParameter(ParameterGenerator.Parameter(param.name, genericString, param.type, param.modifier));
                    }
                }

                foreach (var variable in Data.variables)
                {
                    methodGenerator.Data.AddLocalNameInScope(variable.FieldName, variable.type);
                }
                methodGenerator.Data = data;
                method.Body(string.IsNullOrEmpty(methodGenerator.MethodBody) ? methodGenerator.GenerateControl(methodGenerator.unit.controlInputs.Count == 0 ? null : methodGenerator.unit.controlInputs[0], data, 0) : methodGenerator.MethodBody);
                @class.AddMethod(method);
            }
        }
    }
}