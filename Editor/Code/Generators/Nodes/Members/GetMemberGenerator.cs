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
    [NodeGenerator(typeof(Unity.VisualScripting.GetMember))]
    public sealed class GetMemberGenerator : NodeGenerator<Unity.VisualScripting.GetMember>
    {
        public GetMemberGenerator(Unity.VisualScripting.GetMember unit) : base(unit)
        {
            NameSpaces = Unit.member.declaringType.Namespace;
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            var builder = Unit.CreateClickableString();
            if (Unit.target != null)
            {
                if (Unit.target.hasValidConnection)
                {
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
                        name = Unit.member.ToPseudoDeclarer().ToString(); // I don't think this should be possible through normal usage.
                    }
                    if (typeof(Component).IsAssignableFrom(Unit.member.pseudoDeclaringType))
                    {
                        if (GetComponent(Unit.target, data, out var code))
                            return builder.InvokeMember(t => t.Ignore(GenerateValue(Unit.target, data)), code).GetMember(name);
                        else
                            return builder.GetMember(t => t.Ignore(GenerateValue(Unit.target, data)), name);
                    }
                    else
                    {
                        return builder.GetMember(t => t.Ignore(GenerateValue(Unit.target, data)), name);
                    }
                }
                else
                {
                    return builder.GetMember(t => t.Ignore(GenerateValue(Unit.target, data)), Unit.member.name);
                }
            }
            else
            {
                return builder.GetMember(Unit.member.targetType, Unit.member.name);
            }
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
                        data.SetExpectedType(Unit.member.pseudoDeclaringType);
                        var connectedCode = GetNextValueUnit(input, data);
                        data.RemoveExpectedType();
                        if (Unit.member.pseudoDeclaringType.IsSubclassOf(typeof(Component)))
                        {
                            return connectedCode.CastAs(typeof(GameObject), Unit, ShouldCast(input, data));
                        }
                        return connectedCode.CastAs(input.type, Unit, ShouldCast(input, data));
                    }
                    else if (Unit.target.hasDefaultValue)
                    {
                        if (input.type == typeof(GameObject) || input.type.IsSubclassOf(typeof(Component)) || input.type == typeof(Component))
                        {
                            if (GetComponent(Unit.target, data, out var code))
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

        bool GetComponent(ValueInput valueInput, ControlGenerationData data, out string code)
        {
            if (valueInput.hasValidConnection)
            {
                if (valueInput.type == valueInput.connection.source.type && valueInput.connection.source.unit is MemberUnit or InheritedMemberUnit or AssetFieldUnit or AssetMethodCallUnit)
                {
                    code = string.Empty;
                    return false;
                }
                else
                {
                    if ((valueInput.connection.source.unit is MemberUnit memberUnit && memberUnit.member.name != "GetComponent") || GetSourceType(valueInput, data) == typeof(GameObject) && Unit.member.pseudoDeclaringType != typeof(GameObject))
                    {
                        code = $"GetComponent<{Unit.member.pseudoDeclaringType.As().CSharpName(false, true)}>";
                        return true;
                    }
                    code = string.Empty;
                    return false;
                }
            }
            else
            {
                if (Unit.member.pseudoDeclaringType != typeof(GameObject))
                {
                    code = $"GetComponent<{Unit.member.pseudoDeclaringType.As().CSharpName(false, true)}>";
                    return true;
                }
                else
                {
                    code = string.Empty;
                    return false;
                }
            }
        }

    }
}