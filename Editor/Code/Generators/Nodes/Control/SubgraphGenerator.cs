using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

[NodeGenerator(typeof(SubgraphUnit))]
[RequiresVariables]
public class SubgraphGenerator : NodeGenerator<SubgraphUnit>, IRequireVariables
{
<<<<<<< Updated upstream
    public SubgraphGenerator(Unity.VisualScripting.SubgraphUnit unit) : base(unit)
=======
    private static readonly Dictionary<string, Type> typeCache = new();
    private readonly Dictionary<CustomEvent, int> customEventIds = new();
    private Unit graphInput;
    private Unit graphOutput;
    HashSet<CustomEvent> customEvents = new HashSet<CustomEvent>();

    bool hasEventUnit;
    public SubgraphGenerator(SubgraphUnit unit) : base(unit)
>>>>>>> Stashed changes
    {
    }

<<<<<<< Updated upstream
    private Dictionary<CustomEvent, int> customEventIds = new Dictionary<CustomEvent, int>();
=======
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
>>>>>>> Stashed changes

    public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
    {
        if (data.TryGetGraphPointer(out var graphPointer))
        {
            data.SetGraphPointer(graphPointer.AsReference().ChildReference(Unit, false));
        }
        var Units = Unit.nest.graph.units;
        var customEvents = Units.Where(unit => unit is CustomEvent);
        var _graphinput = Units.FirstOrDefault(unit => unit is GraphInput) as Unit;
        var _graphOutput = Units.FirstOrDefault(unit => unit is GraphOutput) as Unit;
        if (Unit.nest != null && Unit.nest.graph != null)
        {
            if (Units.Any(unit => unit is GraphInput or GraphOutput))
            {
                GetSingleDecorator(_graphinput, _graphinput).connectedValueInputs.Clear();
                foreach (var item in Unit.valueInputs)
                {
                    if ((item.hasDefaultValue || item.hasValidConnection) && !GetSingleDecorator(_graphinput, _graphinput).connectedValueInputs.Contains(item))
                    {
                        GetSingleDecorator(_graphinput, _graphinput).connectedValueInputs.Add(item);
                    }
                }
                GetSingleDecorator(_graphOutput, _graphOutput).connectedGraphOutputs.Clear();
                foreach (var connectedOutput in Unit.controlOutputs.Where(output => output.hasValidConnection))
                {
                    if (!GetSingleDecorator(_graphOutput, _graphOutput).connectedGraphOutputs.Contains(connectedOutput))
                        GetSingleDecorator(_graphOutput, _graphOutput).connectedGraphOutputs.Add(connectedOutput);
                }
            }
            var output = string.Empty;

<<<<<<< Updated upstream
            var subgraphName = Unit.nest.graph.title?.Length > 0 ? Unit.nest.graph.title : Unit.nest.source == GraphSource.Macro ? Unit.nest.macro.name : "UnnamedSubgraph";
            if (CSharpPreviewSettings.ShouldShowSubgraphComment)
            {
                if (_graphinput != null || _graphOutput != null)
                {
                    output += "\n" + CodeBuilder.Indent(indent) + $"//Subgraph: \"{subgraphName}\" Port({input.key}) \n".CommentHighlight();
                }
                else
                {
                    output += "\n" + CodeBuilder.Indent(indent) + $"/* Subgraph \"{subgraphName}\" is empty */ \n".WarningHighlight();
                }
=======
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
>>>>>>> Stashed changes
            }

            output += "\n";

<<<<<<< Updated upstream
            foreach (var variable in Unit.nest.graph.variables)
=======
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
>>>>>>> Stashed changes
            {
                var type = Type.GetType(variable.typeHandle.Identification) ?? typeof(object);
                var name = data.AddLocalNameInScope(variable.name, type);
                output += CodeBuilder.Indent(indent) + MakeSelectableForThisUnit($"{type.As().CSharpName(false, true)} {name.VariableHighlight()} = ") + variable.value.As().Code(true, Unit, true, true, "", false, true) + MakeSelectableForThisUnit(";") + "\n";
            }

            var index = 0;
            foreach (CustomEvent customEvent in customEvents)
            {
                index++;
                customEventIds[customEvent] = index;
                if (!typeof(MonoBehaviour).IsAssignableFrom(data.ScriptType))
                {
                    output += CodeUtility.ToolTip("/* Custom Event units only work on monobehaviours */", "Could not generate Custom Events", "");
                    break;
                }
                var generator = GetSingleDecorator(customEvent, customEvent);
                var action = CodeUtility.MakeSelectable(customEvent, customEvent.coroutine ? $"({"args".VariableHighlight()}) => StartCoroutine({GetMethodName(customEvent)}({"args".VariableHighlight()}))" : GetMethodName(customEvent));
                output += CodeBuilder.Indent(indent) + CodeBuilder.CallCSharpUtilityMethod(customEvent, CodeUtility.MakeSelectable(customEvent, nameof(CSharpUtility.RegisterCustomEvent)), generator.GenerateValue(customEvent.target, data), action) + CodeUtility.MakeSelectable(customEvent, ";") + "\n";
                var returnType = customEvent.coroutine ? typeof(IEnumerator) : typeof(void);
                output += CodeBuilder.Indent(indent) + CodeUtility.MakeSelectable(customEvent, returnType.As().CSharpName(false, true) + " " + GetMethodName(customEvent) + "(" + "CustomEventArgs ".TypeHighlight() + "args".VariableHighlight() + ")") + "\n";
                output += CodeBuilder.Indent(indent) + CodeUtility.MakeSelectable(customEvent, "{") + "\n";
                output += CodeBuilder.Indent(indent + 1) + CodeUtility.MakeSelectable(customEvent, "if".ControlHighlight() + $" ({"args".VariableHighlight()}.name == ") + generator.GenerateValue(customEvent.name, data) + CodeUtility.MakeSelectable(customEvent, ")") + "\n";
                output += CodeBuilder.Indent(indent + 1) + CodeUtility.MakeSelectable(customEvent, "{") + "\n";
                var customEventData = new ControlGenerationData(data)
                {
                    returns = returnType
                };
                output += GetNextUnit(customEvent.trigger, customEventData, indent + 2);
                output += "\n" + CodeBuilder.Indent(indent + 1) + CodeUtility.MakeSelectable(customEvent, "}") + "\n";
                output += "\n" + CodeBuilder.Indent(indent) + CodeUtility.MakeSelectable(customEvent, "}") + "\n";
            }

            if (input.hasValidConnection)
            {
                if (_graphinput != null)
                {
                    var _output = _graphinput.controlOutputs.FirstOrDefault(output => output.key.Equals(input.key, StringComparison.OrdinalIgnoreCase));
                    output += GetNextUnit(_output, data, indent);
                }
            }
            if (data.TryGetGraphPointer(out var _graphPointer))
            {
                data.SetGraphPointer(graphPointer.AsReference().ParentReference(false));
            }
            return output;
        }
        else
            return base.GenerateControl(input, data, indent);
    }

    private string GetMethodName(CustomEvent customEvent)
    {
        if (!customEvent.name.hasValidConnection)
        {
            return (string)customEvent.defaultValues[customEvent.name.key];
        }
<<<<<<< Updated upstream
        else
=======
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
>>>>>>> Stashed changes
        {
            return "CustomEvent" + customEventIds[customEvent];
        }
    }

    public override string GenerateValue(ValueOutput output, ControlGenerationData data)
    {
        if (Unit.nest.graph.units.FirstOrDefault(unit => unit is GraphOutput) is Unit graphOutput)
            return GetSingleDecorator(graphOutput, graphOutput).GenerateValue(output, data);
        else
            return "/* Subgraph missing GraphOutput unit */".WarningHighlight();
    }
}
