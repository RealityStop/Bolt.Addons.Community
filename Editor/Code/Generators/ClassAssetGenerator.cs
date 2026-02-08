using Unity.VisualScripting.Community.Libraries.CSharp;
using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.Humility;
using System.Reflection;
using Unity.VisualScripting.Community.Utility;
#if PACKAGE_INPUT_SYSTEM_EXISTS
using UnityEngine.InputSystem;
using Unity.VisualScripting.InputSystem;
#endif
namespace Unity.VisualScripting.Community.CSharp
{
    [Serializable]
    [CodeGenerator(typeof(ClassAsset))]
    public sealed class ClassAssetGenerator : MemberTypeAssetGenerator<ClassAsset, ClassFieldDeclaration, ClassMethodDeclaration, ClassConstructorDeclaration>
    {
        /// <summary>
        /// Units that require the Update method in a monobehaviour to function
        /// </summary>
        private static readonly HashSet<Type> _specialUnitTypes = new HashSet<Type> {
            typeof(Timer),
            typeof(Cooldown),
#if PACKAGE_INPUT_SYSTEM_EXISTS
            typeof(OnInputSystemEventButton),
            typeof(OnInputSystemEventVector2),
            typeof(OnInputSystemEventFloat)
#endif
        };

        private readonly HashSet<Unit> _specialUnits = new HashSet<Unit>();

        protected override TypeGenerator OnGenerateType(NamespaceGenerator @namespace)
        {
            _specialUnits.Clear();
            if (Data == null)
                return ClassGenerator.Class(RootAccessModifier.Public, ClassModifier.None, "", null);
            generatorCounts.Clear();
            CreateGenerationData();
            string className = Data.title.LegalMemberName();
            Type baseType = Data.scriptableObject
                ? typeof(ScriptableObject)
                : (Data.inheritsType && Data.inherits.type != null ? Data.GetInheritedType() : typeof(object));

            var @class = ClassGenerator.Class(RootAccessModifier.Public, Data.classModifier, className, baseType);
            @namespace.beforeUsings = "#pragma warning disable".ConstructHighlight();

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

            if (Data.inheritsType && Data.inherits.type != null && Data.classModifier != ClassModifier.Static && Data.classModifier != ClassModifier.StaticPartial)
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
                        if (param.modifier == Libraries.CSharp.ParameterModifier.Params)
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
                foreach (var item in attribute.fields)
                {
                    attrGenerator.AddParameter(item.Key, item.Value);
                }
                @class.AddAttribute(attrGenerator);
            }

            foreach (var @interface in Data.interfaces)
            {
                @class.ImplementInterface(@interface.type);
            }

            bool addedStatic = false;

