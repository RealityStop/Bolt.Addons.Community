using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

namespace Unity.VisualScripting.Community
{

    [NodeGenerator(typeof(AssetMethodCallUnit))]
    public class AssetMethodCallUnitGenerator : NodeGenerator<AssetMethodCallUnit>
    {
        private ControlGenerationData controlGenerationData;

        private Dictionary<ValueOutput, string> outputNames;
        public AssetMethodCallUnitGenerator(Unit unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;
            controlGenerationData = data;
            output += MakeSelectableForThisUnit(Unit.method.methodName + "(") + GenerateArguments(Unit.InputParameters.Values.ToList(), data) + MakeSelectableForThisUnit(";");
            output += "\n" + GetNextUnit(Unit.exit, data, indent);
            return output;
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {

            if (Unit.enter != null && !Unit.enter.hasValidConnection && Unit.OutputParameters.Count > 0)
            {
                return $"/* Control Port Enter requires a connection */";
            }

            if (Unit.OutputParameters.ContainsValue(output))
            {

                var transformedKey = outputNames[output].Replace("&", "").Replace("%", "");

                return MakeSelectableForThisUnit(transformedKey.VariableHighlight());
            }

            return MakeSelectableForThisUnit(Unit.method.methodName + "(") + GenerateArguments(Unit.InputParameters.Values.ToList(), data) + MakeSelectableForThisUnit(")");
        }

        private string GenerateArguments(List<ValueInput> arguments, ControlGenerationData data)
        {
            if (controlGenerationData != null)
            {
                List<string> output = new List<string>();
                var index = 0;
                foreach (var parameter in Unit.method.parameters)
                {
                    var name = controlGenerationData.AddLocalNameInScope(parameter.name).VariableHighlight();
                    if (parameter.modifier == ParameterModifier.Out)
                    {
                        output.Add("out var ".ConstructHighlight() + name);
                        if (Unit.OutputParameters.Values.Any(output => output.key == "&" + parameter.name && !outputNames.ContainsKey(Unit.OutputParameters[index])))
                            outputNames.Add(Unit.OutputParameters.Values.First(output => output.key == "&" + parameter.name), "&" + name);
                    }
                    else if (parameter.modifier == ParameterModifier.Ref)
                    {
                        var input = Unit.InputParameters.Values.First(value => value.key == "%" + parameter.name);
                        if (!input.hasValidConnection || input.hasValidConnection && input.connection.source.unit is not GetVariable)
                        {
                            output.Add($"/* {input.key.Replace("%", "")} needs to be connected to a variable unit or a get member unit */");
                            continue;
                        }
                        output.Add("ref ".ConstructHighlight() + GenerateValue(input, data));
                        if (Unit.OutputParameters.Values.Any(output => output.key == "&" + parameter.name && !outputNames.ContainsKey(Unit.OutputParameters[index])))
                            outputNames.Add(Unit.OutputParameters[index], "&" + name);
                    }
                    else
                    {
                        output.Add(GenerateValue(Unit.InputParameters[index], data));
                    }
                    index++;
                }
                return string.Join(MakeSelectableForThisUnit(", "), output);
            }
            else
            {
                List<string> output = arguments.Select(arg => GenerateValue(arg, data)).ToList();
                return string.Join(MakeSelectableForThisUnit(", "), output);
            }
        }
    }
}