using System;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

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

        private Dictionary<string, GraphMethodDecleration> methods;

        private Dictionary<CustomEvent, int> customEventIds;

        public override string Generate(int indent)
        {
            var script = string.Empty;

            if (Data?.graph == null)
            {
                return "";
            }

            var Units = Data.graph.GetUnitsRecursive(Recursion.New(Recursion.defaultMaxDepth));

            methods = new();

            var usings = new List<string> { "Unity", "UnityEngine", "Unity.VisualScripting" };

            foreach (Unit unit in Units)
            {
                var generator = NodeGenerator.GetSingleDecorator(unit, unit);
                if (!string.IsNullOrEmpty(generator.NameSpace))
                {
                    foreach (var ns in generator.NameSpace.Split(","))
                    {
                        if (!usings.Contains(ns))
                        {
                            usings.Add(ns);
                        }
                    }
                }
            }

            foreach (var ns in usings)
            {
                if (!string.IsNullOrWhiteSpace(ns))
                {
                    script += $"using".ConstructHighlight() + $" {ns};\n";
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
                                + $"{variable.value.As().Code(true, true, true)};\n"
                            : string.Empty + ";"
                    );
            };

            foreach (Unit unit in Units)
            {
                if (unit is Once)
                {
                    script +=
                        "\n    " + "private ".ConstructHighlight()
                        + "bool ".ConstructHighlight()
                        + "Once_".VariableHighlight()
                        + NodeGenerator.GetSingleDecorator(unit, unit).UniqueID.VariableHighlight()
                        + ";\n";
                }
                else if (unit is ToggleFlow)
                {
                    script +=
                        "\n    " + "private ".ConstructHighlight()
                        + "ToggleFlowLogic ".TypeHighlight()
                        + "Toggle_".VariableHighlight()
                        + NodeGenerator.GetSingleDecorator(unit, unit).UniqueID.VariableHighlight()
                        + ";\n";
                }
            }
            customEventIds = new Dictionary<CustomEvent, int>();
            if (Units.Any(unit => unit is CustomEvent))
            {
                var customEvents = Units.Where(unit => unit is CustomEvent);
                script += $"\n    " + "private void".ConstructHighlight() + " Awake()";
                script += "\n    {\n";
                int id = 0;
                foreach (CustomEvent eventUnit in customEvents)
                {
                    var data = new ControlGenerationData();
                    data.AddLocalNameInScope("args");
                    foreach (VariableDeclaration variable in Data.graph.variables)
                    {
                        data.AddLocalNameInScope(variable.name, !string.IsNullOrEmpty(variable.typeHandle.Identification) ? Type.GetType(variable.typeHandle.Identification) : typeof(object));
                    };
                    customEventIds.Add(eventUnit, id);
                    id++;
                    script += CodeUtility.MakeSelectable(eventUnit, $"        {"EventBus".TypeHighlight()}.Register<{"CustomEventArgs".TypeHighlight()}>({"new".ConstructHighlight()} {"EventHook".TypeHighlight()}({"EventHooks".TypeHighlight()}.{"Custom".VariableHighlight()}, ") + eventUnit.GenerateValue(eventUnit.target) + CodeUtility.MakeSelectable(eventUnit, $"), ") + GetMethodName(eventUnit) + CodeUtility.MakeSelectable(eventUnit, "Runner);");

                    script += "\n";

                    var eventName = GetMethodName(eventUnit) + CodeUtility.MakeSelectable(eventUnit, "Runner");

                    AddNewMethod(eventUnit, eventName, GetMethodSignature(eventUnit, false, eventName, AccessModifier.Private), CodeBuilder.Indent(2) + (eventUnit.coroutine ? CodeUtility.MakeSelectable(eventUnit, $"StartCoroutine(") + GetMethodName(eventUnit) + CodeUtility.MakeSelectable(eventUnit, $"({"args".VariableHighlight()}));") : GetMethodName(eventUnit) + CodeUtility.MakeSelectable(eventUnit, $"({"args".VariableHighlight()});")), "CustomEventArgs ".TypeHighlight() + "args".VariableHighlight(), data);
                }
                script += "\n    }\n";
            }

            foreach (IEventUnit unit in Units.Where(unit => unit is IEventUnit && (unit as IEventUnit).coroutine))
            {
                if (!methods.ContainsKey(GetMethodName(unit)))
                {
                    var data = new ControlGenerationData();

                    foreach (VariableDeclaration variable in Data.graph.variables)
                    {
                        data.AddLocalNameInScope(variable.name, !string.IsNullOrEmpty(variable.typeHandle.Identification) ? Type.GetType(variable.typeHandle.Identification) : typeof(object));
                    }

                    var parameters = GetMethodParameters(unit);
                    data.AddLocalNameInScope(parameters.parameterName);
                    if (unit.controlOutputs.Any(output => output.key == "trigger"))
                    {
                        if (unit.controlOutputs.First(output => output.key == "trigger").hasValidConnection) AddNewMethod(unit as Unit, GetMethodName(unit), GetMethodSignature(unit), GetMethodBody(unit, data), parameters.parameterSignature, data);
                    }
                    else
                    {
                        if (unit.controlOutputs.First().hasValidConnection) AddNewMethod(unit as Unit, GetMethodName(unit), GetMethodSignature(unit), GetMethodBody(unit, data), parameters.parameterSignature, data);
                    }
                }
                else if (methods.TryGetValue(GetMethodName(unit), out var method))
                {
                    method.methodBody = method.methodBody + "\n" + GetMethodBody(unit, method.generationData);
                }
            }

            foreach (IEventUnit unit in Units.Where(unit => unit is IEventUnit && !(unit as IEventUnit).coroutine))
            {
                if (!methods.ContainsKey(GetMethodName(unit)))
                {
                    var data = new ControlGenerationData();
                    foreach (VariableDeclaration variable in Data.graph.variables)
                    {
                        data.AddLocalNameInScope(variable.name, !string.IsNullOrEmpty(variable.typeHandle.Identification) ? Type.GetType(variable.typeHandle.Identification) : typeof(object));
                    };
                    var parameters = GetMethodParameters(unit);
                    data.AddLocalNameInScope(parameters.parameterName);
                    if (unit.controlOutputs.Any(output => output.key == "trigger"))
                    {
                        if (unit.controlOutputs.First(output => output.key == "trigger").hasValidConnection) AddNewMethod(unit as Unit, GetMethodName(unit), GetMethodSignature(unit), GetMethodBody(unit, data), parameters.parameterSignature, data);
                    }
                    else
                    {
                        if (unit.controlOutputs.First().hasValidConnection) AddNewMethod(unit as Unit, GetMethodName(unit), GetMethodSignature(unit), GetMethodBody(unit, data), parameters.parameterSignature, data);
                    }
                }
                else if (methods.TryGetValue(GetMethodName(unit), out var method))
                {
                    method.methodBody = GetMethodBody(unit, method.generationData) + "\n" + method.methodBody;
                }
            }

            foreach (var method in methods.Values)
            {
                script += "\n" + method.GetMethod() + "\n";
            }

            script += "}";

            return script;
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
            var methodBody = variablesCode + NodeGenerator.GetSingleDecorator(eventUnit as Unit, eventUnit as Unit).GenerateControl(null, data, 2);

            return methodBody;
        }

        private string GetMethodSignature(IEventUnit eventUnit)
        {
            return GetMethodSignature(eventUnit as Unit, eventUnit.coroutine, GetMethodName(eventUnit));
        }

        private string GetMethodSignature(Unit unit, bool isCoroutine, string _methodName, AccessModifier accessModifier = AccessModifier.Public)
        {
            var returnType = isCoroutine ? "System.Collections".NamespaceHighlight() + "." + "IEnumerator".TypeHighlight() : "void".ConstructHighlight();
            var methodName = _methodName;

            return $"\n" + CodeBuilder.Indent(1) + CodeUtility.MakeSelectable(unit, accessModifier.AsString().ConstructHighlight() + $" {returnType} ") + $"{methodName.Replace(" ", "")}";
        }

        private (string parameterSignature, string parameterName) GetMethodParameters(IEventUnit eventUnit)
        {
            Dictionary<Type, (string parameterSignature, string parameterName)> parameterMappings = new Dictionary<Type, (string, string)>
            {
                { typeof(OnCollisionEnter), ("Collision ".TypeHighlight() + "collision".VariableHighlight(), "collision") },
                { typeof(OnCollisionExit), ("Collision ".TypeHighlight() + "collision".VariableHighlight(), "collision") },
                { typeof(OnCollisionStay), ("Collision ".TypeHighlight() + "collision".VariableHighlight(), "collision") },
                { typeof(OnJointBreak), ("float ".ConstructHighlight() + "breakForce".VariableHighlight(), "breakForce") },
                { typeof(OnCollisionEnter2D), ("Collision2D ".TypeHighlight() + "collision".VariableHighlight(), "collision") },
                { typeof(OnCollisionExit2D), ("Collision2D ".TypeHighlight() + "collision".VariableHighlight(), "collision") },
                { typeof(OnCollisionStay2D), ("Collision2D ".TypeHighlight() + "collision".VariableHighlight(), "collision") },
                { typeof(OnJointBreak2D), ("Joint2D ".TypeHighlight() + "brokenJoint".VariableHighlight(), "brokenJoint") },
                { typeof(OnTriggerEnter), ("Collider ".TypeHighlight() + "other".VariableHighlight(), "other") },
                { typeof(OnTriggerEnter2D), ("Collider2D ".TypeHighlight() + "other".VariableHighlight(), "other") },
                { typeof(OnTriggerExit), ("Collider ".TypeHighlight() + "other".VariableHighlight(), "other") },
                { typeof(OnTriggerStay), ("Collider ".TypeHighlight() + "other".VariableHighlight(), "other") },
                { typeof(OnTriggerExit2D), ("Collider2D ".TypeHighlight() + "other".VariableHighlight(), "other") },
                { typeof(OnTriggerStay2D), ("Collider2D ".TypeHighlight() + "other".VariableHighlight(), "other") },
                { typeof(OnControllerColliderHit), ("ControllerColliderHit ".TypeHighlight() + "hitData".VariableHighlight(), "hitData") },
                { typeof(OnApplicationFocus), ("bool ".ConstructHighlight() + "focusStatus".VariableHighlight(), "focusStatus") },
                { typeof(OnApplicationPause), ("bool ".ConstructHighlight() + "pauseStatus".VariableHighlight(), "pauseStatus") },
                { typeof(CustomEvent), ("CustomEventArgs ".TypeHighlight() + "args".VariableHighlight(), "args") }
            };

            if (parameterMappings.TryGetValue(eventUnit.GetType(), out var parameterInfo))
            {
                return parameterInfo;
            }

            return (string.Empty, string.Empty);
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

    public static class GraphAssetGeneratorUtilites
    {
        public static IEnumerable<IUnit> GetUnits(this FlowGraph flowGraph, Recursion recursion)
        {
            Ensure.That(nameof(flowGraph)).IsNotNull(flowGraph);
            if (!recursion?.TryEnter(flowGraph) ?? false)
            {
                yield break;
            }

            var stack = new Stack<FlowGraph>();
            stack.Push(flowGraph);

            while (stack.Count > 0)
            {
                var currentGraph = stack.Pop();

                foreach (var unit in currentGraph.units)
                {
                    yield return unit;

                    if (unit is SubgraphUnit subgraphUnit && subgraphUnit?.nest?.graph != null)
                    {
                        stack.Push(subgraphUnit.nest.graph);
                    }
                }

                recursion?.Exit(currentGraph);
            }
        }
    }
}
