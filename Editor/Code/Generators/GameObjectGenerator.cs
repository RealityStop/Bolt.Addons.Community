using System;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;
using System.Collections;
#if PACKAGE_INPUT_SYSTEM_EXISTS
using Unity.VisualScripting.InputSystem;
using UnityEngine.InputSystem;
#endif

namespace Unity.VisualScripting.Community
{
    [Serializable]
    [CodeGenerator(typeof(GameObject))]
    public sealed class GameObjectGenerator : CodeGenerator<GameObject>
    {
        #region Static Fields
        private static readonly Dictionary<string, string> EVENT_NAMES = new Dictionary<string, string>()
        {
            { "OnStart", "Start" },
            { "OnUpdate", "Update" },
            { "OnAwake", "Awake" },
            { "OnFixedUpdate", "FixedUpdate" },
            { "OnLateUpdate", "LateUpdate" },
        };

        private static readonly HashSet<Type> UNITY_METHOD_TYPES = new HashSet<Type>
        {
#if MODULE_PHYSICS_EXISTS
            typeof(OnCollisionEnter),
            typeof(OnCollisionExit),
            typeof(OnCollisionStay),
            typeof(OnJointBreak),
            typeof(OnTriggerEnter),
            typeof(OnTriggerExit),
            typeof(OnTriggerStay),
            typeof(OnControllerColliderHit),
            typeof(OnParticleCollision),
#endif
#if MODULE_PHYSICS_2D_EXISTS
            typeof(OnCollisionEnter2D),
            typeof(OnCollisionExit2D),
            typeof(OnCollisionStay2D),
            typeof(OnJointBreak2D),
            typeof(OnTriggerEnter2D),
            typeof(OnTriggerExit2D),
            typeof(OnTriggerStay2D),
#endif
            typeof(OnApplicationFocus),
            typeof(OnApplicationPause),
            typeof(Start),
            typeof(Update),
            typeof(FixedUpdate),
            typeof(LateUpdate),
#if PACKAGE_INPUT_SYSTEM_EXISTS
            typeof(OnInputSystemEventButton),
            typeof(OnInputSystemEventVector2),
            typeof(OnInputSystemEventFloat),
#endif
        };
        #endregion

        #region Private Fields
        private Dictionary<string, GraphMethodDecleration> _methods;
        private Dictionary<CustomEvent, int> _customEventIds;
        private readonly List<Unit> _specialUnits = new List<Unit>();
        #endregion
        public ScriptMachine[] components = new ScriptMachine[0];
        public ScriptMachine current;
        List<Unit> units = new();
        public override string Generate(int indent)
        {
            components = Data?.GetComponents<ScriptMachine>();
            if (components == null || components.Length == 0) return string.Empty;

            if (current == null)
            {
                current = components[0];
            }

            InitializeCollections();
            var script = GenerateScriptHeader();
            script += GenerateClassDefinition();
            script += GenerateVariableDeclarations();
            script += GeneratAwakeRequiredHandlers();
            script += GenerateEventMethods();
            script += GenerateSpecialUnits();
            script += GenerateMethodDeclarations();
            script += "}";

            return script;
        }

        #region Private Methods
        private void InitializeCollections()
        {
            _methods = new Dictionary<string, GraphMethodDecleration>();
            _customEventIds = new Dictionary<CustomEvent, int>();
            _specialUnits.Clear();
            generatorCount.Clear();
            units = current.nest.graph.GetUnitsRecursive(Recursion.New(Recursion.defaultMaxDepth)).Cast<Unit>().ToList();
        }

