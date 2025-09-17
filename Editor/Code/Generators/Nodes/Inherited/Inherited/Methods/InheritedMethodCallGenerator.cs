using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

namespace Unity.VisualScripting.Community
{

    [NodeGenerator(typeof(InheritedMethodCall))]
    public class InheritedMethodCallGenerator : NodeGenerator<InheritedMethodCall>
    {
        private ControlGenerationData controlGenerationData;

        private Dictionary<ValueOutput, string> outputNames;
        public InheritedMethodCallGenerator(Unit unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;
            controlGenerationData = data;
            var thisKeyword = Unit.showThisKeyword ? "this".ConstructHighlight() + "." : string.Empty;
            output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit(thisKeyword + Unit.member.name + "(") + $"{GenerateArguments(data)}{MakeClickableForThisUnit(");")}" + "\n";
            output += GetNextUnit(Unit.exit, data, indent);
            return output;
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if (Unit.enter != null && !Unit.enter.hasValidConnection && Unit.OutputParameters.Count > 0)
            {
                return $"/* Control Port Enter requires a connection */".WarningHighlight();
            }

            if (Unit.OutputParameters.ContainsValue(output))
            {
                var transformedKey = outputNames[output].Replace("&", "").Replace("%", "");

                return MakeClickableForThisUnit(transformedKey.VariableHighlight());
            }
            var thisKeyword = Unit.showThisKeyword ? "this".ConstructHighlight() + "." : string.Empty;
            return MakeClickableForThisUnit(thisKeyword + Unit.member.name + "(") + GenerateArguments(data) + MakeClickableForThisUnit(")");
        }


        private string GenerateArguments(ControlGenerationData data)
        {
            if (controlGenerationData != null && Unit.member.isMethod)
            {
                List<string> output = new List<string>();
                var index = 0;
                foreach (var parameter in Unit.member.methodInfo.GetParameters())
                {
                    if (parameter.HasOutModifier())
                    {
                        var name = controlGenerationData.AddLocalNameInScope(parameter.Name, parameter.ParameterType).VariableHighlight();
                        output.Add("out var ".ConstructHighlight() + name);
                        if (Unit.OutputParameters.Values.Any(output => output.key == "&" + parameter.Name && !outputNames.ContainsKey(Unit.OutputParameters[index])))
                            outputNames.Add(Unit.OutputParameters[index], "&" + name);
                    }
                    else if (parameter.ParameterType.IsByRef)
                    {
                        var input = Unit.InputParameters[index];
                        if (!input.hasValidConnection || input.hasValidConnection && input.connection.source.unit is not GetVariable)
                        {
                            output.Add($"/* {input.key.Replace("%", "")} needs to be connected to a variable unit or a get member unit */".WarningHighlight());
                            continue;
                        }
                        output.Add("ref ".ConstructHighlight() + GenerateValue(Unit.InputParameters[index], data));
                        outputNames.Add(Unit.OutputParameters[index], "&" + parameter.Name);
                    }
                    else if (parameter.IsDefined(typeof(ParamArrayAttribute), false) && !Unit.InputParameters[index].hasValidConnection)
                    {
                        continue;
                    }
                    else
                    {
                        output.Add(GenerateValue(Unit.InputParameters.Values.First(input => input.key == "%" + parameter.Name), data));
                    }
                    index++;
                }
                return string.Join(MakeClickableForThisUnit(", "), output);
            }
            else if (Unit.member.isMethod)
            {
                List<string> output = new List<string>();
                var index = 0;
                foreach (var parameter in Unit.member.methodInfo.GetParameters())
                {
                    if (parameter.IsDefined(typeof(ParamArrayAttribute), false) && !Unit.InputParameters[index].hasValidConnection)
                    {
                        continue;
                    }
                    else
                    {
                        output.Add(GenerateValue(Unit.InputParameters[index], data));
                    }
                    index++;
                }
                return string.Join(MakeClickableForThisUnit(", "), output);
            }
            else
            {
                List<string> output = Unit.valueInputs.Select(input => GenerateValue(input, data)).ToList();
                return string.Join(MakeClickableForThisUnit(", "), output);
            }
        }
    }
}