            foreach (var constructorData in Data.constructors)
            {
                var parameters = constructorData.parameters;

                if (constructorData.graph.units.Count == 0) continue;

                if (addedStatic || @class.constructors.Any(f => f.parameters.Select(param => param.generator.type).SequenceEqual(parameters.Select(param => param.type))))
                {
                    continue;
                }

                var constructor = ConstructorGenerator.Constructor(constructorData.scope, Data.IsStatic() ? ConstructorModifier.Static : constructorData.modifier, constructorData.initializerType, className);

                var unit = constructorData.graph.units[0] as FunctionNode;

                if (unit == null) continue;

                constructor.SetOwner(unit);

                if (Data.IsStatic())
                {
                    addedStatic = true;
                }
                @class.AddUsings(ProcessGraphUnits(constructorData.graph, @class));
                constructor.Body(w =>
                {
                    data.EnterMethod();
                    data.SetGraphPointer(constructorData.GetReference().AsReference());
                    using (data.Expect(typeof(void)))
                        unit.GenerateControl(null, data, w);
                    data.ExitMethod();
                });
                foreach (var param in parameters)
                {
                    param.showInitalizer = true;
                    if (!string.IsNullOrEmpty(param.name))
                    {
                        constructor.AddParameter(param.useInInitializer, CreateParameter(param));
                    }
                }

                @class.AddConstructor(constructor);
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

                    if (methodData.graph.units.Count == 0) continue;
                    var modifier = methodData.modifier;

                    if (Data.IsStatic())
                    {
                        modifier |= MethodModifier.Static;

                        if (CodeConverter.methodModifierConflicts.TryGetValue(MethodModifier.Static, out var conflicts))
                        {
                            foreach (var conflict in conflicts)
                            {
                                modifier &= ~conflict;
                            }
                        }
                    }

                    var method = MethodGenerator.Method(methodData.scope, modifier, methodData.returnType, methodData.name);
                    var unit = methodData.functionNode;

                    method.SetOwner(unit);

                    method.AddGenerics(methodData.genericParameters.ToArray());
                    AddMethodAttributes(method, methodData);

                    @class.AddUsings(ProcessGraphUnits(methodData.graph, @class));
                    method.Body(w =>
                    {
                        data.EnterMethod();
                        data.SetReturns(methodData.returnType);
                        data.SetGraphPointer(methodData.GetReference().AsReference());

                        unit.GenerateControl(null, data, w);

                        if (data.MustReturn && !data.HasReturned) method.SetWarning("Not all code paths return a value");
                        data.ExitMethod();
                    });

                    foreach (var param in methodData.parameters)
                    {
                        if (!string.IsNullOrEmpty(param.name))
                        {
                            method.AddParameter(CreateParameter(param));
                        }
                    }

                    @class.AddMethod(method);
                }
            }