        private string GenerateScriptHeader()
        {
            var usings = GetRequiredNamespaces();
            return string.Join("\n", usings.Select(u => GenerateUsingStatement(u))) + "\n";
        }
        private Dictionary<Type, int> generatorCount = new Dictionary<Type, int>();
        private List<(string, Unit)> GetRequiredNamespaces()
        {
            var usings = new List<(string, Unit)> { ("Unity", null), ("UnityEngine", null), ("Unity.VisualScripting", null) };
            foreach (Unit unit in units)
            {
                var generator = NodeGenerator.GetSingleDecorator(unit, unit);

                if (unit is Timer timer)
                {
                    if (!generatorCount.ContainsKey(typeof(TimerGenerator)))
                    {
                        generatorCount[typeof(TimerGenerator)] = 0;
                    }
                    _specialUnits.Add(timer);
                    (generator as TimerGenerator).count = generatorCount[typeof(TimerGenerator)];
                    generatorCount[typeof(TimerGenerator)]++;
                }
                else if (unit is Cooldown cooldown)
                {
                    if (!generatorCount.ContainsKey(typeof(CooldownGenerator)))
                    {
                        generatorCount[typeof(CooldownGenerator)] = 0;
                    }
                    _specialUnits.Add(cooldown);
                    (generator as CooldownGenerator).count = generatorCount[typeof(CooldownGenerator)];
                    generatorCount[typeof(CooldownGenerator)]++;
                }
                else if (generator is MethodNodeGenerator methodNodeGenerator)
                {
                    if (!generatorCount.ContainsKey(methodNodeGenerator.GetType()))
                    {
                        generatorCount[methodNodeGenerator.GetType()] = 0;
                    }
                    methodNodeGenerator.count = generatorCount[methodNodeGenerator.GetType()];
                    generatorCount[methodNodeGenerator.GetType()]++;
                }

                if (!string.IsNullOrEmpty(generator.NameSpaces))
                {
                    foreach (var ns in generator.NameSpaces.Split(","))
                    {
                        if (!usings.Any(@using => @using.Item1 == ns))
                        {
                            usings.Add((ns, unit));
                        }
                    }
                }
            }
            return usings;
        }

        private string GenerateUsingStatement((string, Unit) @using)
        {
            return !string.IsNullOrWhiteSpace(@using.Item1)
                ? @using.Item2 != null
                    ? CodeUtility.MakeSelectable(@using.Item2, $"using".ConstructHighlight() + $" {@using.Item1};")
                    : $"using".ConstructHighlight() + $" {@using.Item1};"
                : string.Empty;
        }

        private string GenerateClassDefinition()
        {
            return $"\n" + "public class ".ConstructHighlight()
                + $"{(current.nest.graph.title?.Length > 0 ? current.nest.graph.title : Data.name + "_ScriptMachine_" + components.ToList().IndexOf(current))}".LegalMemberName().TypeHighlight()
                + " : "
                + "MonoBehaviour\n".TypeHighlight()
                + "{";
        }

        private string GenerateVariableDeclarations()
        {
            var script = string.Empty;
            foreach (VariableDeclaration variable in current.nest.graph.variables)
            {
                script +=
                    "\n" + CodeBuilder.Indent(1) + "public ".ConstructHighlight()
                    + Type.GetType(variable.typeHandle.Identification).As().CSharpName(false, true)
                    + " "
                    + variable.name.LegalMemberName().VariableHighlight()
                    + (
                        variable.value != null
                            ? $" = "
                                + ""
                                + $"{variable.value.As().Code(true, true, true, "", true, true, false)};\n"
                            : string.Empty + ";\n"
                    );
            };

            foreach (Unit unit in units)
            {
                var generator = unit.GetGenerator();
                CodeBuilder.Indent(1);
                if (generator is VariableNodeGenerator variableNodeGenerator)
                {
                    script +=
                        "\n" + CodeBuilder.Indent(1) + CodeUtility.MakeSelectable(unit, variableNodeGenerator.AccessModifier.AsString().ConstructHighlight() + " "
                        + variableNodeGenerator.Type.As().CSharpName(false, true) + " " + variableNodeGenerator.FieldModifier.AsString().ConstructHighlight()
                        + variableNodeGenerator.Name.VariableHighlight()
                        + (variableNodeGenerator.HasDefaultValue ? $" = " : "")) + (variableNodeGenerator.HasDefaultValue ? variableNodeGenerator.DefaultValue.As().Code(variableNodeGenerator.IsNew, variableNodeGenerator.unit, variableNodeGenerator.Literal, true, "", variableNodeGenerator.NewLineLiteral, true, false) : "") + CodeUtility.MakeSelectable(unit, ";") + "\n";
                }
#if PACKAGE_INPUT_SYSTEM_EXISTS && !PACKAGE_INPUT_SYSTEM_1_2_0_OR_NEWER_EXISTS
                else if (unit is OnInputSystemEvent eventUnit && generator is MethodNodeGenerator methodNodeGenerator)
                {
                    if (!eventUnit.trigger.hasValidConnection) continue;
                    script +=
                        "\n" + CodeBuilder.Indent(1) + CodeUtility.MakeSelectable(unit, "private".ConstructHighlight() + " "
                        + "bool".ConstructHighlight() + " "
                        + GetInputSystemEventVariableName(unit as OnInputSystemEvent, methodNodeGenerator).VariableHighlight());
                }
#endif
            }
            return script;
        }

#if PACKAGE_INPUT_SYSTEM_EXISTS && !PACKAGE_INPUT_SYSTEM_1_2_0_OR_NEWER_EXISTS
        private string GetInputSystemEventVariableName(OnInputSystemEvent onInputSystemEvent, MethodNodeGenerator methodNodeGenerator)
        {
            if (onInputSystemEvent is OnInputSystemEventButton)
            {
                return $"button{methodNodeGenerator.count}_wasRunning";
            }
            else if (onInputSystemEvent is OnInputSystemEventFloat)
            {
                return $"float{methodNodeGenerator.count}_wasRunning";
            }
            else
            {
                return $"vector2{methodNodeGenerator.count}_wasRunning";
            }
        }
#endif

