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
    [NodeGenerator(typeof(Unity.VisualScripting.GetMember))]
    public sealed class GetMemberGenerator : NodeGenerator<Unity.VisualScripting.GetMember>
    {
        public GetMemberGenerator(Unity.VisualScripting.GetMember unit) : base(unit)
        {
            NameSpaces = Unit.member.pseudoDeclaringType.Namespace;
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            var builder = Unit.CreateClickableString();

            data.CreateSymbol(Unit, output.type);

            if (Unit.target == null)
                return builder.GetMember(Unit.member.targetType, Unit.member.name);

            if (!Unit.target.hasValidConnection)
                return builder.GetMember(t => t.Ignore(GenerateValue(Unit.target, data)), Unit.member.name);

            string name;

            if (Unit.member.isField)
            {
                name = Unit.member.fieldInfo.Name;
            }
            else if (Unit.member.isProperty)
            {
                name = Unit.member.name;
            }
            else
            {
                name = Unit.member.ToDeclarer().ToString(); // I don't think this should be possible through normal usage.
            }
            data.SetExpectedType(Unit.target.type);
            var targetCode = GenerateValue(Unit.target, data);
            var result = data.RemoveExpectedType();
            if (!typeof(Component).IsAssignableFrom(Unit.member.pseudoDeclaringType))
                return builder.GetMember(t => t.Ignore(targetCode), name);

            var code = !result.isMet ? Unit.target.GetComponent(SourceType(Unit.target, data), Unit.member.pseudoDeclaringType, false, false) : "";
            if (!string.IsNullOrEmpty(code))
                return builder.InvokeMember(t => t.Ignore(targetCode), code).GetMember(name);
            else
                return builder.GetMember(t => t.Ignore(targetCode), name);
        }

        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            var builder = Unit.CreateClickableString();
            if (Unit.target != null)
            {
                if (input == Unit.target)
                {
                    if (Unit.target.hasValidConnection)
                    {
                        var connectedCode = GetNextValueUnit(input, data);
                        // if (typeof(Component).IsStrictlyAssignableFrom(Unit.member.pseudoDeclaringType))
                        // {
                        //     return connectedCode.CastAs(typeof(GameObject), Unit, ShouldCast(input, data));
                        // }
                        return ShouldCast(input, data) ? connectedCode.GetConvertToString(input.type, Unit) : connectedCode;
                    }
                    else if (Unit.target.hasDefaultValue)
                    {
                        if (input.type == typeof(GameObject) || typeof(Component).IsStrictlyAssignableFrom(input.type))
                        {
                            var code = Unit.target.GetComponent(SourceType(Unit.target, data), Unit.member.pseudoDeclaringType, false, false);
                            if (!string.IsNullOrEmpty(code))
                                return builder.InvokeMember("gameObject".VariableHighlight(), code, Array.Empty<string>());
                            else
                                builder.Clickable("gameObject".VariableHighlight());
                        }
                        return Unit.defaultValues[input.key].As().Code(true, Unit, true, true, "", false, true);
                    }
                    else
                    {
                        return base.GenerateValue(input, data);
                    }
                }
            }

            return base.GenerateValue(input, data);
        }

        private Type SourceType(ValueInput valueInput, ControlGenerationData data)
        {
            return GetSourceType(valueInput, data) ?? valueInput.connection?.source?.type ?? valueInput.type;
        }
    }
}