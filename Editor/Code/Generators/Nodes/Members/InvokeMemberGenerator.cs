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
                NameSpaces = Unit.member.info.DeclaringType.Namespace;
            }
            else
            {
                NameSpaces = Unit.member.declaringType.Namespace;
            }
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if (output == Unit.result)
            {
                if (!Unit.enter.hasValidConnection && Unit.outputParameters.Count > 0)
                {
                    return $"/* Control Port Enter of {Unit.member.ToDeclarer()} requires a connection */".WarningHighlight();
                }
                var _output = string.Empty;

                if (Unit.member.isConstructor)
                {
                    _output += new ValueCode(MakeClickableForThisUnit($"{"new".ConstructHighlight()} {Unit.member.pseudoDeclaringType.As().CSharpName(false, true)}(") + $"{GenerateArguments(data)}{MakeClickableForThisUnit(")")}");
                }
                else
                {
                    if (Unit.target == null)
                    {
                        _output += new ValueCode(MakeClickableForThisUnit($"{Unit.member.pseudoDeclaringType.As().CSharpName(false, true)}.{Unit.member.name}(") + $"{GenerateArguments(data)}{MakeClickableForThisUnit(")")}");
                    }
                    else
                    {
                        if (Unit.target.hasValidConnection && Unit.target.type != Unit.target.connection.source.type && Unit.member.pseudoDeclaringType.IsSubclassOf(typeof(Component)))
                        {
                            _output += new ValueCode(GenerateValue(Unit.target, data) + MakeClickableForThisUnit(GetComponent(Unit.target, data) + "." + Unit.member.name + $"(") + $"{GenerateArguments(data)}{MakeClickableForThisUnit(")")}");
                        }
                        else if (Unit.member.pseudoDeclaringType.IsSubclassOf(typeof(Component)))
                        {
                            _output += new ValueCode(GenerateValue(Unit.target, data) + MakeClickableForThisUnit(GetComponent(Unit.target, data) + "." + Unit.member.name + $"(") + $"{GenerateArguments(data)}{MakeClickableForThisUnit(")")}", typeof(GameObject), ShouldCast(Unit.target, data, false));
                        }
                        else
                        {
                            _output += new ValueCode($"{GenerateValue(Unit.target, data)}" + MakeClickableForThisUnit($".{Unit.member.name}(") + $"{GenerateArguments(data)}{MakeClickableForThisUnit(")")}");
                        }
                    }
                }
                return _output;
            }
            else if (Unit.outputParameters.ContainsValue(output))
            {
                if (!Unit.enter.hasValidConnection && Unit.outputParameters.Count > 0)
                {
                    return $"/* Control Port Enter of {Unit.member.ToDeclarer()} requires a connection */".WarningHighlight();
                }

                var transformedKey = outputNames[output].Replace("&", "").Replace("%", "");

                return MakeClickableForThisUnit(transformedKey.VariableHighlight());
            }
            else if (output == Unit.targetOutput)
            {
                return GenerateValue(Unit.target, data);
            }
            return base.GenerateValue(output, data);
        }

        string GetComponent(ValueInput valueInput, ControlGenerationData data)
        {
            if (valueInput.hasValidConnection)
            {
                if (valueInput.type == valueInput.connection.source.type && valueInput.connection.source.unit is MemberUnit or CodeAssetUnit)
                {
                    return string.Empty;
                }
                else
                {
                    return ((valueInput.connection.source.unit is MemberUnit memberUnit && memberUnit.member.name != "GetComponent") || GetSourceType(valueInput, data) == typeof(GameObject)) && Unit.member.pseudoDeclaringType != typeof(GameObject) ? $".GetComponent<{Unit.member.pseudoDeclaringType.As().CSharpName(false, true)}>()" : string.Empty;
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
                    output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit($"{"new".ConstructHighlight()} {Unit.member.pseudoDeclaringType.As().CSharpName(false, true)}(") + $"{GenerateArguments(data)}{MakeClickableForThisUnit(");")}" + "\n";
                }
                else
                {
                    if (Unit.target == null)
                    {
                        output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit($"{Unit.member.pseudoDeclaringType.As().CSharpName(false, true)}.{Unit.member.name}(") + $"{GenerateArguments(data)}{MakeClickableForThisUnit(");")}" + "\n";
                    }
                    else
                    {

                        var target = GenerateValue(Unit.target, data);
                        if (Unit.member.pseudoDeclaringType == typeof(GameObject) && Unit.target.hasValidConnection && typeof(Component).IsAssignableFrom(GetSourceType(Unit.target, data) ?? Unit.target.connection.source.type))
                        {
                            output += CodeBuilder.Indent(indent) + target + (typeof(Component).IsAssignableFrom(Unit.target.type) && Unit.target.type != typeof(object) ? MakeClickableForThisUnit($".{"gameObject".VariableHighlight()}.GetComponent<{(GetSourceType(Unit.target, data) ?? Unit.target.connection.source.type).As().CSharpName(false, true)}>().{Unit.member.name}(") : MakeClickableForThisUnit($".{"gameObject".VariableHighlight()}.{Unit.member.name}(")) + $"{GenerateArguments(data)}{MakeClickableForThisUnit(");")}" + "\n";
                        }
                        else if (Unit.target.hasValidConnection && Unit.target.type != Unit.target.connection.source.type && typeof(Component).IsAssignableFrom(Unit.member.pseudoDeclaringType))
                        {
                            output += CodeBuilder.Indent(indent) + target + MakeClickableForThisUnit($"{GetComponent(Unit.target, data)}.{Unit.member.name}(") + GenerateArguments(data) + MakeClickableForThisUnit(");") + "\n";
                        }
                        else if (typeof(Component).IsAssignableFrom(Unit.member.pseudoDeclaringType))
                        {
                            output += CodeBuilder.Indent(indent) + target + MakeClickableForThisUnit($"{GetComponent(Unit.target, data)}.{Unit.member.name}(") + GenerateArguments(data) + MakeClickableForThisUnit(");") + "\n";
                        }
                        else
                        {
                            output += CodeBuilder.Indent(indent) + target + MakeClickableForThisUnit($".{Unit.member.name}(") + GenerateArguments(data) + MakeClickableForThisUnit(");") + "\n";
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
                data.SetExpectedType(input.type);
                var connectedCode = GetNextValueUnit(input, data);
                data.RemoveExpectedType();
                if (input.type.IsSubclassOf(typeof(Component))) return new ValueCode(GetNextValueUnit(input, data), typeof(GameObject), ShouldCast(input, data, false));
                return new ValueCode(connectedCode, input.type, ShouldCast(input, data, false));
            }
            else if (input.hasDefaultValue)
            {
                if (input.type == typeof(GameObject) || input.type.IsSubclassOf(typeof(Component)) || input.type == typeof(Component) && input == Unit.target)
                {
                    return MakeClickableForThisUnit("gameObject".VariableHighlight());
                }
                return Unit.defaultValues[input.key].As().Code(true, Unit, true, true, "", false, true);
            }
            else
            {
                if (Unit.member.isMethod)
                {
                    if (Unit.member.methodInfo.GetParameters()[Unit.inputParameters.FirstOrDefault(parameter => parameter.Value == input).Key].IsDefined(typeof(ParamArrayAttribute), true))
                    {
                        return string.Empty;
                    }
                    else if (Unit.member.methodInfo.GetParameters()[Unit.inputParameters.FirstOrDefault(parameter => parameter.Value == input).Key].IsOptional)
                    {
                        return string.Empty;
                    }
                }
                else if (Unit.member.isConstructor)
                {
                    if (Unit.member.constructorInfo.GetParameters()[Unit.inputParameters.FirstOrDefault(parameter => parameter.Value == input).Key].IsDefined(typeof(ParamArrayAttribute), true))
                    {
                        return string.Empty;
                    }
                    else if (Unit.member.constructorInfo.GetParameters()[Unit.inputParameters.FirstOrDefault(parameter => parameter.Value == input).Key].IsOptional)
                    {
                        return string.Empty;
                    }
                }
                return MakeClickableForThisUnit($"/* \"{input.key} Requires Input\" */".WarningHighlight());
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
                        if (!input.hasValidConnection || (input.hasValidConnection && input.connection.source.unit is not GetVariable or AssetFieldUnit or InheritedFieldUnit or GetMember))
                        {
                            output.Add($"/* {input.key.Replace("%", "")} needs to be connected to a variable unit or a get member unit */".WarningHighlight());
                            continue;
                        }
                        output.Add("ref ".ConstructHighlight() + GenerateValue(Unit.inputParameters[index], data));
                        outputNames.Add(Unit.outputParameters[index], "&" + name);
                    }
                    else if (parameter.IsDefined(typeof(ParamArrayAttribute), false) && !Unit.inputParameters[index].hasValidConnection)
                    {
                        continue;
                    }
                    else if (parameter.IsOptional && !Unit.inputParameters[index].hasValidConnection)
                    {
                        continue;
                    }
                    else
                    {
                        output.Add(GenerateValue(Unit.inputParameters.Values.First(input => input.key == "%" + parameter.Name), data));
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
                    if (parameter.IsDefined(typeof(ParamArrayAttribute), false) && !Unit.inputParameters[index].hasValidConnection)
                    {
                        continue;
                    }
                    else if (parameter.IsOptional && !Unit.inputParameters[index].hasValidConnection)
                    {
                        continue;
                    }
                    else if (Unit.inputParameters.ContainsKey(index))
                        output.Add(GenerateValue(Unit.inputParameters[index], data));
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