        private string GeneratAwakeRequiredHandlers()
        {
            var script = string.Empty;
            if (current.nest.graph.units.Any(unit => unit is CustomEvent))
            {
                var customEvents = current.nest.graph.units.Where(unit => unit is CustomEvent);
                script += $"\n" + CodeBuilder.Indent(1) + "private void".ConstructHighlight() + " Awake()";
                script += "\n" + CodeBuilder.Indent(1) + "{\n";
                int id = 0;
                foreach (CustomEvent eventUnit in customEvents)
                {
                    var data = new ControlGenerationData();
                    data.ScriptType = typeof(MonoBehaviour);
                    data.returns = eventUnit.coroutine ? typeof(IEnumerator) : typeof(void);
                    data.AddLocalNameInScope("args", typeof(CustomEventArgs));
                    foreach (VariableDeclaration variable in current.nest.graph.variables)
                    {
                        data.AddLocalNameInScope(variable.name, !string.IsNullOrEmpty(variable.typeHandle.Identification) ? Type.GetType(variable.typeHandle.Identification) : typeof(object));
                    };
                    _customEventIds.Add(eventUnit, id);
                    id++;
                    script += CodeUtility.MakeSelectable(eventUnit, $"{CodeBuilder.Indent(2)}{"CSharpUtility".TypeHighlight()}.RegisterCustomEvent(") + eventUnit.GenerateValue(eventUnit.target) + CodeUtility.MakeSelectable(eventUnit, $", ") + GetMethodName(eventUnit) + CodeUtility.MakeSelectable(eventUnit, "Runner);");

                    script += "\n";

                    var eventName = GetMethodName(eventUnit) + CodeUtility.MakeSelectable(eventUnit, "Runner");
                    string runnerCode = GetCustomEventRunnerCode(eventUnit, data);
                    AddNewMethod(eventUnit, eventName, GetMethodSignature(eventUnit, false, eventName, AccessModifier.Private), runnerCode, "CustomEventArgs ".TypeHighlight() + "args".VariableHighlight(), data);
                }
                script += $"\n{CodeBuilder.Indent(1)}\n";
            }
            return script;
        }

