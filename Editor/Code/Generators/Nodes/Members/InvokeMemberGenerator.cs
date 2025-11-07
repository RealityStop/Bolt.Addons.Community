using Unity;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;
using System;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(InvokeMember))]
    public sealed class InvokeMemberGenerator : LocalVariableGenerator
    {
        private InvokeMember Unit => unit as InvokeMember;
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
            data.SetCurrentExpectedTypeMet(data.GetExpectedType() != null && data.GetExpectedType().IsStrictlyAssignableFrom(output.type), output.type);
            if (output == Unit.result)
            {
                if (!Unit.enter.hasValidConnection && Unit.outputParameters.Count > 0)
                {
                    return MakeClickableForThisUnit($"/* Control Port Enter of {Unit.member.ToDeclarer()} requires a connection */".WarningHighlight());
                }


                if (Unit.enter.hasValidConnection)
                {
                    return MakeClickableForThisUnit(variableName.VariableHighlight());
                }
                else variableType = Unit.result.type;

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
                    if (Unit.member.isInvokedAsExtension)
                    {
                        return GenerateValue(Unit.target, data) + MakeClickableForThisUnit($".{Unit.member.name}(") + GenerateArguments(data) + MakeClickableForThisUnit(")");
                    }
                    else if (Unit.member.isExtension)
                    {
                        return GenerateValue(Unit.inputParameters[0], data) + MakeClickableForThisUnit($".{Unit.member.name}(") + GenerateArguments(data) + MakeClickableForThisUnit(")");
                    }
                    else if (Unit.target == null)
                    {
                        return MakeClickableForThisUnit(Unit.member.pseudoDeclaringType.As().CSharpName(false, true) + "." + Unit.member.name + "(") + GenerateArguments(data) + MakeClickableForThisUnit(")");
                    }
                    else
                    {
                        if (Unit.target.hasValidConnection && Unit.target.type != Unit.target.connection.source.type && Unit.member.pseudoDeclaringType.IsSubclassOf(typeof(Component)))
                        {
                            return GenerateValue(Unit.target, data) + MakeClickableForThisUnit(Unit.target.GetComponent(SourceType(Unit.target, data), Unit.member.pseudoDeclaringType, true, true) + "." + Unit.member.name + "(") + GenerateArguments(data) + MakeClickableForThisUnit(")");
                        }
                        else if (typeof(Component).IsStrictlyAssignableFrom(Unit.member.pseudoDeclaringType) && !data.IsCurrentExpectedTypeMet() && data.GetExpectedType() != Unit.member.pseudoDeclaringType && Unit.member.pseudoDeclaringType.IsConvertibleTo(data.GetExpectedType(), true))
                        {
                            data.SetCurrentExpectedTypeMet(true, data.GetExpectedType());
                            return (GenerateValue(Unit.target, data) + MakeClickableForThisUnit(Unit.target.GetComponent(SourceType(Unit.target, data), Unit.member.pseudoDeclaringType, true, true) + "." + Unit.member.name + "(") + GenerateArguments(data) + MakeClickableForThisUnit(")")).GetConvertToString(data.GetExpectedType(), Unit);
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

        private Type SourceType(ValueInput valueInput, ControlGenerationData data)
        {
            return GetSourceType(valueInput, data) ?? valueInput.connection?.source?.type ?? valueInput.type;
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            outputNames = new Dictionary<ValueOutput, string>();
            var output = string.Empty;

            bool hasResultConnection = Unit.result != null && Unit.result.hasValidConnection;
            var lineIndent = CodeBuilder.Indent(indent);
            if (hasResultConnection)
            {
                variableName = data.AddLocalNameInScope(Unit.member.name.LegalMemberName() + "_Variable", Unit.result.type);
                variableType = Unit.result.type;
                output += lineIndent + MakeClickableForThisUnit("var ".ConstructHighlight() + variableName.VariableHighlight() + " = ");
                lineIndent = "";
            }
            else if (Unit.result != null)
            {
                variableType = Unit.result.type;
            }
            else
            {
                variableType = Unit.member.declaringType;
            }

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
                {
                    parameters = MakeClickableForThisUnit(Unit.member.pseudoDeclaringType.As().CSharpName(false, true) + "(") + GenerateArguments(data) + MakeClickableForThisUnit(");");
                }
                output += lineIndent + MakeClickableForThisUnit($"{"new".ConstructHighlight()} ") + parameters + "\n";
                output += GetNextUnit(Unit.exit, data, indent);
                return output;
            }
            else
            {
                if (Unit.member.isInvokedAsExtension)
                {
                    var target = GenerateValue(Unit.target, data);
                    output += lineIndent + target + MakeClickableForThisUnit($".{Unit.member.name}(") + GenerateArguments(data) + MakeClickableForThisUnit(");") + "\n";
                }
                else if (Unit.member.isExtension)
                {
                    var target = GenerateValue(Unit.inputParameters[0], data);
                    output += lineIndent + target + MakeClickableForThisUnit($".{Unit.member.name}(") + GenerateArguments(data) + MakeClickableForThisUnit(");") + "\n";
                }
                else if (Unit.target == null)
                {
                    output += lineIndent + MakeClickableForThisUnit($"{Unit.member.pseudoDeclaringType.As().CSharpName(false, true)}.{Unit.member.name}(") + $"{GenerateArguments(data)}{MakeClickableForThisUnit(");")}" + "\n";
                }
                else
                {
                    var target = GenerateValue(Unit.target, data);
                    if (Unit.member.pseudoDeclaringType == typeof(GameObject) && Unit.target.hasValidConnection && typeof(Component).IsStrictlyAssignableFrom(SourceType(Unit.target, data)))
                    {
                        output += lineIndent + target + (typeof(Component).IsStrictlyAssignableFrom(Unit.target.type) && Unit.target.type != typeof(object) ? MakeClickableForThisUnit($".{"gameObject".VariableHighlight()}.GetComponent<{(GetSourceType(Unit.target, data) ?? Unit.target.connection.source.type).As().CSharpName(false, true)}>().{Unit.member.name}(") : MakeClickableForThisUnit($".{"gameObject".VariableHighlight()}.{Unit.member.name}(")) + $"{GenerateArguments(data)}{MakeClickableForThisUnit(");")}" + "\n";
                    }
                    else if (Unit.target.hasValidConnection && Unit.target.type != Unit.target.connection.source.type && typeof(Component).IsStrictlyAssignableFrom(Unit.member.pseudoDeclaringType))
                    {
                        output += lineIndent + target + MakeClickableForThisUnit($"{Unit.target.GetComponent(SourceType(Unit.target, data), Unit.member.pseudoDeclaringType, true, true)}.{Unit.member.name}(") + GenerateArguments(data) + MakeClickableForThisUnit(");") + "\n";
                    }
                    else if (typeof(Component).IsStrictlyAssignableFrom(Unit.member.pseudoDeclaringType) && data.GetExpectedType() != null && !data.IsCurrentExpectedTypeMet() && Unit.member.pseudoDeclaringType.IsConvertibleTo(data.GetExpectedType(), true))
                    {
                        output += (lineIndent + target + MakeClickableForThisUnit($"{Unit.target.GetComponent(SourceType(Unit.target, data), Unit.member.pseudoDeclaringType, true, true)}.{Unit.member.name}(") + GenerateArguments(data) + MakeClickableForThisUnit(");")).GetConvertToString(data.GetExpectedType(), Unit) + "\n";
                    }
                    else
                    {
                        output += lineIndent + target + MakeClickableForThisUnit($".{Unit.member.name}(") + GenerateArguments(data) + MakeClickableForThisUnit(");") + "\n";
                    }
                }

                output += GetNextUnit(Unit.exit, data, indent);
                return output;
            }
        }

        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input == null) return MakeClickableForThisUnit($"Error");
            if (input.hasValidConnection)
            {
                // if (input.type.IsSubclassOf(typeof(Component))) return GetNextValueUnit(input, data).CastAs(typeof(GameObject), Unit, shouldCast);
                data.SetExpectedType(input.type);
                var connectedCode = GetNextValueUnit(input, data);
                // var shouldCast = ShouldCast(input, data, false);
                data.RemoveExpectedType();
                return connectedCode/*.CastAs(input.type, Unit, shouldCast)*/;
            }
            else if (input.hasDefaultValue)
            {
                if (input == Unit.target && (input.type == typeof(GameObject) || typeof(Component).IsAssignableFrom(input.type)))
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
                int startIndex = Unit.member.isExtension && !Unit.member.isInvokedAsExtension ? 1 : 0;

                for (int i = startIndex; i < (Unit.member.isInvokedAsExtension ? parameters.Length - 1 : parameters.Length); i++)
                {
                    var parameter = parameters[i];
                    var input = Unit.inputParameters.TryGetValue(i, out var p) ? p : null;
                    if (input == null)
                    {
                        continue;
                    }
                    else if (parameter.HasOutModifier())
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
                    else if (parameter.IsOptional && !input.hasValidConnection && !input.hasDefaultValue)
                    {
                        bool hasLaterConnection = false;

                        for (int j = i + 1; j < parameters.Length; j++)
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