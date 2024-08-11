using Unity;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;
using System;
using System.Runtime.CompilerServices;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Unity.VisualScripting.InvokeMember))]
    public sealed class InvokeMemberGenerator : NodeGenerator<Unity.VisualScripting.InvokeMember>
    {
        private ControlGenerationData controlGenerationData;

        private Dictionary<ValueOutput, string> outputNames;

        public InvokeMemberGenerator(Unity.VisualScripting.InvokeMember unit) : base(unit)
        {
            if (Unit.member.isExtension)
            {
                NameSpace = Unit.member.info.DeclaringType.Namespace;
            }
            else
            {
                NameSpace = Unit.member.declaringType.Namespace;
            }
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if (output == Unit.result)
            {
                if (!Unit.enter.hasValidConnection && Unit.outputParameters.Count > 0)
                {
                    return $"/* Control Port Enter of {Unit.member.ToDeclarer()} requires a connection */";
                }
                var _output = string.Empty;

                if (Unit.member.isConstructor)
                {
                    _output += new ValueCode($"{"new".ConstructHighlight()} {Unit.member.pseudoDeclaringType.As().CSharpName(false, true)}({GenerateArguments(data)})");
                }
                else
                {
                    if (Unit.target == null)
                    {
                        _output += new ValueCode($"{Unit.member.pseudoDeclaringType.As().CSharpName(false, true)}.{Unit.member.name}({GenerateArguments(data)})");
                    }
                    else
                    {
                        if (Unit.target.hasValidConnection && Unit.target.type != Unit.target.connection.source.type && Unit.member.pseudoDeclaringType.IsSubclassOf(typeof(Component)))
                        {
                            _output += new ValueCode(new ValueCode(GenerateValue(Unit.target, data) + GetComponent(Unit.target) + "." + Unit.member.name + $"({GenerateArguments(data)})"));
                        }
                        else if (Unit.member.pseudoDeclaringType.IsSubclassOf(typeof(Component)))
                        {
                            _output += new ValueCode(GenerateValue(Unit.target, data) + GetComponent(Unit.target) + "." + Unit.member.name + $"({GenerateArguments(data)})", typeof(GameObject), ShouldCast(Unit.target, false, data));
                        }
                        else
                        {
                            _output += new ValueCode($"{GenerateValue(Unit.target, data)}.{Unit.member.name}({GenerateArguments(data)})");
                        }
                    }
                }
                return _output;
            }
            else if (Unit.outputParameters.ContainsValue(output))
            {
                if (!Unit.enter.hasValidConnection && Unit.outputParameters.Count > 0)
                {
                    return $"/* Control Port Enter of {Unit.member.ToDeclarer()} requires a connection */";
                }

                var transformedKey = outputNames[output].Replace("&", "").Replace("%", "");

                return transformedKey.VariableHighlight();
            }
            else if (output == Unit.targetOutput)
            {
                return GenerateValue(Unit.target, data);
            }
            return base.GenerateValue(output, data);
        }

        string GetComponent(ValueInput valueInput)
        {
            if (valueInput.hasValidConnection)
            {
                if (valueInput.type == valueInput.connection.source.type && valueInput.connection.source.unit is MemberUnit or CodeAssetUnit)
                {
                    return "";
                }
                else
                {
                    return valueInput.connection.source.unit is MemberUnit memberUnit && memberUnit.member.name != "GetComponent" ? $".GetComponent<{Unit.member.pseudoDeclaringType.As().CSharpName(false, true)}>()" : string.Empty;
                }
            }
            else
            {
                return $".GetComponent<{Unit.member.pseudoDeclaringType.As().CSharpName(false, true)}>()";
            }
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            outputNames = new Dictionary<ValueOutput, string>();
            controlGenerationData = data;
            var output = string.Empty;
            if (Unit.result == null || !Unit.result.hasValidConnection)
            {
                if (Unit.member.isConstructor)
                {
                    output += new CodeLine($"{"new".ConstructHighlight()} {Unit.member.pseudoDeclaringType.As().CSharpName(false, true)}({GenerateArguments(data)})", indent).GetCode(true);
                }
                else
                {
                    if (Unit.target == null)
                    {
                        output += CodeUtility.MakeSelectable(Unit, CodeBuilder.Indent(indent) + new CodeLine($"{Unit.member.pseudoDeclaringType.As().CSharpName(false, true)}.{Unit.member.name}({GenerateArguments(data)})").GetCode(true));
                    }
                    else
                    {
                        if (Unit.member.pseudoDeclaringType == typeof(GameObject) && Unit.target.hasValidConnection && Unit.target.connection.source.type.IsSubclassOf(typeof(Component)))
                        {
                            output += CodeUtility.MakeSelectable(Unit, CodeBuilder.Indent(indent) + new CodeLine($"{GenerateValue(Unit.target, data)}.gameObject.GetComponent<{Unit.member.pseudoDeclaringType.As().CSharpName(false, true)}>().{Unit.member.name}({GenerateArguments(data)})").GetCode(true));
                        }
                        else if (Unit.target.hasValidConnection && Unit.target.type != Unit.target.connection.source.type && Unit.member.pseudoDeclaringType.IsSubclassOf(typeof(Component)))
                        {
                            output += CodeUtility.MakeSelectable(Unit, CodeBuilder.Indent(indent) + new CodeLine(new ValueCode(GenerateValue(Unit.target, data) + GetComponent(Unit.target) + "." + Unit.member.name + $"({GenerateArguments(data)})", typeof(GameObject), ShouldCast(Unit.target))).GetCode(true));
                        }
                        else if (Unit.member.pseudoDeclaringType.IsSubclassOf(typeof(Component)))
                        {
                            output += CodeUtility.MakeSelectable(Unit, CodeBuilder.Indent(indent) + new CodeLine($"{GenerateValue(Unit.target, data)}{GetComponent(Unit.target)}.{Unit.member.name}({GenerateArguments(data)})").GetCode(true));
                        }
                        else
                        {
                            output += CodeUtility.MakeSelectable(Unit, CodeBuilder.Indent(indent) + new CodeLine($"{GenerateValue(Unit.target, data)}.{Unit.member.name}({GenerateArguments(data)})").GetCode(true));
                        }
                    }
                }
                output += GetNextUnit(Unit.exit, data, indent);
                return output;
            }
            return GetNextUnit(Unit.exit, data, indent);
        }

        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input.hasValidConnection)
            {
                if (input.type.IsSubclassOf(typeof(Component))) return new ValueCode(GetNextValueUnit(input, data), typeof(GameObject), ShouldCast(input, false, data)) + new ValueCode($"{(input.connection.source.type == typeof(GameObject) ? $".GetComponent<{input.type.As().CSharpName(false, true)}>()" : string.Empty)}");
                return new ValueCode(GetNextValueUnit(input, data), input.type, ShouldCast(input, false, data)) + new ValueCode($"{(input.type.IsSubclassOf(typeof(Component)) && input.connection.source.type == typeof(GameObject) ? $".GetComponent<{input.type.As().CSharpName(false, true)}>()" : string.Empty)}");
            }
            else if (input.hasDefaultValue)
            {
                if (input.type == typeof(GameObject) || input.type.IsSubclassOf(typeof(Component)) || input.type == typeof(Component) && input == Unit.target)
                {
                    return "gameObject".VariableHighlight();
                }
                return Unit.defaultValues[input.key].As().Code(true, true, true, "", false);
            }
            else
            {
                if (Unit.member.methodInfo.GetParameters()[Unit.inputParameters.FirstOrDefault(parameter => parameter.Value == input).Key].IsDefined(typeof(ParamArrayAttribute), true))
                {
                    return "";
                }
                return $"/* \"{input.key} Requires Input\" */";
            }
        }

        private string GenerateArguments(ControlGenerationData data = null)
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
                        if (Unit.outputParameters.Values.Any(output => output.key == "&" + parameter.Name && !outputNames.ContainsKey(Unit.outputParameters[index])))
                            outputNames.Add(Unit.outputParameters[index], "&" + name);
                    }
                    else if (parameter.ParameterType.IsByRef)
                    {
                        var name = controlGenerationData.AddLocalNameInScope(parameter.Name, parameter.ParameterType).VariableHighlight();
                        var input = Unit.inputParameters[index];
                        if (!input.hasValidConnection || input.hasValidConnection && input.connection.source.unit is not GetVariable)
                        {
                            output.Add($"/* {input.key.Replace("%", "")} needs to be connected to a variable unit or a get member unit */");
                            continue;
                        }
                        output.Add("ref ".ConstructHighlight() + GenerateValue(Unit.inputParameters[index], data));
                        outputNames.Add(Unit.outputParameters[index], "&" + name);
                    }
                    else if (parameter.IsDefined(typeof(ParamArrayAttribute), false) && !Unit.inputParameters[index].hasValidConnection)
                    {
                        continue;
                    }
                    else if (Unit.member.methodInfo.IsDefined(typeof(ExtensionAttribute), false) && index == 0)
                    {
                        output.Add(string.Empty);
                    }
                    else
                    {
                        output.Add(GenerateValue(Unit.inputParameters.Values.First(input => input.key == "%" + parameter.Name), data));
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
                    if (parameter.IsDefined(typeof(ParamArrayAttribute), false) && !Unit.inputParameters[index].hasValidConnection)
                    {
                        continue;
                    }
                    else if (Unit.member.methodInfo.IsDefined(typeof(ExtensionAttribute), false) && index == 0)
                    {
                        output.Add(string.Empty);
                    }
                    else
                        output.Add(GenerateValue(Unit.inputParameters[index], data));
                    index++;
                }
                return string.Join(", ", output);
            }
            else
            {
                List<string> output = Unit.valueInputs.Select(input => GenerateValue(input, data)).ToList();
                return string.Join(", ", output);
            }
        }
    }
}