            if (_specialUnits.Count > 0)
            {
                data.EnterMethod();
                bool addedSpecialUpdatedCode = false;
#if PACKAGE_INPUT_SYSTEM_EXISTS
                bool addedSpecialFixedUpdatedCode = false;
#endif
                HashSet<Unit> visited = new HashSet<Unit>();
                if (!addedSpecialUpdatedCode && @class.methods.Any(m => m.name.Replace(" ", "") == "Update"))
                {
                    addedSpecialUpdatedCode = true;
                    var method = @class.methods.First(m => m.name.Replace(" ", "") == "Update");
                    if (Data.inheritsType && typeof(MonoBehaviour).IsAssignableFrom(Data.GetInheritedType()))
                    {
                        if (method.HasBody())
                        {
                            method.beforeBodyAction += w =>
                            {
                                var index = 0;
                                foreach (var unit in _specialUnits)
                                {
                                    using (w.BeginNode(unit))
                                    {
                                        w.WriteLine((unit.GetGenerator() as VariableNodeGenerator)?.Name.VariableHighlight() + ".Update();");
                                    }
                                    index++;
                                }
                            };
                        }
                        else
                            method.AddToBody(w =>
                            {
                                var index = 0;
                                foreach (var unit in _specialUnits)
                                {
                                    using (w.BeginNode(unit))
                                    {
                                        w.WriteLine((unit.GetGenerator() as VariableNodeGenerator)?.Name.VariableHighlight() + ".Update();");
                                    }
                                    index++;
                                }
                            });
                    }
#if PACKAGE_INPUT_SYSTEM_EXISTS
                    if (UnityEngine.InputSystem.InputSystem.settings.updateMode == InputSettings.UpdateMode.ProcessEventsInDynamicUpdate && Data.inheritsType && typeof(MonoBehaviour).IsAssignableFrom(Data.GetInheritedType()))
                    {
                        foreach (var unit in _specialUnits.Where(unit => unit is OnInputSystemEvent).Cast<OnInputSystemEvent>())
                        {
                            if (!unit.trigger.hasValidConnection) continue;
                            method.beforeBodyAction += w =>
                            {
                                using (w.BeginNode(unit))
                                {
                                    w.WriteLine(MethodNodeGenerator.GetSingleDecorator<MethodNodeGenerator>(unit, unit).Name + "();");
                                }
                            };
                        }
                    }
#endif
                }
                else if (!addedSpecialUpdatedCode)
                {
                    addedSpecialUpdatedCode = true;
                    var method = MethodGenerator.Method(AccessModifier.None, MethodModifier.None, typeof(void), "Update");
                    if (Data.inheritsType && typeof(MonoBehaviour).IsAssignableFrom(Data.GetInheritedType()))
                    {
                        method.AddToBody(w =>
                        {
                            var index = 0;
                            foreach (var unit in _specialUnits)
                            {
                                using (w.BeginNode(unit))
                                {
                                    w.WriteLine((unit.GetGenerator() as VariableNodeGenerator)?.Name.VariableHighlight() + ".Update();");
                                }
                                index++;
                            }
                        });
                    }

#if PACKAGE_INPUT_SYSTEM_EXISTS
                    if (UnityEngine.InputSystem.InputSystem.settings.updateMode == InputSettings.UpdateMode.ProcessEventsInDynamicUpdate && Data.inheritsType && typeof(MonoBehaviour).IsAssignableFrom(Data.GetInheritedType()))
                    {
                        foreach (var unit in _specialUnits.Where(unit => unit is OnInputSystemEvent).Cast<OnInputSystemEvent>())
                        {
                            if (!unit.trigger.hasValidConnection) continue;
                            method.AddToBody(w =>
                            {
                                using (w.BeginNode(unit))
                                {
                                    w.WriteLine(MethodNodeGenerator.GetSingleDecorator<MethodNodeGenerator>(unit, unit).Name + "();");
                                }
                            });
                        }
                    }
#endif
                    @class.AddMethod(method);
                }
#if PACKAGE_INPUT_SYSTEM_EXISTS
                if (UnityEngine.InputSystem.InputSystem.settings.updateMode != InputSettings.UpdateMode.ProcessEventsInDynamicUpdate && Data.inheritsType && typeof(MonoBehaviour).IsAssignableFrom(Data.GetInheritedType()))
                {
                    if (!addedSpecialFixedUpdatedCode && @class.methods.Any(m => m.name.Replace(" ", "") == "FixedUpdate"))
                    {
                        addedSpecialFixedUpdatedCode = true;
                        var method = @class.methods.First(m => m.name.Replace(" ", "") == "FixedUpdate");
                        foreach (var unit in _specialUnits.Where(unit => unit is OnInputSystemEvent).Cast<OnInputSystemEvent>())
                        {
                            if (!unit.trigger.hasValidConnection) continue;
                            method.beforeBodyAction += w =>
                            {
                                using (w.BeginNode(unit))
                                {
                                    w.WriteLine(unit.GetMethodGenerator()?.Name + "();");
                                }
                            };
                        }
                    }
                    else if (!addedSpecialFixedUpdatedCode)
                    {
                        addedSpecialFixedUpdatedCode = true;
                        var method = MethodGenerator.Method(AccessModifier.None, MethodModifier.None, typeof(void), "FixedUpdate");
                        foreach (var unit in _specialUnits.Where(unit => unit is OnInputSystemEvent).Cast<OnInputSystemEvent>())
                        {
                            if (!unit.trigger.hasValidConnection) continue;
                            method.beforeBodyAction += w =>
                            {
                                using (w.BeginNode(unit))
                                {
                                    w.WriteLine(MethodNodeGenerator.GetSingleDecorator<MethodNodeGenerator>(unit, unit)?.Name + "();");
                                }
                            };
                        }
                        @class.AddMethod(method);
                    }
                }
#endif
                data.ExitMethod();
            }
            var values = CodeGeneratorValueUtility.GetAllValues(Data, true);
            var index = 0;
            foreach (var variable in values)
            {
                var field = FieldGenerator.Field(AccessModifier.Public, FieldModifier.None, variable.Value != null ? variable.Value.GetType() : typeof(UnityEngine.Object), variable.Key.LegalMemberName());

                var attribute = AttributeGenerator.Attribute(typeof(FoldoutAttribute));
                attribute.AddParameter("ObjectReferences");
                field.AddAttribute(attribute);

                @class.AddField(field);
                index++;
            }

