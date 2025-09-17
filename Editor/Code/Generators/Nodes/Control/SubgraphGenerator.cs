using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

[NodeGenerator(typeof(SubgraphUnit))]
[RequiresVariables]
public class SubgraphGenerator : NodeGenerator<SubgraphUnit>, IRequireVariables
{
    private static readonly Dictionary<string, Type> typeCache = new();
    private readonly Dictionary<CustomEvent, int> customEventIds = new();
    private Unit graphInput;
    private Unit graphOutput;
    HashSet<CustomEvent> customEvents = new HashSet<CustomEvent>();

    bool hasEventUnit;
    public SubgraphGenerator(SubgraphUnit unit) : base(unit)
    {
        SubscribeToGraphChanges();
        InitializeInputOutputConnections();
    }

    private void SubscribeToGraphChanges()
    {
        if (Unit.nest?.graph?.units != null)
        {
            Unit.nest.graph.units.CollectionChanged += OnUnitsChanged;
        }
    }

    private void OnUnitsChanged()
    {
        InitializeInputOutputConnections();
    }


    private void InitializeInputOutputConnections()
    {
        var units = Unit.nest.graph.units;
        foreach (var unit in units)
        {
            if (graphInput == null && unit is GraphInput gi)
            {
                graphInput = gi;
                continue;
            }

            if (graphOutput == null && unit is GraphOutput go)
            {
                graphOutput = go;
                continue;
            }

            if (unit is IEventUnit)
            {
                hasEventUnit = true;
                if (unit is CustomEvent ce)
                {
                    customEvents.Add(ce);
                }
            }
        }

        if (graphInput != null)
        {
            var inputGen = graphInput.GetGenerator();
            if (inputGen != null)
            {
                inputGen.connectedValueInputs.Clear();
                foreach (var input in Unit.valueInputs)
                {
                    if ((input.hasDefaultValue || input.hasValidConnection) && !inputGen.connectedValueInputs.Contains(input))
                    {
                        inputGen.connectedValueInputs.Add(input);
                    }
                }
            }
        }

        if (graphOutput != null)
        {
            var outputGen = graphOutput.GetGenerator();
            if (outputGen != null)
            {
                outputGen.connectedGraphOutputs.Clear();
                foreach (var output in Unit.controlOutputs.Where(o => o.hasValidConnection))
                {
                    if (!outputGen.connectedGraphOutputs.Contains(output))
                    {
                        outputGen.connectedGraphOutputs.Add(output);
                    }
                }
            }
        }
    }