        private string GenerateEventMethods()
        {
            var script = string.Empty;
            bool addedSpecialUpdateCode = false;
            bool addedSpecialFixedUpdateCode = false;
            foreach (IEventUnit unit in units.Where(unit => unit is IEventUnit).Cast<IEventUnit>())
            {
                if (unit is CustomEvent && unit.graph != current.nest.graph) continue;
                var specialUnitCode = "";
                string methodName = CodeUtility.CleanCode(GetMethodName(unit));
                if (unit.coroutine)
                {
                    if (!_methods.ContainsKey(methodName))
                    {
                        var data = new ControlGenerationData();
                        data.ScriptType = typeof(MonoBehaviour);
                        data.returns = typeof(IEnumerator);
                        foreach (VariableDeclaration variable in current.nest.graph.variables)
                        {
                            data.AddLocalNameInScope(variable.name, !string.IsNullOrEmpty(variable.typeHandle.Identification) ? Type.GetType(variable.typeHandle.Identification) : typeof(object));
                        }

                        var parameters = GetMethodParameters(unit);
                        data.AddLocalNameInScope(parameters.paramInfo.parameterName, parameters.paramInfo.parameterType);

                        if (unit.controlOutputs.Any(output => output.key == "trigger"))
                        {
                            if (unit is Update update && !addedSpecialUpdateCode)
                            {
                                addedSpecialUpdateCode = true;
                                specialUnitCode = string.Join("\n", _specialUnits.Select(t => CodeBuilder.Indent(2) + CodeUtility.MakeSelectable(t, (NodeGenerator.GetSingleDecorator(t, t) as VariableNodeGenerator).Name.VariableHighlight() + ".Update();")));
                                specialUnitCode += "\n";
#if PACKAGE_INPUT_SYSTEM_EXISTS
                                if (UnityEngine.InputSystem.InputSystem.settings.updateMode == InputSettings.UpdateMode.ProcessEventsInDynamicUpdate)
                                {
                                    foreach (var inputsystemUnit in units.Where(unit => unit is OnInputSystemEvent).Cast<OnInputSystemEvent>())
                                    {
                                        if (!inputsystemUnit.trigger.hasValidConnection) continue;
                                        specialUnitCode += CodeBuilder.Indent(2) + CodeUtility.MakeSelectable(inputsystemUnit, MethodNodeGenerator.GetSingleDecorator<MethodNodeGenerator>(inputsystemUnit, inputsystemUnit).Name + "();") + "\n";
                                    }
                                }
#endif
                            }
#if PACKAGE_INPUT_SYSTEM_EXISTS
                            else if (!addedSpecialFixedUpdateCode && unit is FixedUpdate && UnityEngine.InputSystem.InputSystem.settings.updateMode != InputSettings.UpdateMode.ProcessEventsInDynamicUpdate)
                            {
                                addedSpecialFixedUpdateCode = true;
                                foreach (var inputsystemUnit in units.Where(unit => unit is OnInputSystemEvent).Cast<OnInputSystemEvent>())
                                {
                                    if (!inputsystemUnit.trigger.hasValidConnection) continue;
                                    specialUnitCode += CodeBuilder.Indent(2) + CodeUtility.MakeSelectable(inputsystemUnit, MethodNodeGenerator.GetSingleDecorator<MethodNodeGenerator>(inputsystemUnit, inputsystemUnit).Name + "();") + "\n";
                                }
                            }
#endif
                            if (UNITY_METHOD_TYPES.Contains(unit.GetType()))
                            {
                                if (unit.controlOutputs["trigger"].hasValidConnection)
                                {
                                    AddNewMethod(unit as Unit, GetMethodName(unit), GetMethodSignature(unit, false), specialUnitCode + CodeBuilder.Indent(2) + CodeUtility.MakeSelectable(unit as Unit, $"StartCoroutine(") + GetMethodName(unit) + CodeUtility.MakeSelectable(unit as Unit, "_Coroutine());"), parameters.parameterSignature, data);
                                    AddNewMethod(unit as Unit, GetMethodName(unit) + CodeUtility.MakeSelectable(unit as Unit, "_Coroutine"), GetMethodSignature(unit, GetMethodName(unit) + CodeUtility.MakeSelectable(unit as Unit, "_Coroutine")), GetMethodBody(unit, data), parameters.parameterSignature, data);
                                }
                            }
                            else if (unit.controlOutputs.First(output => output.key == "trigger").hasValidConnection)
                            {
                                AddNewMethod(unit as Unit, GetMethodName(unit), GetMethodSignature(unit), GetMethodBody(unit, data), parameters.parameterSignature, data);
                            }
                        }
                    }
                    else if (_methods.TryGetValue(methodName, out var method))
                    {
                        method.methodBody += "\n" + GetMethodBody(unit, method.generationData);
                    }
                }
                else
                {
                    if (!_methods.ContainsKey(methodName))
                    {
                        var data = new ControlGenerationData();
                        data.ScriptType = typeof(MonoBehaviour);
                        data.returns = typeof(void);
                        foreach (VariableDeclaration variable in current.nest.graph.variables)
                        {
                            data.AddLocalNameInScope(variable.name, !string.IsNullOrEmpty(variable.typeHandle.Identification) ? Type.GetType(variable.typeHandle.Identification) : typeof(object));
                        };
                        var parameters = GetMethodParameters(unit);
                        data.AddLocalNameInScope(parameters.paramInfo.parameterName, parameters.paramInfo.parameterType);
                        if (unit is Update update && !addedSpecialUpdateCode)
                        {
                            addedSpecialUpdateCode = true;
                            specialUnitCode = string.Join("\n", _specialUnits.Select(t => CodeBuilder.Indent(2) + CodeUtility.MakeSelectable(t, (NodeGenerator.GetSingleDecorator(t, t) as VariableNodeGenerator).Name.VariableHighlight() + ".Update();")));
                            specialUnitCode += "\n";
#if PACKAGE_INPUT_SYSTEM_EXISTS
                            if (UnityEngine.InputSystem.InputSystem.settings.updateMode == InputSettings.UpdateMode.ProcessEventsInDynamicUpdate)
                            {
                                foreach (var inputsystemUnit in units.Where(unit => unit is OnInputSystemEvent).Cast<OnInputSystemEvent>())
                                {
                                    if (!inputsystemUnit.trigger.hasValidConnection) continue;
                                    specialUnitCode += CodeBuilder.Indent(2) + CodeUtility.MakeSelectable(inputsystemUnit, MethodNodeGenerator.GetSingleDecorator<MethodNodeGenerator>(inputsystemUnit, inputsystemUnit).Name + "();") + "\n";
                                }
                            }
#endif
                        }
#if PACKAGE_INPUT_SYSTEM_EXISTS
                        else if (!addedSpecialFixedUpdateCode && unit is FixedUpdate && UnityEngine.InputSystem.InputSystem.settings.updateMode != InputSettings.UpdateMode.ProcessEventsInDynamicUpdate)
                        {
                            addedSpecialFixedUpdateCode = true;
                            foreach (var inputsystemUnit in units.Where(unit => unit is OnInputSystemEvent).Cast<OnInputSystemEvent>())
                            {
                                if (!inputsystemUnit.trigger.hasValidConnection) continue;
                                specialUnitCode += CodeBuilder.Indent(2) + CodeUtility.MakeSelectable(inputsystemUnit, MethodNodeGenerator.GetSingleDecorator<MethodNodeGenerator>(inputsystemUnit, inputsystemUnit).Name + "();") + "\n";
                            }
                        }
#endif
                        if (unit.controlOutputs.Any(output => output.key == "trigger"))
                        {
                            if (unit.controlOutputs.First(output => output.key == "trigger").hasValidConnection) AddNewMethod(unit as Unit, GetMethodName(unit), GetMethodSignature(unit), specialUnitCode + GetMethodBody(unit, data), parameters.parameterSignature, data);
                        }
                        else
                        {
                            if (unit.controlOutputs.Count > 0 && unit.controlOutputs[0].hasValidConnection) AddNewMethod(unit as Unit, GetMethodName(unit), GetMethodSignature(unit), GetMethodBody(unit, data), parameters.parameterSignature, data);
                        }
                    }
                    else if (_methods.TryGetValue(methodName, out var method))
                    {
                        method.methodBody += GetMethodBody(unit, method.generationData) + "\n";
                    }
                }
            }
            return script;
        }

