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
            output += CodeUtility.MakeSelectable(Unit, CodeBuilder.Indent(indent) + thisKeyword + Unit.member.name + $"({GenerateArguments()});") + (Unit.exit.hasValidConnection ? "\n" : string.Empty);
            output += GetNextUnit(Unit.exit, data, indent);
            return output;
        }
    
        public override string GenerateValue(ValueOutput output)
        {
            if (Unit.enter != null && !Unit.enter.hasValidConnection && Unit.OutputParameters.Count > 0)
            {
                return $"/* Control Port Enter requires a connection */";
            }
    
            if (Unit.OutputParameters.ContainsValue(output))
            {
                var transformedKey = outputNames[output].Replace("&", "").Replace("%", "");
    
                return transformedKey.VariableHighlight();
            }
            var thisKeyword = Unit.showThisKeyword ? "this".ConstructHighlight() + "." : string.Empty;
            return CodeUtility.MakeSelectable(Unit, thisKeyword + Unit.member.name + $"({GenerateArguments()})");
        }
    
    
        private string GenerateArguments()
        {
            if (controlGenerationData != null && Unit.member.isMethod)
            {
                List<string> output = new List<string>();
                var index = 0;
                foreach (var parameter in Unit.member.methodInfo.GetParameters())
                {
                    var name = controlGenerationData.AddLocalNameInScope(parameter.Name).VariableHighlight();
                    if (parameter.HasOutModifier())
                    {
                        output.Add("out var ".ConstructHighlight() + name);
                        if (Unit.OutputParameters.Values.Any(output => output.key == "&" + parameter.Name && !outputNames.ContainsKey(Unit.OutputParameters[index])))
                            outputNames.Add(Unit.OutputParameters[index], "&" + name);
                    }
                    else if (parameter.ParameterType.IsByRef)
                    {
                        var input = Unit.InputParameters[index];
                        if (!input.hasValidConnection || input.hasValidConnection && input.connection.source.unit is not GetVariable)
                        {
                            output.Add($"/* {input.key.Replace("%", "")} needs to be connected to a variable unit or a get member unit */");
                            continue;
                        }
                        output.Add("ref ".ConstructHighlight() + GenerateValue(Unit.InputParameters[index]));
                        outputNames.Add(Unit.OutputParameters[index], "&" + name);
                    }
                    else if (parameter.IsDefined(typeof(ParamArrayAttribute), false) && !Unit.InputParameters[index].hasValidConnection)
                    {
                        continue;
                    }
                    else
                    {
                        output.Add(GenerateValue(Unit.InputParameters.Values.First(input => input.key == "%" + parameter.Name)));
                    }
                    index++;
                }
                return string.Join(", ", output);
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
                        output.Add(GenerateValue(Unit.InputParameters[index]));
                    }
                    index++;
                }
                return string.Join(", ", output);
            }
            else
            {
                List<string> output = Unit.valueInputs.Select(input => GenerateValue(input)).ToList();
                return string.Join(", ", output);
            }
        }
    }
}