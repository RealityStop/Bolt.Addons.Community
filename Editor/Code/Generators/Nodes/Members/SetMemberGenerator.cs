using Unity;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(SetMember))]
    public sealed class SetMemberGenerator : NodeGenerator<SetMember>
    {
        public SetMemberGenerator(SetMember unit) : base(unit)
        {
            NameSpaces = Unit.member.declaringType.Namespace;
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            if (input == Unit.assign)
            {
                var output = string.Empty;
                var memberName = Unit.member.name;
                var inputValue = GenerateValue(Unit.input, data);

                if (Unit.target != null)
                {
                    var targetValue = GenerateValue(Unit.target, data);

                    output += CodeBuilder.Indent(indent) + targetValue + MakeClickableForThisUnit($".{memberName} = ") + $"{inputValue}{MakeClickableForThisUnit(";")}\n";
                }
                else
                {
                    output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit(Unit.member.pseudoDeclaringType.As().CSharpName(false, true) + $".{memberName} = " + $"{inputValue}{MakeClickableForThisUnit(";")}\n");
                }
                output += GetNextUnit(Unit.assigned, data, indent);

                return output;
            }

            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return new ValueCode($"{GenerateValue(Unit.target, data)}", Unit.target.type, ShouldCast(Unit.target, data)) + MakeClickableForThisUnit(new ValueCode($".{Unit.member.name}"));
        }

        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input != Unit.target)
            {
                if (input.hasValidConnection)
                {
                    data.SetExpectedType(input.type);
                    var connectedCode = GetNextValueUnit(input, data);
                    data.RemoveExpectedType();
                    return connectedCode;
                }
                else if (input.hasDefaultValue)
                {
                    return Unit.defaultValues[input.key].As().Code(true, Unit, true, true, "", false, true);
                }
                else
                {
                    return MakeClickableForThisUnit($"/* {input.key} requires input */".WarningHighlight());
                }
            }
            else
            {
                if (input.hasValidConnection)
                {
                    if (typeof(Component).IsAssignableFrom(input.type) || input.type == typeof(GameObject))
                    {
                        var inputCode = GetNextValueUnit(input, data);
                        return new ValueCode(inputCode, typeof(GameObject), ShouldCast(input, data)) + MakeClickableForThisUnit($"{GetComponent(Unit.target, data)}");
                    }
                    return GetNextValueUnit(input, data);
                }
                else
                {
                    if (Unit.target.nullMeansSelf)
                    {
                        if (input.type.IsSubclassOf(typeof(Component)))
                        {
                            return MakeClickableForThisUnit("gameObject".VariableHighlight() + new ValueCode($"{GetComponent(Unit.target, data)}"));
                        }
                        else
                        {
                            return MakeClickableForThisUnit("gameObject".VariableHighlight());
                        }
                    }
                    else
                    {
                        return Unit.defaultValues[input.key].As().Code(true, Unit, true, true, "", false, true);
                    }
                }
            }
        }

        string GetComponent(ValueInput valueInput, ControlGenerationData data)
        {
            if (valueInput.hasValidConnection)
            {
                if (valueInput.type == valueInput.connection.source.type && valueInput.connection.source.unit is MemberUnit or CodeAssetUnit)
                {
                    return "";
                }
                else
                {
                    return (valueInput.connection.source.unit is MemberUnit memberUnit && memberUnit.member.name != "GetComponent") || (GetSourceType(valueInput, data) == typeof(GameObject)) && Unit.member.declaringType != typeof(GameObject) ? $".GetComponent<{Unit.member.pseudoDeclaringType.As().CSharpName(false, true)}>()" : string.Empty;
                }
            }
            else
            {
                return $".GetComponent<{Unit.member.pseudoDeclaringType.As().CSharpName(false, true)}>()";
            }
        }
    }
}