        private string GenerateSpecialUnits()
        {
            var script = string.Empty;
            if (_specialUnits.Count > 0 && !units.Any(e => e is Update))
            {
                var update = new Update();
                var data = new ControlGenerationData
                {
                    ScriptType = typeof(MonoBehaviour)
                };
                var specialCode = string.Join("\n", _specialUnits.Select(t => CodeBuilder.Indent(2) + CodeUtility.MakeSelectable(t, (NodeGenerator.GetSingleDecorator(t, t) as VariableNodeGenerator).Name.VariableHighlight() + ".Update();")));
#if PACKAGE_INPUT_SYSTEM_EXISTS
                if (UnityEngine.InputSystem.InputSystem.settings.updateMode == InputSettings.UpdateMode.ProcessEventsInDynamicUpdate)
                {
                    foreach (var unit in units.Where(unit => unit is OnInputSystemEvent).Cast<OnInputSystemEvent>())
                    {
                        if (!unit.trigger.hasValidConnection) continue;
                        specialCode += CodeBuilder.Indent(2) + CodeUtility.MakeSelectable(unit, MethodNodeGenerator.GetSingleDecorator<MethodNodeGenerator>(unit, unit).Name + "();") + "\n";
                    }
                }
#endif
                AddNewMethod(update, GetMethodName(update), GetMethodSignature(update), specialCode, "", data);
            }
#if PACKAGE_INPUT_SYSTEM_EXISTS
            else if (UnityEngine.InputSystem.InputSystem.settings.updateMode == InputSettings.UpdateMode.ProcessEventsInDynamicUpdate && !units.Any(e => e is Update) && units.Any(unit => unit is OnInputSystemEvent onInputSystemEvent && onInputSystemEvent.trigger.hasValidConnection))
            {
                var update = new Update();
                var data = new ControlGenerationData
                {
                    ScriptType = typeof(MonoBehaviour)
                };
                var specialCode = "";
                foreach (var unit in units.Where(unit => unit is OnInputSystemEvent).Cast<OnInputSystemEvent>())
                {
                    if (!unit.trigger.hasValidConnection) continue;
                    specialCode += CodeBuilder.Indent(2) + CodeUtility.MakeSelectable(unit, MethodNodeGenerator.GetSingleDecorator<MethodNodeGenerator>(unit, unit).Name + "();") + "\n";
                }
                AddNewMethod(update, GetMethodName(update), GetMethodSignature(update), specialCode, "", data);
            }
            else if (UnityEngine.InputSystem.InputSystem.settings.updateMode != InputSettings.UpdateMode.ProcessEventsInDynamicUpdate && !units.Any(e => e is FixedUpdate) && units.Any(unit => unit is OnInputSystemEvent onInputSystemEvent && onInputSystemEvent.trigger.hasValidConnection))
            {
                var update = new FixedUpdate();
                var data = new ControlGenerationData
                {
                    ScriptType = typeof(MonoBehaviour)
                };
                var specialCode = "";
                foreach (var unit in units.Where(unit => unit is OnInputSystemEvent).Cast<OnInputSystemEvent>())
                {
                    if (!unit.trigger.hasValidConnection) continue;
                    specialCode += CodeBuilder.Indent(2) + CodeUtility.MakeSelectable(unit, MethodNodeGenerator.GetSingleDecorator<MethodNodeGenerator>(unit, unit).Name + "();") + "\n";
                }
                AddNewMethod(update, GetMethodName(update), GetMethodSignature(update), specialCode, "", data);
            }
#endif
            return script;
        }

