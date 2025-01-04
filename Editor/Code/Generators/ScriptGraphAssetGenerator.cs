using System;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;
using System.Collections;

namespace Unity.VisualScripting.Community
{
    [Serializable]
    [CodeGenerator(typeof(ScriptGraphAsset))]
    public sealed class ScriptGraphAssetGenerator : CodeGenerator<ScriptGraphAsset>
    {

        private readonly Dictionary<string, string> EventsNames = new Dictionary<string, string>()
        {
            { "OnStart", "Start" },
            { "OnUpdate", "Update" },
            { "OnAwake", "Awake" },
            { "OnFixedUpdate", "FixedUpdate" },
            { "OnLateUpdate", "LateUpdate" },
        };

        List<Type> UnityMethodTypes = new List<Type>
        {
            typeof(OnCollisionEnter),
            typeof(OnCollisionExit),
            typeof(OnCollisionStay),
            typeof(OnJointBreak),
            typeof(OnCollisionEnter2D),
            typeof(OnCollisionExit2D),
            typeof(OnCollisionStay2D),
            typeof(OnJointBreak2D),
            typeof(OnTriggerEnter),
            typeof(OnTriggerEnter2D),
            typeof(OnTriggerExit),
            typeof(OnTriggerStay),
            typeof(OnTriggerExit2D),
            typeof(OnTriggerStay2D),
            typeof(OnControllerColliderHit),
            typeof(OnApplicationFocus),
            typeof(OnApplicationPause),
            typeof(Start),
            typeof(Update),
            typeof(FixedUpdate),
            typeof(LateUpdate)
        };

        private Dictionary<string, GraphMethodDecleration> methods;

        private Dictionary<CustomEvent, int> customEventIds;

        public List<Timer> timers = new List<Timer>();

