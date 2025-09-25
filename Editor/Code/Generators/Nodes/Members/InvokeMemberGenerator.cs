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
    [NodeGenerator(typeof(InvokeMember))]
    public sealed class InvokeMemberGenerator : NodeGenerator<InvokeMember>
    {
        private Dictionary<ValueOutput, string> outputNames;
        public InvokeMemberGenerator(InvokeMember unit) : base(unit)
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
                    return MakeClickableForThisUnit($"/* Control Port Enter of {Unit.member.ToDeclarer()} requires a connection */".WarningHighlight());
                }

                if (Unit.member.isConstructor)
                {
                    string parameters = string.Empty;
                    string typeName = Unit.member.pseudoDeclaringType.As().CSharpName(false, true);
                    if (Unit.member.pseudoDeclaringType.IsArray)
                    {
                        int count = 0;
                        var type = Unit.member.pseudoDeclaringType;
                        while (type.IsArray)
                        {
                            count++;
                            type = type.GetElementType();
                        }

                        typeName = MakeClickableForThisUnit(typeName.Replace("[]", "")) + MakeClickableForThisUnit("[") + GenerateArguments(data) + MakeClickableForThisUnit("]") + MakeClickableForThisUnit(string.Concat(Enumerable.Repeat("[]", count - 1)));
                    }
                    else
                    {
                        parameters = MakeClickableForThisUnit("(") + GenerateArguments(data) + MakeClickableForThisUnit(")");
                        typeName = MakeClickableForThisUnit(typeName);
                    }
                    return MakeClickableForThisUnit("new ".ConstructHighlight()) + typeName + parameters;
                }
                else
                {
                    if (Unit.target == null)
                    {
                        return MakeClickableForThisUnit(Unit.member.pseudoDeclaringType.As().CSharpName(false, true) + "." + Unit.member.name + "(") + GenerateArguments(data) + MakeClickableForThisUnit(")");
                    }
                    else
                    {
                        if (Unit.target.hasValidConnection && Unit.target.type != Unit.target.connection.source.type && Unit.member.pseudoDeclaringType.IsSubclassOf(typeof(Component)))
                        {
                            return GenerateValue(Unit.target, data) + MakeClickableForThisUnit(GetComponent(Unit.target, data) + "." + Unit.member.name + "(") + GenerateArguments(data) + MakeClickableForThisUnit(")");
                        }
                        else if (Unit.member.pseudoDeclaringType.IsSubclassOf(typeof(Component)))
                        {
                            return (GenerateValue(Unit.target, data) + MakeClickableForThisUnit(GetComponent(Unit.target, data) + "." + Unit.member.name + "(") + GenerateArguments(data) + MakeClickableForThisUnit(")")).CastAs(typeof(GameObject), Unit, ShouldCast(Unit.target, data, false));
                        }
                        else
                        {
                            return GenerateValue(Unit.target, data) + MakeClickableForThisUnit("." + Unit.member.name + "(") + GenerateArguments(data) + MakeClickableForThisUnit(")");
                        }
                    }
                }
            }
            else if (Unit.outputParameters.ContainsValue(output))
            {
                if (!Unit.enter.hasValidConnection && Unit.outputParameters.Count > 0)
                {
                    return MakeClickableForThisUnit($"/* Control Port Enter of {Unit.member.ToDeclarer()} requires a connection */".WarningHighlight());
                }

                if (Unit.member.GetParameterInfos().ToArray()[Unit.outputParameters.FirstOrDefault(parameter => parameter.Value == output).Key].ParameterType.IsByRef)
                {
                    return GenerateValue(Unit.inputParameters[Unit.outputParameters.FirstOrDefault(parameter => parameter.Value == output).Key], data);
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
            var output = string.Empty;
            if (Unit.result == null || !Unit.result.hasValidConnection)
            {
                if (Unit.member.isConstructor)
                {
                    string parameters;
                    if (Unit.member.pseudoDeclaringType.IsArray)
                    {
                        int count = 0;
                        var type = Unit.member.pseudoDeclaringType;
                        while (type.IsArray)
                        {
                            count++;
                            type = type.GetElementType();
                        }

                        string typeName = Unit.member.pseudoDeclaringType.As().CSharpName(false, true);
                        parameters = MakeClickableForThisUnit(typeName.Replace("[]", "")) + MakeClickableForThisUnit("[") + GenerateArguments(data) + MakeClickableForThisUnit("]") + MakeClickableForThisUnit(string.Concat(Enumerable.Repeat("[]", count - 1)));
                    }
                    else
                        parameters = MakeClickableForThisUnit(Unit.member.pseudoDeclaringType.As().CSharpName(false, true) + "(") + GenerateArguments(data) + MakeClickableForThisUnit(");");

                    output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit($"{"new".ConstructHighlight()} ") + parameters + "\n";
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
                var shouldCast = ShouldCast(input, data, false);
                if (input.type.IsSubclassOf(typeof(Component))) return GetNextValueUnit(input, data).CastAs(typeof(GameObject), Unit, shouldCast);
                data.SetExpectedType(input.type);
                var connectedCode = GetNextValueUnit(input, data);
                data.RemoveExpectedType();
                return connectedCode.CastAs(input.type, Unit, shouldCast);
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

        private string GenerateArguments(ControlGenerationData data)
        {
            var method = Unit.member.methodInfo;
            var parameters = method?.GetParameters();
            if (data != null && Unit.member.isMethod)
            {
                var output = new List<string>();
                int count = parameters.Length;

                for (int i = 0; i < count; i++)
                {
                    var parameter = parameters[i];
                    var input = Unit.inputParameters.TryGetValue(i, out var p) ? p : null;
                    if (parameter.HasOutModifier())
                    {
                        var name = data.AddLocalNameInScope(parameter.Name, parameter.ParameterType).VariableHighlight();
                        output.Add(MakeClickableForThisUnit("out var".ConstructHighlight() + name));

                        if (Unit.outputParameters.TryGetValue(i, out var outValue) && !outputNames.ContainsKey(outValue))
                            outputNames.Add(outValue, "&" + name);
                    }
                    else if (parameter.ParameterType.IsByRef)
                    {
                        if (input == null)
                        {
                            output.Add(MakeClickableForThisUnit($"/* Missing input for {parameter.Name} */".WarningHighlight()));
                            continue;
                        }

                        if (!input.hasValidConnection || (input.hasValidConnection && !input.connection.source.unit.IsValidRefUnit()))
                        {
                            output.Add(MakeClickableForThisUnit($"/* {input.key.Replace("%", "")} needs connection to a variable or member unit */".WarningHighlight()));
                            continue;
                        }

                        var name = data.AddLocalNameInScope(parameter.Name, parameter.ParameterType).VariableHighlight();
                        output.Add(MakeClickableForThisUnit("ref ".ConstructHighlight()) + GenerateValue(input, data));
                        if (Unit.outputParameters.TryGetValue(i, out var outRef) && !outputNames.ContainsKey(outRef))
                            outputNames.Add(outRef, "&" + name);
                    }
                    if (parameter.IsOptional && !input.hasValidConnection && !input.hasDefaultValue)
                    {
                        bool hasLaterConnection = false;

                        for (int j = i + 1; j < count; j++)
                        {
                            var laterParam = Unit.inputParameters[j];
                            if (laterParam != null && (laterParam.hasValidConnection || laterParam.hasDefaultValue))
                            {
                                hasLaterConnection = true;
                                break;
                            }
                        }

                        if (!hasLaterConnection)
                            continue;
                    }
                    else if (parameter.IsDefined(typeof(ParamArrayAttribute), false) && (input == null || !input.hasValidConnection))
                    {
                        continue;
                    }
                    else
                    {
                        output.Add(GenerateValue(input, data));
                    }
                }

                return string.Join(MakeClickableForThisUnit(", "), output);
            }
            else if (Unit.member.isMethod)
            {
                var output = new List<string>();
                int count = parameters.Length;

                for (int i = 0; i < count; i++)
                {
                    if (!Unit.inputParameters.TryGetValue(i, out var input))
                        continue;

                    var param = parameters[i];
                    if (param.IsOptional && !input.hasValidConnection && !input.hasDefaultValue)
                    {
                        bool hasLaterConnection = false;

                        for (int j = i + 1; j < count; j++)
                        {
                            var laterParam = Unit.inputParameters[j];
                            if (laterParam != null && (laterParam.hasValidConnection || laterParam.hasDefaultValue))
                            {
                                hasLaterConnection = true;
                                break;
                            }
                        }

                        if (!hasLaterConnection)
                            continue;
                    }

                    if (param.IsDefined(typeof(ParamArrayAttribute), false) && !input.hasValidConnection)
                        continue;

                    output.Add(GenerateValue(input, data));
                }

                return string.Join(MakeClickableForThisUnit(", "), output);
            }
            else
            {
                return string.Join(MakeClickableForThisUnit(", "), Unit.valueInputs.Select(input => GenerateValue(input, data)));
            }
        }
    }
}