        private string GenerateMethodDeclarations()
        {
            var script = string.Empty;
            foreach (var method in _methods.Values)
            {
                script += method.GetMethod() + "\n";
            }
            return script;
        }

        private string GetCustomEventRunnerCode(CustomEvent eventUnit, ControlGenerationData data)
        {
            var output = "";
            output += CodeBuilder.Indent(2) + CodeUtility.MakeSelectable(eventUnit, "if ".ControlHighlight() + $"({"args".VariableHighlight()}.name == ") + eventUnit.GenerateValue(eventUnit.name, data) + CodeUtility.MakeSelectable(eventUnit, ")") + "\n";
            output += CodeBuilder.Indent(2) + CodeUtility.MakeSelectable(eventUnit, "{") + "\n";
            output += CodeBuilder.Indent(3) + (eventUnit.coroutine ? CodeUtility.MakeSelectable(eventUnit, $"StartCoroutine(") + GetMethodName(eventUnit) + CodeUtility.MakeSelectable(eventUnit, $"({"args".VariableHighlight()}));") : GetMethodName(eventUnit) + CodeUtility.MakeSelectable(eventUnit, $"({"args".VariableHighlight()});"));
            output += "\n" + CodeBuilder.Indent(2) + CodeUtility.MakeSelectable(eventUnit, "}") + "\n";
            return output;
        }

        private string AddNewMethod(Unit unit, string name, string methodSignture, string methodBody, string parameters, ControlGenerationData generationData)
        {
            var method = new GraphMethodDecleration(unit, name, methodSignture, methodBody, parameters, generationData);
            _methods.Add(CodeUtility.CleanCode(name), method);
            return method.GetMethod();
        }

