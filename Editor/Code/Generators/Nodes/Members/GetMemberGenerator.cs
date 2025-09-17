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
    [NodeGenerator(typeof(Unity.VisualScripting.GetMember))]
    public sealed class GetMemberGenerator : NodeGenerator<Unity.VisualScripting.GetMember>
    {
        public GetMemberGenerator(Unity.VisualScripting.GetMember unit) : base(unit)
        {
            NameSpaces = Unit.member.declaringType.Namespace;
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if (Unit.target != null)
            {
                if (Unit.target.hasValidConnection)
                {
                    string type;

                    if (Unit.member.isField)
                    {
                        type = Unit.member.fieldInfo.Name;
                    }
                    else if (Unit.member.isProperty)
                    {
                        type = Unit.member.name;
                    }
                    else
                    {
                        type = Unit.member.ToPseudoDeclarer().ToString(); // I don't think this should be possible.
                    }

                    string outputCode;

                    if (typeof(Component).IsAssignableFrom(Unit.member.pseudoDeclaringType))
                    {
                        outputCode = new ValueCode($"{GenerateValue(Unit.target, data)}{MakeClickableForThisUnit($"{GetComponent(Unit.target, data)}.{type.VariableHighlight()}")}");
                    }
                    else
                    {
                        outputCode = new ValueCode(GenerateValue(Unit.target, data) + MakeClickableForThisUnit($".{type.VariableHighlight()}"));
                    }

                    return outputCode;
                }
                else
                {
                    return $"{GenerateValue(Unit.target, data)}{MakeClickableForThisUnit($".{Unit.member.name.VariableHighlight()}")}";
                }
            }
            else
            {
                return MakeClickableForThisUnit($"{Unit.member.targetType.As().CSharpName(false, true)}.{Unit.member.name.VariableHighlight()}");
            }
        }


        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
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
                            return new ValueCode(connectedCode, typeof(GameObject), ShouldCast(input, data));
                        }
                        return new ValueCode(connectedCode, input.type, ShouldCast(input, data));
                    }
                    else if (Unit.target.hasDefaultValue)
                    {
                        var defaultValue = Unit.defaultValues[input.key];

                        if (Unit.target.type == typeof(GameObject) || input.type.IsSubclassOf(typeof(Component)))
                        {
                            return MakeClickableForThisUnit("gameObject".VariableHighlight() + new ValueCode($"{GetComponent(Unit.target, data)}"));
                        }
                        else
                        {
                            return MakeClickableForThisUnit(defaultValue.As().Code(false, true, true));
                        }

                    }
                    else
                    {
                        return MakeClickableForThisUnit("/* Target Requires Input */".WarningHighlight());
                    }
                }
            }

            return base.GenerateValue(input, data);
        }

        string GetComponent(ValueInput valueInput, ControlGenerationData data)
        {
            if (valueInput.hasValidConnection)
            {
                if (valueInput.type == valueInput.connection.source.type && valueInput.connection.source.unit is MemberUnit or InheritedMemberUnit or AssetFieldUnit or AssetMethodCallUnit)
                {
                    return "";
                }
                else
                {
                    return ((valueInput.connection.source.unit is MemberUnit memberUnit && memberUnit.member.name != "GetComponent") || GetSourceType(valueInput, data) == typeof(GameObject)) && Unit.member.pseudoDeclaringType != typeof(GameObject) ? $".GetComponent<{Unit.member.pseudoDeclaringType.As().CSharpName(false, true)}>()" : string.Empty;
                }
            }
            else
            {
                if (Unit.member.pseudoDeclaringType != typeof(GameObject))
                    return $".GetComponent<{Unit.member.pseudoDeclaringType.As().CSharpName(false, true)}>()";
                else
                    return "";
            }
        }

    }
}