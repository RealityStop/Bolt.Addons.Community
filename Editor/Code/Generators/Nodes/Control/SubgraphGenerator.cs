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
public class SubgraphGenerator : NodeGenerator<SubgraphUnit>
{
    public SubgraphGenerator(Unity.VisualScripting.SubgraphUnit unit) : base(unit)
    {
    }

    public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
    {
        var Units = Unit.nest.graph.units;
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

            var subgraphName = Unit.nest.graph.title?.Length > 0 ? Unit.nest.graph.title : Unit.nest.source == GraphSource.Macro ? Unit.nest.macro.name : "UnnamedSubgraph";
            if (CSharpPreview.ShowSubgraphComment)
            {
                if (_graphinput != null || _graphOutput != null)
                {
                    output += "\n" + CodeBuilder.Indent(indent) + $"//Subgraph: \"{subgraphName}\" Port({input.key}) \n".CommentHighlight();
                }
                else
                {
                    output += "\n" + CodeBuilder.Indent(indent) + $"/* Subgraph \"{subgraphName}\" is empty */ \n";
                }
            }

            foreach (var variable in Unit.nest.graph.variables)
            {
                var type = Type.GetType(variable.typeHandle.Identification) != null ? Type.GetType(variable.typeHandle.Identification) : typeof(object);
                var name = data.AddLocalNameInScope(variable.name, type);
                output += $"{CodeBuilder.Indent(indent)}{type.As().CSharpName(false, true)} {name.VariableHighlight()} = {variable.value.As().Code(true, true)};\n";
            }

            if (input.hasValidConnection)
            {
                if (_graphinput != null)
                {
                    var _output = _graphinput.controlOutputs.FirstOrDefault(output => output.key.Equals(input.key, StringComparison.OrdinalIgnoreCase));
                    output += GetNextUnit(_output, data, indent);
                }
            }

            return output;
        }
        else
            return base.GenerateControl(input, data, indent);
    }

    public override string GenerateValue(ValueOutput output, ControlGenerationData data)
    {
        if (Unit.nest.graph.units.FirstOrDefault(unit => unit is GraphOutput) is Unit graphOutput)
            return GetSingleDecorator(graphOutput, graphOutput).GenerateValue(output, data);
        else
            return "/* Subgraph missing GraphOutput unit */";
    }
}