        private string GetMethodName(IEventUnit eventUnit)
        {
            var UnitTitle = BoltFlowNameUtility.UnitTitle(eventUnit.GetType(), false, false).LegalMemberName();

            string methodName;

            if (EVENT_NAMES.TryGetValue(UnitTitle, out var title))
            {
                methodName = CodeUtility.MakeSelectable(eventUnit as Unit, title);
            }
            else
            {
                methodName = CodeUtility.MakeSelectable(eventUnit as Unit, UnitTitle);
            }

            if (eventUnit is CustomEvent customEvent)
            {
                if (!customEvent.name.hasValidConnection)
                {
                    methodName = CodeUtility.MakeSelectable(eventUnit as Unit, (string)customEvent.defaultValues[customEvent.name.key]);
                }
                else
                {
                    return CodeUtility.MakeSelectable(eventUnit as Unit, "CustomEvent" + _customEventIds[eventUnit as CustomEvent]);
                }
            }
            else if (eventUnit is BoltNamedAnimationEvent animationEvent)
            {
                methodName = animationEvent.GenerateValue(animationEvent.name);
            }
            else if (NodeGenerator.GetSingleDecorator(eventUnit as Unit, eventUnit as Unit) is MethodNodeGenerator methodNodeGenerator)
            {
                return CodeUtility.MakeSelectable(eventUnit as Unit, methodNodeGenerator.Name);
            }

            return methodName;
        }

        private string GetMethodBody(IEventUnit eventUnit, ControlGenerationData data)
        {
            var variablesCode = "";
            foreach (var variable in eventUnit.graph.variables)
            {
                if (!data.ContainsNameInAnyScope(variable.name))
                {
                    variablesCode += CodeBuilder.Indent(2) + CodeUtility.MakeSelectable(eventUnit as Unit, Type.GetType(variable.typeHandle.Identification).As().CSharpName(false, true) + " " + variable.name.LegalMemberName().VariableHighlight() + (variable.value != null ? $" = " + "" + $"{variable.value.As().Code(true, true, true)}" : string.Empty) + ";") + "\n";
                    data.AddLocalNameInScope(variable.name, Type.GetType(variable.typeHandle.Identification) ?? typeof(object));
                }
            }

            var methodBody = variablesCode + (eventUnit as Unit).GenerateControl(null, data, 2);

            return methodBody;
        }

        private string GetMethodSignature(IEventUnit eventUnit, string methodName = null)
        {
            return GetMethodSignature(eventUnit as Unit, eventUnit.coroutine, methodName == null ? GetMethodName(eventUnit) : methodName);
        }

        private string GetMethodSignature(IEventUnit eventUnit, bool isCoroutine)
        {
            return GetMethodSignature(eventUnit as Unit, isCoroutine, GetMethodName(eventUnit));
        }

        private string GetMethodSignature(Unit unit, bool isCoroutine, string _methodName, AccessModifier accessModifier = AccessModifier.Public)
        {
            var returnType = isCoroutine ? "System.Collections".NamespaceHighlight() + "." + "IEnumerator".TypeHighlight() : "void".ConstructHighlight();
            var methodName = _methodName;

            return $"\n" + CodeBuilder.Indent(1) + CodeUtility.MakeSelectable(unit, accessModifier.AsString().ConstructHighlight() + $" {returnType} ") + $"{methodName.Replace(" ", "")}";
        }

        private (string parameterSignature, (Type parameterType, string parameterName) paramInfo) GetMethodParameters(IEventUnit eventUnit)
        {
            return MethodParameterMapper.GetParameters(eventUnit);
        }
        #endregion