    public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
    {
        if (!Unit.nest?.graph?.units?.Any() ?? true)
            return base.GenerateControl(input, data, indent);

        if (data.TryGetGraphPointer(out var graphPointer))
        {
            data.SetGraphPointer(graphPointer.AsReference().ChildReference(Unit, false));
        }

        var sb = new StringBuilder();
        if (graphOutput != null)
        {
            var outputGen = graphOutput.GetGenerator();
            if (outputGen != null)
            {
                outputGen.connectedGraphOutputs.Clear();
                foreach (var output in Unit.controlOutputs.Where(o => o.hasValidConnection))
                {
                    if (!outputGen.connectedGraphOutputs.Contains(output))
                    {
                        outputGen.connectedGraphOutputs.Add(output);
                    }
                }
            }
        }
        if (graphInput != null)
        {
            var inputGen = graphInput.GetGenerator();
            if (inputGen != null)
            {
                inputGen.connectedValueInputs.Clear();
                foreach (var _input in Unit.valueInputs)
                {
                    if ((_input.hasDefaultValue || _input.hasValidConnection) && !inputGen.connectedValueInputs.Contains(_input))
                    {
                        inputGen.connectedValueInputs.Add(_input);
                    }
                }
            }
        }

        var subgraphName = Unit.nest.graph.title?.Trim();

        if (string.IsNullOrWhiteSpace(subgraphName))
        {
            subgraphName = Unit.nest.source == GraphSource.Macro ? Unit.nest.macro.name : "UnnamedSubgraph";
        }

        if (CSharpPreviewSettings.ShouldShowSubgraphComment)
        {
            var comment = (graphInput != null || graphOutput != null)
                ? $"//Subgraph: \"{subgraphName}\" Port({input.key})".CommentHighlight()
                : $"/* Subgraph \"{subgraphName}\" is empty */".WarningHighlight();

            sb.AppendLine(CodeBuilder.Indent(indent) + MakeClickableForThisUnit(comment));
        }

        if (!hasEventUnit)
        {
            foreach (var variable in Unit.nest.graph.variables)
            {
                var type = GetCachedType(variable.typeHandle.Identification);
                var name = data.AddLocalNameInScope(variable.name.LegalMemberName(), type);
                sb.Append(CodeBuilder.Indent(indent));
                sb.Append(MakeClickableForThisUnit($"{type.As().CSharpName(false, true)} {name.VariableHighlight()} = "));
                sb.Append(variable.value.As().Code(true, Unit, true, true, "", false, true));
                sb.AppendLine(MakeClickableForThisUnit(";"));
            }
        }

        int index = 0;
        if (customEvents.Count > 0) NameSpaces = "Unity.VisualScripting.Community";
        foreach (var customEvent in customEvents)
        {
            if (!customEvent.trigger.hasValidConnection) continue;
            index++;
            customEventIds[customEvent] = index;

            if (!typeof(MonoBehaviour).IsAssignableFrom(data.ScriptType))
            {
                sb.AppendLine(CodeBuilder.Indent(indent) + MakeClickableForThisUnit(CodeUtility.ToolTip("/* Custom Event units only work on monobehaviours */", "Could not generate Custom Events", "")));
                break;
            }

            var generator = customEvent.GetGenerator();
            var methodName = GetMethodName(customEvent);
            var action = customEvent.coroutine
                ? $"({"args".VariableHighlight()}) => StartCoroutine({methodName}({"args".VariableHighlight()}))"
                : methodName;

            sb.AppendLine(CodeBuilder.Indent(indent) +
                          CodeBuilder.CallCSharpUtilityMethod(customEvent, CodeUtility.MakeClickable(customEvent, nameof(CSharpUtility.RegisterCustomEvent)),
                          generator.GenerateValue(customEvent.target, data), CodeUtility.MakeClickable(customEvent, action), CodeUtility.MakeClickable(customEvent, (action + "_" + customEvent.ToString().Replace(".", "")).As().Code(false))) +
                          CodeUtility.MakeClickable(customEvent, ";"));

            var returnType = customEvent.coroutine ? typeof(IEnumerator) : typeof(void);
            sb.AppendLine(CodeBuilder.Indent(indent) + CodeUtility.MakeClickable(customEvent,
                $"{returnType.As().CSharpName(false, true)} {methodName}({"CustomEventArgs".TypeHighlight()} {"args".VariableHighlight()})"));
            sb.AppendLine(CodeBuilder.Indent(indent) + CodeUtility.MakeClickable(customEvent, "{"));
            sb.AppendLine(CodeBuilder.Indent(indent + 1) + CodeUtility.MakeClickable(customEvent,
                $"{"if".ControlHighlight()} ({"args".VariableHighlight()}.{"name".VariableHighlight()} == ") +
                generator.GenerateValue(customEvent.name, data) + CodeUtility.MakeClickable(customEvent, ")"));
            sb.AppendLine(CodeBuilder.Indent(indent + 1) + CodeUtility.MakeClickable(customEvent, "{"));
            data.NewScope();
            sb.Append(GetNextUnit(customEvent.trigger, data, indent + 2));
            data.ExitScope();
            sb.AppendLine(CodeBuilder.Indent(indent + 1) + CodeUtility.MakeClickable(customEvent, "}"));
            sb.AppendLine(CodeBuilder.Indent(indent) + CodeUtility.MakeClickable(customEvent, "}"));
        }
        if (input.hasValidConnection && graphInput != null)
        {
            var matchingOutput = graphInput.controlOutputs.FirstOrDefault(o => o.key.Equals(input.key, StringComparison.OrdinalIgnoreCase));
            if (matchingOutput != null)
            {
                sb.Append(GetNextUnit(matchingOutput, data, indent));
            }
        }

        if (data.TryGetGraphPointer(out var _))
        {
            data.SetGraphPointer(graphPointer.AsReference().ParentReference(false));
        }

        return sb.ToString();
    }

    public override string GenerateValue(ValueOutput output, ControlGenerationData data)
    {
        var graphOutput = Unit.nest.graph.units.FirstOrDefault(u => u is GraphOutput) as Unit;
        if (graphOutput != null)
        {
            return graphOutput.GetGenerator().GenerateValue(output, data);
        }

        return "/* Subgraph missing GraphOutput unit */".WarningHighlight();
    }

    private static Type GetCachedType(string typeId)
    {
        if (!typeCache.TryGetValue(typeId, out var type))
        {
            type = Type.GetType(typeId) ?? typeof(object);
            typeCache[typeId] = type;
        }
        return type;
    }

    private string GetMethodName(CustomEvent customEvent)
    {
        if (!customEvent.name.hasValidConnection)
        {
            return (string)customEvent.defaultValues[customEvent.name.key];
        }
        return "CustomEvent" + (customEventIds.TryGetValue(customEvent, out var id) ? id : 0);
    }

    /// <summary>
    /// We do this to ensure the graph variables will still be able to be accessed across scopes
    /// </summary>
    public IEnumerable<FieldGenerator> GetRequiredVariables(ControlGenerationData data)
    {
        if (hasEventUnit)
        {
            foreach (var variable in Unit.nest.graph.variables)
            {
                var type = GetCachedType(variable.typeHandle.Identification);
                var name = data.AddLocalNameInScope(variable.name.LegalMemberName(), type);
                var field = FieldGenerator.Field(AccessModifier.None, FieldModifier.None, type, name, variable.value);
                yield return field;
            }
        }
    }

    ~SubgraphGenerator()
    {
        if (Unit.nest?.graph?.units != null)
        {
            Unit.nest.graph.units.CollectionChanged -= OnUnitsChanged;
        }
    }
}