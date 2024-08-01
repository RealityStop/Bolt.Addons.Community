using Unity;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;
using System;

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

        public override string GenerateValue(ValueOutput output)
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
                    _output += new ValueCode($"{"new".ConstructHighlight()} {Unit.member.pseudoDeclaringType.As().CSharpName(false, true)}({GenerateArguments()})");
                }
                else
                {
                    if (Unit.target == null)
                    {
                        _output += new ValueCode($"{Unit.member.pseudoDeclaringType.As().CSharpName(false, true)}.{Unit.member.name}({GenerateArguments()})");
                    }
                    else
                    {
                        if (Unit.target.hasValidConnection && Unit.target.type != Unit.target.connection.source.type && Unit.member.pseudoDeclaringType.IsSubclassOf(typeof(Component)))
                        {
                            _output += new ValueCode(new ValueCode(GenerateValue(Unit.target) + GetComponent(Unit.target) + "." + Unit.member.name + $"({GenerateArguments()})"));
                        }
                        else if (Unit.member.pseudoDeclaringType.IsSubclassOf(typeof(Component)))
                        {
                            _output += new ValueCode(GenerateValue(Unit.target) + GetComponent(Unit.target) + "." + Unit.member.name + $"({GenerateArguments()})", typeof(GameObject), ShouldCast(Unit.target));
                        }
                        else
                        {
                            _output += new ValueCode($"{GenerateValue(Unit.target)}.{Unit.member.name}({GenerateArguments()})");
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
                return GenerateValue(Unit.target);
            }
            return base.GenerateValue(output);
        }

        string GetComponent(ValueInput valueInput)
        {
            if (valueInput.hasValidConnection)
            {
                if (valueInput.type == valueInput.connection.source.type && valueInput.connection.source.unit is MemberUnit or InheritedMemberUnit or AssetFieldUnit or AssetMethodCallUnit)
                {
                    return "";
                }
                else
                {
                    return valueInput.connection.source.unit is MemberUnit memberUnit && memberUnit.member.name != "GetComponent" ? $".GetComponent<{Unit.member.pseudoDeclaringType.As().CSharpName(false, true)}>()" : ".";
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
                    output += new CodeLine($"{"new".ConstructHighlight()} {Unit.member.pseudoDeclaringType.As().CSharpName(false, true)}({GenerateArguments()})", indent).GetCode(Unit.exit.hasValidConnection);
                }
                else
                {
                    if (Unit.target == null)
                    {
                        output += CodeUtility.MakeSelectable(Unit, CodeBuilder.Indent(indent) + new CodeLine($"{Unit.member.pseudoDeclaringType.As().CSharpName(false, true)}.{Unit.member.name}({GenerateArguments()})").GetCode(Unit.exit.hasValidConnection));
                    }
                    else
                    {
                        if (Unit.member.pseudoDeclaringType == typeof(GameObject) && Unit.target.hasValidConnection && Unit.target.connection.source.type.IsSubclassOf(typeof(Component)))
                        {
                            output += CodeUtility.MakeSelectable(Unit, CodeBuilder.Indent(indent) + new CodeLine($"{GenerateValue(Unit.target)}.gameObject.GetComponent<{Unit.member.pseudoDeclaringType.As().CSharpName(false, true)}>().{Unit.member.name}({GenerateArguments()})").GetCode(Unit.exit.hasValidConnection));
                        }
                        else if (Unit.target.hasValidConnection && Unit.target.type != Unit.target.connection.source.type && Unit.member.pseudoDeclaringType.IsSubclassOf(typeof(Component)))
                        {
                            output += CodeUtility.MakeSelectable(Unit, CodeBuilder.Indent(indent) + new CodeLine(new ValueCode(GenerateValue(Unit.target) + GetComponent(Unit.target) + "." + Unit.member.name + $"({GenerateArguments()})", typeof(GameObject), ShouldCast(Unit.target))).GetCode(Unit.exit.hasValidConnection));
                        }
                        else if (Unit.member.pseudoDeclaringType.IsSubclassOf(typeof(Component)))
                        {
                            output += CodeUtility.MakeSelectable(Unit, CodeBuilder.Indent(indent) + new CodeLine($"{GenerateValue(Unit.target)}{GetComponent(Unit.target)}.{Unit.member.name}({GenerateArguments()})").GetCode(Unit.exit.hasValidConnection));
                        }
                        else
                        {
                            output += CodeUtility.MakeSelectable(Unit, CodeBuilder.Indent(indent) + new CodeLine($"{GenerateValue(Unit.target)}.{Unit.member.name}({GenerateArguments()})").GetCode(Unit.exit.hasValidConnection));
                        }
                    }
                }
                output += GetNextUnit(Unit.exit, data, indent);
                return output;
            }
            return GetNextUnit(Unit.exit, data, indent);
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                if (input.type.IsSubclassOf(typeof(Component))) return new ValueCode(GetNextValueUnit(input), typeof(GameObject), ShouldCast(input)) + new ValueCode($"{(input.connection.source.type == typeof(GameObject) ? $".GetComponent<{input.type.As().CSharpName(false, true)}>()" : string.Empty)}");
                return new ValueCode(GetNextValueUnit(input), input.type, ShouldCast(input)) + new ValueCode($"{(input.type.IsSubclassOf(typeof(Component)) && input.connection.source.type == typeof(GameObject) ? $".GetComponent<{input.type.As().CSharpName(false, true)}>()" : string.Empty)}");
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
                if (Unit.member.methodInfo.GetParameters()[Unit.inputParameters.First(parameter => parameter.Value == input).Key].IsDefined(typeof(ParamArrayAttribute), false))
                {
                    return "";
                }
                return $"/* \"{input.key} Requires Input\" */";
            }
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
                        if (Unit.outputParameters.Values.Any(output => output.key == "&" + parameter.Name && !outputNames.ContainsKey(Unit.outputParameters[index])))
                            outputNames.Add(Unit.outputParameters[index], "&" + name);
                    }
                    else if (parameter.ParameterType.IsByRef)
                    {
                        var input = Unit.inputParameters[index];
                        if (!input.hasValidConnection || input.hasValidConnection && input.connection.source.unit is not GetVariable)
                        {
                            output.Add($"/* {input.key.Replace("%", "")} needs to be connected to a variable unit or a get member unit */");
                            continue;
                        }
                        output.Add("ref ".ConstructHighlight() + GenerateValue(Unit.inputParameters[index]));
                        outputNames.Add(Unit.outputParameters[index], "&" + name);
                    }
                    else if (parameter.IsDefined(typeof(ParamArrayAttribute), false) && !Unit.inputParameters[index].hasValidConnection)
                    {
                        continue;
                    }
                    else
                    {
                        output.Add(GenerateValue(Unit.inputParameters.Values.First(input => input.key == "%" + parameter.Name)));
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
                    else
                    {
                        output.Add(GenerateValue(Unit.inputParameters[index]));
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