        #region Helper Classes
        private static class MethodParameterMapper
        {
 private static readonly Dictionary<Type, (string signature, (Type type, string name) info)> _parameterMap
                = new Dictionary<Type, (string, (Type, string))>
            {
#if MODULE_PHYSICS_EXISTS
                { typeof(OnCollisionEnter), ($"{"Collision".TypeHighlight()} {"collision".VariableHighlight()}", (typeof(Collision), "collision")) },
                { typeof(OnCollisionExit), ($"{"Collision".TypeHighlight()} {"collision".VariableHighlight()}", (typeof(Collision), "collision")) },
                { typeof(OnCollisionStay), ($"{"Collision".TypeHighlight()} {"collision".VariableHighlight()}", (typeof(Collision), "collision")) },
                { typeof(OnJointBreak), ($"{"float".ConstructHighlight()} {"breakForce".VariableHighlight()}", (typeof(float), "breakForce")) },
                { typeof(OnTriggerEnter), ($"{"Collider".TypeHighlight()} {"other".VariableHighlight()}", (typeof(Collider), "other")) },
                { typeof(OnTriggerExit), ($"{"Collider".TypeHighlight()} {"other".VariableHighlight()}", (typeof(Collider), "other")) },
                { typeof(OnTriggerStay), ($"{"Collider".TypeHighlight()} {"other".VariableHighlight()}", (typeof(Collider), "other")) },
                { typeof(OnControllerColliderHit), ($"{"ControllerColliderHit".TypeHighlight()} {"hitData".VariableHighlight()}", (typeof(ControllerColliderHit), "hitData")) },
                { typeof(OnParticleCollision), ($"{"GameObject".TypeHighlight()} {"other".VariableHighlight()}", (typeof(GameObject), "other")) },
#endif
#if MODULE_PHYSICS_2D_EXISTS
                { typeof(OnCollisionEnter2D), ($"{"Collision2D".TypeHighlight()} {"collision".VariableHighlight()}", (typeof(Collision2D), "collision")) },
                { typeof(OnCollisionExit2D), ($"{"Collision2D".TypeHighlight()} {"collision".VariableHighlight()}", (typeof(Collision2D), "collision")) },
                { typeof(OnCollisionStay2D), ($"{"Collision2D".TypeHighlight()} {"collision".VariableHighlight()}", (typeof(Collision2D), "collision")) },
                { typeof(OnJointBreak2D), ($"{"Joint2D".TypeHighlight()} {"brokenJoint".VariableHighlight()}", (typeof(Joint2D), "brokenJoint")) },
                { typeof(OnTriggerEnter2D), ($"{"Collider2D".TypeHighlight()} {"other".VariableHighlight()}", (typeof(Collider2D), "other")) },
                { typeof(OnTriggerExit2D), ($"{"Collider2D".TypeHighlight()} {"other".VariableHighlight()}", (typeof(Collider2D), "other")) },
                { typeof(OnTriggerStay2D), ($"{"Collider2D".TypeHighlight()} {"other".VariableHighlight()}", (typeof(Collider2D), "other")) },
#endif
                { typeof(OnApplicationFocus), ($"{"bool".ConstructHighlight()} {"focusStatus".VariableHighlight()}", (typeof(bool), "focusStatus")) },
                { typeof(OnApplicationPause), ($"{"bool".ConstructHighlight()} {"pauseStatus".VariableHighlight()}", (typeof(bool), "pauseStatus")) },
                { typeof(CustomEvent), ($"{"CustomEventArgs".TypeHighlight()} {"args".VariableHighlight()}", (typeof(CustomEventArgs), "args")) }
            };
            public static (string parameterSignature, (Type parameterType, string parameterName) paramInfo)
                GetParameters(IEventUnit eventUnit)
            {
                return _parameterMap.TryGetValue(eventUnit.GetType(), out var parameterInfo)
                    ? parameterInfo
                    : (string.Empty, (null, string.Empty));
            }
        }

        private class GraphMethodDecleration
        {
            public string name;
            public string parameters;
            public string methodBody;
            public string methodSignature;
            private Unit unit;
            public ControlGenerationData generationData;

            public GraphMethodDecleration(Unit unit, string name, string methodSignature, string methodBody, string parameters, ControlGenerationData generationData)
            {
                this.unit = unit;
                this.name = name;
                this.methodSignature = methodSignature;
                this.parameters = parameters;
                this.methodBody = methodBody;
                this.generationData = generationData;
            }

            public string GetMethod()
            {
                var method = string.Empty;
                method += CodeBuilder.Indent(1) + methodSignature;
                method += CodeUtility.MakeSelectable(unit, $"({parameters})");
                method += $"\n{CodeBuilder.Indent(1)}{{\n{methodBody}\n";
                method += CodeBuilder.Indent(1) + "}";
                return method;
            }
        }
        #endregion
    }
}