        public override string Generate(int indent)
        {
            var script = string.Empty;
            timers = new List<Timer>();

            if (Data?.graph == null)
            {
                return "";
            }

            var Units = Data.graph.GetUnitsRecursive(Recursion.New(Recursion.defaultMaxDepth));
            var EventUnits = Units.Where(unit => unit is IEventUnit).Cast<IEventUnit>();

            methods = new();

            var usings = new List<(string, Unit)> { ("Unity", null), ("UnityEngine", null), ("Unity.VisualScripting", null) };
            var count = 0;
            foreach (Unit unit in Units)
            {
                var generator = NodeGenerator.GetSingleDecorator(unit, unit);
                if (unit is Timer timer)
                {
                    timers.Add(timer);
                    (generator as TimerGenerator).count = count;
                    count++;
                }

                if (!string.IsNullOrEmpty(generator.NameSpace))
                {
                    foreach (var ns in generator.NameSpace.Split(","))
                    {
                        if (!usings.Any(@using => @using.Item1 == ns))
                        {
                            usings.Add((ns, unit));
                        }
                    }
                }
            }

            foreach (var @using in usings)
            {
                if (!string.IsNullOrWhiteSpace(@using.Item1))
                {
                    if (@using.Item2 != null)
                    {
                        script += CodeUtility.MakeSelectable(@using.Item2, $"using".ConstructHighlight() + $" {@using.Item1};") + "\n";
                    }
                    else
                        script += $"using".ConstructHighlight() + $" {@using.Item1};\n";
                }
            }

            script +=
                $"\n" + "public class ".ConstructHighlight()
                + $"{(Data.graph.title?.Length > 0 ? Data.graph.title : Data.name)}".LegalMemberName().TypeHighlight()
                + " : "
                + "MonoBehaviour\n".TypeHighlight()
                + "{";

            foreach (VariableDeclaration variable in Data.graph.variables)
            {
                script +=
                    "\n    " + "public ".ConstructHighlight()
                    + Type.GetType(variable.typeHandle.Identification).As().CSharpName(false, true)
                    + " "
                    + variable.name.LegalMemberName().VariableHighlight()
                    + (
                        variable.value != null
                            ? $" = "
                                + ""
                                + $"{variable.value.As().Code(true, true, true, "", true, true)};\n"
                            : string.Empty + ";\n"
                    );
            };

            foreach (Unit unit in Units)
            {
                if (unit.GetGenerator() is VariableNodeGenerator variableNodeGenerator)
                {
                    CodeBuilder.Indent(1);
                    script +=
                        "\n    " + CodeUtility.MakeSelectable(unit, variableNodeGenerator.AccessModifier.AsString().ConstructHighlight() + " "
                        + variableNodeGenerator.Type.As().CSharpName(false, true) + " " + variableNodeGenerator.FieldModifier.AsString().ConstructHighlight()
                        + variableNodeGenerator.Name.VariableHighlight()
                        + (variableNodeGenerator.HasDefaultValue ? $" = " : "")) + (variableNodeGenerator.HasDefaultValue ? variableNodeGenerator.DefaultValue.As().Code(variableNodeGenerator.IsNew, variableNodeGenerator.unit, variableNodeGenerator.Literal, true, "", variableNodeGenerator.NewLineLiteral, true) : "") + CodeUtility.MakeSelectable(unit, ";") + "\n";
                }
            }
            customEventIds = new Dictionary<CustomEvent, int>();
            if (Data.graph.units.Any(unit => unit is CustomEvent))
            {
                var customEvents = Data.graph.units.Where(unit => unit is CustomEvent);
                script += $"\n    " + "private void".ConstructHighlight() + " Awake()";
                script += "\n    {\n";
                int id = 0;
                foreach (CustomEvent eventUnit in customEvents)
                {
                    var data = new ControlGenerationData();
                    data.ScriptType = typeof(MonoBehaviour);
                    data.returns = eventUnit.coroutine ? typeof(IEnumerator) : typeof(void);
                    data.AddLocalNameInScope("args", typeof(CustomEventArgs));
                    foreach (VariableDeclaration variable in Data.graph.variables)
                    {
                        data.AddLocalNameInScope(variable.name, !string.IsNullOrEmpty(variable.typeHandle.Identification) ? Type.GetType(variable.typeHandle.Identification) : typeof(object));
                    };
                    customEventIds.Add(eventUnit, id);
                    id++;
                    script += CodeUtility.MakeSelectable(eventUnit, $"        {"CSharpUtility".TypeHighlight()}.RegisterCustomEvent(") + eventUnit.GenerateValue(eventUnit.target) + CodeUtility.MakeSelectable(eventUnit, $", ") + GetMethodName(eventUnit) + CodeUtility.MakeSelectable(eventUnit, "Runner);");

                    script += "\n";

                    var eventName = GetMethodName(eventUnit) + CodeUtility.MakeSelectable(eventUnit, "Runner");
                    string runnerCode = GetCustomEventRunnerCode(eventUnit, data);
                    AddNewMethod(eventUnit, eventName, GetMethodSignature(eventUnit, false, eventName, AccessModifier.Private), runnerCode, "CustomEventArgs ".TypeHighlight() + "args".VariableHighlight(), data);
                }
                script += "\n    }\n";
            }
            bool addedTimerCode = false;
            foreach (IEventUnit unit in EventUnits)
            {
                if (unit is CustomEvent && unit.graph != Data.graph) continue;
                var timerCode = "";
                if (unit.coroutine)
                {
                    if (!methods.ContainsKey(GetMethodName(unit)))
                    {
                        var data = new ControlGenerationData();
                        data.ScriptType = typeof(MonoBehaviour);
                        data.returns = typeof(IEnumerator);
                        foreach (VariableDeclaration variable in Data.graph.variables)
                        {
                            data.AddLocalNameInScope(variable.name, !string.IsNullOrEmpty(variable.typeHandle.Identification) ? Type.GetType(variable.typeHandle.Identification) : typeof(object));
                        }

                        var parameters = GetMethodParameters(unit);
                        data.AddLocalNameInScope(parameters.paramInfo.parameterName, parameters.paramInfo.parameterType);

                        if (unit.controlOutputs.Any(output => output.key == "trigger"))
                        {
                            if (unit is Update && !addedTimerCode)
                            {
                                addedTimerCode = true;
                                timerCode = string.Join("\n", timers.Select(t => CodeBuilder.Indent(2) + CodeUtility.MakeSelectable(t, (NodeGenerator.GetSingleDecorator(t, t) as TimerGenerator).Name.VariableHighlight() + ".Update();")));
                                timerCode += "\n";
                            }

                            if (UnityMethodTypes.Contains(unit.GetType()))
                            {
                                if (unit.controlOutputs["trigger"].hasValidConnection)
                                {
                                    AddNewMethod(unit as Unit, GetMethodName(unit), GetMethodSignature(unit, false), timerCode + CodeBuilder.Indent(2) + CodeUtility.MakeSelectable(unit as Unit, $"StartCoroutine(") + GetMethodName(unit) + CodeUtility.MakeSelectable(unit as Unit, "_Coroutine());"), parameters.parameterSignature, data);
                                    AddNewMethod(unit as Unit, GetMethodName(unit) + CodeUtility.MakeSelectable(unit as Unit, "_Coroutine"), GetMethodSignature(unit, GetMethodName(unit) + CodeUtility.MakeSelectable(unit as Unit, "_Coroutine")), GetMethodBody(unit, data), parameters.parameterSignature, data);
                                }
                            }
                            else if (unit.controlOutputs.First(output => output.key == "trigger").hasValidConnection)
                            {
                                AddNewMethod(unit as Unit, GetMethodName(unit), GetMethodSignature(unit), GetMethodBody(unit, data), parameters.parameterSignature, data);
                            }
                        }
                    }
                    else if (methods.TryGetValue(GetMethodName(unit), out var method))
                    {
                        method.methodBody = method.methodBody + "\n" + GetMethodBody(unit, method.generationData);
                    }
                }
                else
                {
                    if (!methods.ContainsKey(GetMethodName(unit)))
                    {
                        var data = new ControlGenerationData();
                        data.ScriptType = typeof(MonoBehaviour);
                        data.returns = typeof(void);
                        foreach (VariableDeclaration variable in Data.graph.variables)
                        {
                            data.AddLocalNameInScope(variable.name, !string.IsNullOrEmpty(variable.typeHandle.Identification) ? Type.GetType(variable.typeHandle.Identification) : typeof(object));
                        };
                        var parameters = GetMethodParameters(unit);
                        data.AddLocalNameInScope(parameters.paramInfo.parameterName, parameters.paramInfo.parameterType);
                        if (unit is Update update && !addedTimerCode)
                        {
                            addedTimerCode = true;
                            timerCode = string.Join("\n", timers.Select(t => CodeBuilder.Indent(2) + CodeUtility.MakeSelectable(t, (NodeGenerator.GetSingleDecorator(t, t) as TimerGenerator).Name.VariableHighlight() + ".Update();")));
                            timerCode += "\n";
                        }

                        if (unit.controlOutputs.Any(output => output.key == "trigger"))
                        {
                            if (unit.controlOutputs.First(output => output.key == "trigger").hasValidConnection) AddNewMethod(unit as Unit, GetMethodName(unit), GetMethodSignature(unit), timerCode + GetMethodBody(unit, data), parameters.parameterSignature, data);
                        }
                        else
                        {
                            if (unit.controlOutputs.Count > 0 && unit.controlOutputs.First().hasValidConnection) AddNewMethod(unit as Unit, GetMethodName(unit), GetMethodSignature(unit), GetMethodBody(unit, data), parameters.parameterSignature, data);
                        }
                    }
                    else if (methods.TryGetValue(GetMethodName(unit), out var method))
                    {
                        method.methodBody = GetMethodBody(unit, method.generationData) + "\n" + method.methodBody;
                    }
                }
            }

            if (!EventUnits.Any(e => e is Update) && !addedTimerCode && timers.Count > 0)
            {
                var unit = new Update();
                var data = new ControlGenerationData();
                data.ScriptType = typeof(MonoBehaviour);
                var timerCode = string.Join("\n", timers.Select(t => CodeBuilder.Indent(2) + CodeUtility.MakeSelectable(t, (NodeGenerator.GetSingleDecorator(t, t) as TimerGenerator).Name.VariableHighlight() + ".Update();")));
                AddNewMethod(unit, GetMethodName(unit), GetMethodSignature(unit), timerCode, "", data);
            }

            foreach (var method in methods.Values)
            {
                script += "\n" + method.GetMethod() + "\n";
            }

            script += "}";

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
            methods.Add(name, method);
            return method.GetMethod();
        }

