using Unity;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

[NodeGenerator(typeof(Unity.VisualScripting.InvokeMember))]
public sealed class InvokeMemberGenerator : NodeGenerator<Unity.VisualScripting.InvokeMember>
{
    private int codeindent;

    public InvokeMemberGenerator(Unity.VisualScripting.InvokeMember unit) : base(unit)
    {
        NameSpace = Unit.member.declaringType.Namespace;
    }

    public override string GenerateValue(ValueOutput output)
    {
        if (output == Unit.result)
        {
            var _output = string.Empty;
            if (Unit.member.isConstructor)
            {
                _output += new ValueCode($"{"new".ConstructHighlight()} {Unit.member.pseudoDeclaringType.As().CSharpName(false, true)}({GenerateArguments(Unit.inputParameters.Values.ToList())})");
            }
            else
            {
                if (Unit.target == null)
                {
                    _output += new ValueCode($"{Unit.member.pseudoDeclaringType.As().CSharpName(false, true)}.{Unit.member.name}({GenerateArguments(Unit.inputParameters.Values.ToList())})");
                }
                else
                {
                    if (Unit.target.hasValidConnection && Unit.target.type != Unit.target.connection.source.type && Unit.member.pseudoDeclaringType.IsSubclassOf(typeof(Component)))
                    {
                        _output += new ValueCode(new ValueCode(GenerateValue(Unit.target) + GetComponent(Unit.target) + Unit.member.name + $"({GenerateArguments(Unit.inputParameters.Values.ToList())})"));
                    }
                    else if (Unit.member.pseudoDeclaringType.IsSubclassOf(typeof(Component)))
                    {
                        _output += new ValueCode(GenerateValue(Unit.target) + GetComponent(Unit.target) + Unit.member.name + $"({GenerateArguments(Unit.inputParameters.Values.ToList())})", typeof(GameObject), ShouldCast(Unit.target));
                    }
                    else
                    {
                        _output += new ValueCode($"{GenerateValue(Unit.target)}.{Unit.member.name}({GenerateArguments(Unit.inputParameters.Values.ToList())})");
                    }
                }
            }
            return _output;
        }
        else if (Unit.outputParameters.ContainsValue(output))
        {
            return output.key.Replace("&", "").Replace("%", "").VariableHighlight();
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
            return valueInput.connection.source.unit is MemberUnit memberUnit && memberUnit.member.name != "GetComponent" ? $".GetComponent<{Unit.member.pseudoDeclaringType.As().CSharpName(false, true)}>()." : ".";
        }
        else
        {
            return $".GetComponent<{Unit.member.pseudoDeclaringType.As().CSharpName(false, true)}>().";
        }
    }

    public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
    {
        codeindent = indent;
        var output = string.Empty;
        if (Unit.result == null || !Unit.result.hasValidConnection)
        {
            if (Unit.member.isConstructor)
            {
                output += new CodeLine($"{"new".ConstructHighlight()} {Unit.member.pseudoDeclaringType.As().CSharpName(false, true)}({GenerateArguments(Unit.inputParameters.Values.ToList())})", indent);
            }
            else
            {
                if (Unit.target == null)
                {
                    output += new CodeLine($"{Unit.member.pseudoDeclaringType.As().CSharpName(false, true)}.{Unit.member.name}({GenerateArguments(Unit.inputParameters.Values.ToList())})", indent);
                }
                else
                {
                    if (Unit.member.pseudoDeclaringType == typeof(GameObject) && Unit.target.hasValidConnection && Unit.target.connection.source.type.IsSubclassOf(typeof(Component)))
                    {
                        output += new CodeLine($"{GenerateValue(Unit.target)}.gameObject.GetComponent<{Unit.member.pseudoDeclaringType.As().CSharpName(false, true)}>().{Unit.member.name}({GenerateArguments(Unit.inputParameters.Values.ToList())})", indent);
                    }
                    else if (Unit.target.hasValidConnection && Unit.target.type != Unit.target.connection.source.type && Unit.member.pseudoDeclaringType.IsSubclassOf(typeof(Component)))
                    {
                        output += new CodeLine(new ValueCode(GenerateValue(Unit.target) + GetComponent(Unit.target) + Unit.member.name + $"({GenerateArguments(Unit.inputParameters.Values.ToList())})", typeof(GameObject), ShouldCast(Unit.target)), indent);
                    }
                    else if (Unit.member.pseudoDeclaringType.IsSubclassOf(typeof(Component)))
                    {
                        output += new CodeLine($"{GenerateValue(Unit.target)}{GetComponent(Unit.target)}{Unit.member.name}({GenerateArguments(Unit.inputParameters.Values.ToList())})", indent);
                    }
                    else
                    {
                        output += new CodeLine($"{GenerateValue(Unit.target)}.{Unit.member.name}({GenerateArguments(Unit.inputParameters.Values.ToList())})", indent);
                    }
                }
            }
            output += Unit.exit.hasValidConnection ? (Unit.exit.connection.destination.unit as Unit).GenerateControl(Unit.exit.connection.destination, data, indent) : string.Empty;
            return output;
        }
        return Unit.exit.hasValidConnection ? (Unit.exit.connection.destination.unit as Unit).GenerateControl(Unit.exit.connection.destination, data, indent) : string.Empty;
    }

    public override string GenerateValue(ValueInput input)
    {
        if (input.hasValidConnection)
        {
            if (input.type.IsSubclassOf(typeof(Component))) return new ValueCode((input.connection.source.unit as Unit).GenerateValue(input.connection.source), typeof(GameObject), ShouldCast(input)) + new ValueCode($"{(input.connection.source.type == typeof(GameObject) ? $".GetComponent<{input.type.As().CSharpName(false, true)}>()" : string.Empty)}");
            return new ValueCode((input.connection.source.unit as Unit).GenerateValue(input.connection.source), input.type, ShouldCast(input)) + new ValueCode($"{(input.type.IsSubclassOf(typeof(Component)) && input.connection.source.type == typeof(GameObject) ? $".GetComponent<{input.type.As().CSharpName(false, true)}>()" : string.Empty)}");
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
            return $"/* \"{input.key} Requires Input\" */";
        }
    }

    private string GenerateArguments(List<ValueInput> arguments)
    {
        if (arguments.Count > 0)
        {
            var argumentValues = arguments.Select(arg =>
            {
                return GenerateValue(arg);
            });
            return string.Join(", ", argumentValues);
        }
        else
        {
            return string.Empty;
        }
    }
}