            @namespace.AddClass(@class);

            return @class;
        }

        private HashSet<string> ProcessGraphUnits(FlowGraph graph, ClassGenerator @class)
        {
            HashSet<string> usings = new HashSet<string>();
            GraphTraversal.TraverseFlowGraph(graph, (unit) =>
            {
                var type = unit.GetType();
                var generator = unit.GetGenerator();

                if (generator is IRequireVariables variables)
                {
                    foreach (var variable in variables.GetRequiredVariables(data))
                    {
                        variable.SetOwner(unit);
                        @class.AddField(variable);
                    }
                }

                if (generator is IRequireMethods methods)
                {
                    foreach (var method in methods.GetRequiredMethods(data))
                    {
                        method.SetOwner(unit);
                        @class.AddMethod(method);
                    }
                }

                HandleOtherGenerators(@class, generator);

                if (_specialUnitTypes.Contains(type))
                {
                    _specialUnits.Add(unit);
                }

                if (generator is InterfaceNodeGenerator interfaceNodeGenerator)
                {
                    foreach (var interfaceType in interfaceNodeGenerator.InterfaceTypes)
                    {
                        @class.ImplementInterface(interfaceType);
                    }
                }

                foreach (var @namespace in generator.GetNamespaces())
                {
                    usings.Add(@namespace.Trim());
                }
            });
            return usings;
        }

        private void ProcessProperty(ClassFieldDeclaration variableData, ClassGenerator @class)
        {
            var modifier = variableData.propertyModifier;

            if (Data.IsStatic())
            {
                modifier |= PropertyModifier.Static;

                if (CodeConverter.propertyModifierConflicts.TryGetValue(PropertyModifier.Static, out var conflicts))
                {
                    foreach (var conflict in conflicts)
                    {
                        modifier &= ~conflict;
                    }
                }
            }

            var property = PropertyGenerator.Property(variableData.scope, modifier, variableData.type, variableData.name, variableData.defaultValue != null && variableData.hasDefault, variableData.defaultValue, variableData.getterScope, variableData.setterScope);

            AddAttributesToProperty(property, variableData.attributes);

            // Handle getter
            if (variableData.get)
            {
                @class.AddUsings(ProcessGraphUnits(variableData.getter.graph, @class));

                var unit = variableData.getter.graph.units[0] as FunctionNode;

                property.SetGetterOwner(unit);

                property.MultiStatementGetter(variableData.getterScope, GetterMethod);

                void GetterMethod(CodeWriter w)
                {
                    data.EnterMethod();
                    data.SetReturns(variableData.type);
                    data.SetGraphPointer(variableData.getter.GetReference().AsReference());

                    unit.GenerateControl(null, data, w);

                    if (data.MustReturn && !data.HasReturned) property.SetWarning("Not all code paths return a value");
                    data.ExitMethod();
                }
            }

            // Handle setter
            if (variableData.set)
            {
                @class.AddUsings(ProcessGraphUnits(variableData.setter.graph, @class));

                var unit = variableData.setter.graph.units[0] as FunctionNode;

                property.SetSetterOwner(unit);

                property.MultiStatementSetter(variableData.setterScope, SetterMethod);

                void SetterMethod(CodeWriter w)
                {
                    data.EnterMethod();
                    data.SetReturns(variableData.type);
                    data.SetGraphPointer(variableData.setter.GetReference().AsReference());

                    unit.GenerateControl(null, data, w);

                    data.ExitMethod();
                }
            }

            @class.AddProperty(property);
        }

        private void ProcessField(ClassFieldDeclaration variableData, ClassGenerator @class)
        {
            var modifier = variableData.fieldModifier;

            if (Data.IsStatic())
            {
                modifier |= FieldModifier.Static;

                if (CodeConverter.fieldModifierConflicts.TryGetValue(FieldModifier.Static, out var conflicts))
                {
                    foreach (var conflict in conflicts)
                    {
                        modifier &= ~conflict;
                    }
                }
            }

            var field = FieldGenerator.Field(variableData.scope, modifier, variableData.type, variableData.name, variableData.defaultValue, variableData.hasDefault);

            AddAttributesToField(field, variableData.attributes);
            @class.AddField(field);
        }

        private void AddAttributesToProperty(PropertyGenerator property, List<AttributeDeclaration> attributes)
        {
            foreach (var attribute in attributes)
            {
                var attrGenerator = AttributeGenerator.Attribute(attribute.GetAttributeType());
                AddParametersToAttribute(attrGenerator, attribute.parameters);
                AddFieldParametersToAttribute(attrGenerator, attribute.fields);
                property.AddAttribute(attrGenerator);
            }
        }

        private void AddAttributesToField(FieldGenerator field, List<AttributeDeclaration> attributes)
        {
            foreach (var attribute in attributes)
            {
                var attrGenerator = AttributeGenerator.Attribute(attribute.GetAttributeType());
                AddParametersToAttribute(attrGenerator, attribute.parameters);
                AddFieldParametersToAttribute(attrGenerator, attribute.fields);
                field.AddAttribute(attrGenerator);
            }
        }

        private void AddFieldParametersToAttribute(AttributeGenerator attrGenerator, Dictionary<string, object> fields)
        {
            foreach (var item in fields)
            {
                attrGenerator.AddParameter(item.Key, item.Value);
            }
        }

        private void AddParametersToAttribute(AttributeGenerator attrGenerator, List<TypeParam> parameters)
        {
            foreach (var param in parameters)
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
                parameter.defaultValue
            );
        }

        public override ControlGenerationData CreateGenerationData()
        {
            data = new ControlGenerationData(Data.inheritsType ? Data.GetInheritedType() : typeof(object), null);
            return data;
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
                AddFieldParametersToAttribute(attrGenerator, attribute.fields);
                method.AddAttribute(attrGenerator);
            }
        }
        Dictionary<Type, int> generatorCounts = new Dictionary<Type, int>();
        private void HandleOtherGenerators(ClassGenerator @class, NodeGenerator generator)
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
                var field = FieldGenerator.Field(variableGenerator.AccessModifier, variableGenerator.FieldModifier, variableGenerator.Type, variableGenerator.Name, variableGenerator.DefaultValue, variableGenerator.HasDefaultValue);

                field.SetOwner(variableGenerator.unit);

                field.SetLiteral(variableGenerator.Literal);
                field.SetNewlineLiteral(variableGenerator.NewLineLiteral);
                field.SetNew(variableGenerator.IsNew);
                @class.AddField(field);
            }
            else if (generator is MethodNodeGenerator methodGenerator && !(methodGenerator.unit is IEventUnit))
            {
                var type = methodGenerator.unit.GetType();
                if (!generatorCounts.ContainsKey(type))
                {
                    generatorCounts[type] = 0;
                }
                methodGenerator.count = generatorCounts[type];
                generatorCounts[type]++;

                var method = MethodGenerator.Method(methodGenerator.AccessModifier, methodGenerator.MethodModifier, methodGenerator.ReturnType, methodGenerator.Name);

                method.SetOwner(methodGenerator.unit);

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

                if (methodGenerator.Attributes.Count > 0)
                {
                    foreach (var attribute in methodGenerator.Attributes)
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
                        foreach (var item in attribute.fields)
                        {
                            attrGenerator.AddParameter(item.Key, item.Value);
                        }
                        method.AddAttribute(attrGenerator);
                    }
                }

                method.Body(Writer);
                void Writer(CodeWriter writer)
                {
                    data.EnterMethod();
                    data.SetReturns(methodGenerator.ReturnType);
                    try
                    {
                        using (writer.BeginNode(methodGenerator.unit))
                        {
                            methodGenerator.GeneratedMethodCode(data, writer);
                        }
                    }
                    catch (NotImplementedException)
                    {
                        methodGenerator.GenerateControl(null, data, writer);
                    }
                    data.ExitMethod();
                }

                @class.AddMethod(method);
            }
        }
    }
}