        private string GetMethodName(IEventUnit eventUnit)
        {
            var UnitTitle = BoltFlowNameUtility.UnitTitle(eventUnit.GetType(), false, false).LegalMemberName();

            string methodName;

            if (EventsNames.TryGetValue(UnitTitle, out var title))
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
                    return CodeUtility.MakeSelectable(eventUnit as Unit, "CustomEvent" + customEventIds[eventUnit as CustomEvent]);
                }
            }
            else if (eventUnit is BoltNamedAnimationEvent animationEvent)
            {
                methodName = animationEvent.GenerateValue(animationEvent.name);
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
            Dictionary<Type, (string parameterSignature, (Type parameterType, string parameterName))> parameterMappings = new Dictionary<Type, (string, (Type parameterType, string parameterName))>
            {
                { typeof(OnCollisionEnter), ("Collision ".TypeHighlight() + "collision".VariableHighlight(), (typeof(Collision), "collision")) },
                { typeof(OnCollisionExit), ("Collision ".TypeHighlight() + "collision".VariableHighlight(), (typeof(Collision), "collision")) },
                { typeof(OnCollisionStay), ("Collision ".TypeHighlight() + "collision".VariableHighlight(), (typeof(Collision), "collision")) },
                { typeof(OnJointBreak), ("float ".ConstructHighlight() + "breakForce".VariableHighlight(), (typeof(float), "breakForce")) },
                { typeof(OnCollisionEnter2D), ("Collision2D ".TypeHighlight() + "collision".VariableHighlight(), (typeof(Collision2D), "collision")) },
                { typeof(OnCollisionExit2D), ("Collision2D ".TypeHighlight() + "collision".VariableHighlight(), (typeof(Collision2D), "collision")) },
                { typeof(OnCollisionStay2D), ("Collision2D ".TypeHighlight() + "collision".VariableHighlight(), (typeof(Collision2D), "collision")) },
                { typeof(OnJointBreak2D), ("Joint2D ".TypeHighlight() + "brokenJoint".VariableHighlight(), (typeof(Joint2D), "brokenJoint")) },
                { typeof(OnTriggerEnter), ("Collider ".TypeHighlight() + "other".VariableHighlight(), (typeof(Collider), "other")) },
                { typeof(OnTriggerEnter2D), ("Collider2D ".TypeHighlight() + "other".VariableHighlight(), (typeof(Collider2D), "other")) },
                { typeof(OnTriggerExit), ("Collider ".TypeHighlight() + "other".VariableHighlight(), (typeof(Collider), "other")) },
                { typeof(OnTriggerStay), ("Collider ".TypeHighlight() + "other".VariableHighlight(), (typeof(Collider), "other")) },
                { typeof(OnTriggerExit2D), ("Collider2D ".TypeHighlight() + "other".VariableHighlight(), (typeof(Collider2D), "other")) },
                { typeof(OnTriggerStay2D), ("Collider2D ".TypeHighlight() + "other".VariableHighlight(), (typeof(Collider2D), "other")) },
                { typeof(OnControllerColliderHit), ("ControllerColliderHit ".TypeHighlight() + "hitData".VariableHighlight(), (typeof(ControllerColliderHit), "hitData")) },
                { typeof(OnApplicationFocus), ("bool ".ConstructHighlight() + "focusStatus".VariableHighlight(), (typeof(bool), "focusStatus")) },
                { typeof(OnApplicationPause), ("bool ".ConstructHighlight() + "pauseStatus".VariableHighlight(), (typeof(bool), "pauseStatus")) },
                { typeof(CustomEvent), ("CustomEventArgs ".TypeHighlight() + "args".VariableHighlight(), (typeof(CustomEventArgs), "args")) }
            };


            if (parameterMappings.TryGetValue(eventUnit.GetType(), out var parameterInfo))
            {
                return parameterInfo;
            }

            return (string.Empty, (null, string.Empty));
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
    }
}
