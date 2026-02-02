using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

#if VISUAL_SCRIPTING_1_7
using SUnit = Unity.VisualScripting.SubgraphUnit;
#else
using SUnit = Unity.VisualScripting.SuperUnit;
#endif

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(SUnit))]
    public sealed class SubgraphGenerator : NodeGenerator<SUnit>, IRequireVariables
    {
        private static readonly Dictionary<string, Type> typeCache = new Dictionary<string, Type>();

        private readonly Dictionary<CustomEvent, int> customEventIds = new Dictionary<CustomEvent, int>();
        private readonly HashSet<CustomEvent> customEvents = new HashSet<CustomEvent>();

        private Unit graphInput;
        private Unit graphOutput;
        /// <summary>
        /// Has code that is not triggered in the scope that the Subgraph is Generated in,
        /// To avoid errors we make the variables from this subgraph class wide so it will be accessible.
        /// This might add variables that are not used outside of the scope but it this is the best solution i could come up with.
        /// </summary>
        private bool hasExternalCode;

        public override IEnumerable<string> GetNamespaces()
        {
            if (Unit.nest == null || Unit.nest.graph == null)
                yield break;

            foreach (var variable in Unit.nest.graph.variables)
            {
#if VISUAL_SCRIPTING_1_7
                Type type = GetCachedType(variable.typeHandle.Identification);
#else
                Type type = variable.value != null ? variable.value.GetType() : typeof(object);
#endif
                foreach (var @namespace in type.GetAllNamespaces())
                {
                    yield return @namespace;
                }
            }
        }

        public SubgraphGenerator(SUnit unit) : base(unit)
        {
            RebuildGraphState();
        }

        private void RebuildGraphState()
        {
            graphInput = null;
            graphOutput = null;
            hasExternalCode = false;
            customEvents.Clear();
            customEventIds.Clear();

            if (Unit.nest == null || Unit.nest.graph == null)
                return;

            foreach (Unit u in Unit.nest.graph.units)
            {
                if (graphInput == null && u is GraphInput)
                {
                    graphInput = u;
                    continue;
                }

                if (graphOutput == null && u is GraphOutput)
                {
                    graphOutput = u;
                    continue;
                }
                var generator = u.GetGenerator();
                if (u is IEventUnit || generator is IRequireMethods or MethodNodeGenerator)
                {
                    if (u is CustomEvent ce)
                    {
                        customEvents.Add(ce);
                    }
                    else
                    {
                        hasExternalCode = true;
                    }
                }
            }

            if (graphInput != null)
            {
                var gen = graphInput.GetGenerator() as GraphInputGenerator;
                gen.ClearConnectedValueInputs();
                gen.parent = Unit;
                if (gen != null)
                {
                    foreach (var input in Unit.valueInputs)
                    {
                        if (input.hasDefaultValue || input.hasValidConnection)
                        {
                            gen.AddConnectedValueInput(input);
                        }
                    }
                }
            }

            if (graphOutput != null)
            {
                var gen = graphOutput.GetGenerator() as GraphOutputGenerator;
                gen.parent = Unit;
                if (gen != null)
                {
                    gen.ClearConnectedGraphOutputs();
                    foreach (var output in Unit.controlOutputs.Where(o => o.hasValidConnection))
                    {
                        gen.AddConnectedControlOutput(output);
                    }
                }
            }
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (Unit.nest == null || Unit.nest.graph == null || !Unit.nest.graph.units.Any())
            {
                base.GenerateControlInternal(input, data, writer);
                return;
            }

            RebuildGraphState();

            if (data.TryGetGraphPointer(out var graphPointer))
            {
                data.SetGraphPointer(graphPointer.AsReference().ChildReference(Unit, false));
            }

            var subgraphName = GraphTraversal.GetNesterUnitName(Unit);

            if (CSharpPreviewSettings.ShouldShowSubgraphComment)
            {
                if (graphInput != null || graphOutput != null)
                    writer.Comment($"Subgraph: \"{subgraphName}\" Port({input.key})", WriteOptions.IndentedNewLineAfter);
                else
                    writer.Error($"Subgraph \"{subgraphName}\" does not have a GraphInput or GraphOutput", WriteOptions.IndentedNewLineAfter);
            }

            if (!hasExternalCode)
            {
                foreach (var variable in Unit.nest.graph.variables)
                {
#if VISUAL_SCRIPTING_1_7
                    Type type = GetCachedType(variable.typeHandle.Identification);
#else
                    Type type = variable.value != null ? variable.value.GetType() : typeof(object);
#endif
                    string name = data.AddLocalNameInScope(variable.name.LegalVariableName(), type);
                    writer.CreateVariable(type, name, variable.value.As().Code(true, true, true, "", false, true), WriteOptions.Indented, EndWriteOptions.LineEnd);
                }
            }

            int index = 0;

            foreach (CustomEvent customEvent in customEvents)
            {
                if (!customEvent.trigger.hasValidConnection)
                    continue;

                index++;
                customEventIds[customEvent] = index;

                if (!typeof(MonoBehaviour).IsAssignableFrom(data.ScriptType))
                {
                    writer.WriteErrorDiagnostic("Custom Event units only work on MonoBehaviours", "Could not generate Custom Events", WriteOptions.IndentedNewLineAfter);
                    break;
                }

                var generator = customEvent.GetGenerator();
                var methodName = GetMethodName(customEvent);

                var action = customEvent.coroutine
                    ? $"({"args".VariableHighlight()}) => StartCoroutine(" + methodName + $"({"args".VariableHighlight()}))"
                    : methodName;

                writer.WriteIndented();
                writer.CallCSharpUtilityMethod(
                    "RegisterCustomEvent",
                    writer.Action(w => generator.GenerateValue(customEvent.target, data, w)),
                    action,
                    (methodName + "_" + customEvent.ToString().Replace(".", "")).As().Code(false)
                );
                writer.WriteEnd(EndWriteOptions.LineEnd);

                var returnType = customEvent.coroutine ? typeof(IEnumerator) : typeof(void);

                writer.WriteIndented(returnType.As().CSharpName(false, true));
                writer.Write(" ");
                writer.Write(methodName);
                writer.Write($"({"CustomEventArgs".TypeHighlight()} {"args".VariableHighlight()})");
                writer.NewLine();

                writer.WriteLine("{");
                using (writer.Indented())
                {
                    writer.WriteIndented("if ".ControlHighlight() + $"({"args".VariableHighlight()}.{"name".VariableHighlight()} == ");
                    generator.GenerateValue(customEvent.name, data, writer);
                    writer.Write(")");
                    writer.NewLine();

                    writer.WriteLine("{");
                    using (writer.IndentedScope(data))
                    {
                        generator.GenerateControl(null, data, writer);
                    }
                    writer.WriteLine("}");
                }
                writer.WriteLine("}");
            }

            if (input.hasValidConnection && graphInput != null)
            {
                writer.ExitCurrentNode(unit);
                graphInput.GenerateControl(input, data, writer);
            }
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (Unit.nest == null || Unit.nest.graph == null)
            {
                writer.WriteErrorDiagnostic("Subgraph missing GraphOutput unit", "Could not Generate subgraph value.");
                return;
            }

            Unit go = Unit.nest.graph.units.FirstOrDefault(u => u is GraphOutput) as Unit;
            if (go != null)
            {
                go.GenerateValue(output, writer, data);
                return;
            }

            writer.WriteErrorDiagnostic("Subgraph missing GraphOutput unit", "Could not Generate subgraph value.");
        }

        private static Type GetCachedType(string typeId)
        {
            if (string.IsNullOrEmpty(typeId))
                return typeof(object);

            if (!typeCache.TryGetValue(typeId, out Type type))
            {
                type = Type.GetType(typeId) ?? typeof(object);
                typeCache[typeId] = type;
            }

            return type;
        }

        private string GetMethodName(CustomEvent customEvent)
        {
            if (!customEvent.name.hasValidConnection)
                return (string)customEvent.defaultValues[customEvent.name.key];

            return "CustomEvent" + (customEventIds.TryGetValue(customEvent, out int id) ? id : 0);
        }

        public IEnumerable<FieldGenerator> GetRequiredVariables(ControlGenerationData data)
        {
            if (!hasExternalCode)
                yield break;

            foreach (var variable in Unit.nest.graph.variables)
            {
#if VISUAL_SCRIPTING_1_7
                Type type = GetCachedType(variable.typeHandle.Identification);
#else
                Type type = variable.value != null ? variable.value.GetType() : typeof(object);
#endif
                yield return FieldGenerator.Field(AccessModifier.Private, FieldModifier.None, type, variable.name, variable.value);
            }
        }
    }
}