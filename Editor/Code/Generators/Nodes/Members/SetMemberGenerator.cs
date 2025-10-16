using Unity;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;
using System;
using Unity.VisualScripting.Community.CSharp;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(SetMember))]
    public sealed class SetMemberGenerator : NodeGenerator<SetMember>
    {
        public SetMemberGenerator(SetMember unit) : base(unit)
        {
            NameSpaces = Unit.member.pseudoDeclaringType.Namespace;
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            if (input == Unit.assign)
            {
                var output = string.Empty;
                var memberName = Unit.member.name.VariableHighlight();
                data.SetExpectedType(Unit.input.type);
                var inputValue = GenerateValue(Unit.input, data);
                data.RemoveExpectedType();

                if (Unit.target != null)
                {
                    data.SetExpectedType(Unit.target.type);
                    var targetValue = GenerateValue(Unit.target, data);
                    data.RemoveExpectedType();
                    var code = Unit.target.GetComponent(SourceType(Unit.target, data), Unit.member.pseudoDeclaringType, true, true);

                    output += CodeBuilder.Indent(indent) + targetValue + MakeClickableForThisUnit(code + $".{memberName} = ") + $"{inputValue}{MakeClickableForThisUnit(";")}\n";
                }
                else
                {
                    output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit(Unit.member.pseudoDeclaringType.As().CSharpName(false, true) + $".{memberName} = ") + $"{inputValue}{MakeClickableForThisUnit(";")}\n";
                }
                output += GetNextUnit(Unit.assigned, data, indent);

                return output;
            }

            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            var builder = Unit.CreateClickableString();
            data.SetExpectedType(Unit.target.type);
            var targetValue = GenerateValue(Unit.target, data);
            data.RemoveExpectedType();
            var code = Unit.target.GetComponent(SourceType(Unit.target, data), Unit.member.pseudoDeclaringType, true, true);

            return builder.Ignore(targetValue).Clickable(code).GetMember(Unit.member.name);
        }

        private Type SourceType(ValueInput valueInput, ControlGenerationData data)
        {
            return GetSourceType(valueInput, data) ?? valueInput.connection?.source?.type ?? valueInput.type;
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
                    data.SetExpectedType(input.type);
                    var connectedCode = GetNextValueUnit(input, data);
                    var shouldCast = ShouldCast(input, data, false);
                    data.RemoveExpectedType();
                    return connectedCode.CastAs(input.type, Unit, shouldCast);
                }
                else
                {
                    if (Unit.target.nullMeansSelf)
                    {
                        return MakeClickableForThisUnit("gameObject".VariableHighlight());
                    }
                    else
                    {
                        return Unit.defaultValues[input.key].As().Code(true, Unit, true, true, "", false, true);
                    }
                }
            }
        }
    }
}