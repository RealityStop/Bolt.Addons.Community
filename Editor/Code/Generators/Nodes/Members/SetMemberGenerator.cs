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
            NameSpace = Unit.member.declaringType.Namespace;
        }
    
        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            if (input == Unit.assign)
            {
                var output = string.Empty;
                var memberName = Unit.member.name;
                var inputValue = GenerateValue(Unit.input);
    
                if (Unit.target != null)
                {
                    var targetValue = GenerateValue(Unit.target);
    
                    output += CodeBuilder.Indent(indent) + CodeUtility.MakeSelectable(Unit, $"{targetValue}.{memberName} = {inputValue};");
                }
                else
                {
                    output += CodeBuilder.Indent(indent) + CodeUtility.MakeSelectable(Unit, Unit.member.pseudoDeclaringType.As().CSharpName(false, true) + $".{memberName} = {inputValue};");
                }
                output += GetNextUnit(Unit.assigned, data, indent);
    
                return output;
            }
    
            return null;
        }
    
        public override string GenerateValue(ValueOutput output)
        {
            return new ValueCode($"{GenerateValue(Unit.target)}", Unit.target.type, ShouldCast(Unit.target)) + new ValueCode($".{Unit.member.name}");
        }
    
        public override string GenerateValue(ValueInput input)
        {
            if (input != Unit.target)
            {
                if (input.hasValidConnection)
                {
                    return GetNextValueUnit(input);
                }
                else if (input.hasDefaultValue)
                {
                    return Unit.defaultValues[input.key].As().Code(true, true, true, "");
                }
                else
                {
                    return $"/* {input.key} requires input */";
                }
            }
            else
            {
                if (input.hasValidConnection)
                {
                    if (input.type.IsSubclassOf(typeof(Component)) || input.type == typeof(GameObject))
                    {
                        return new ValueCode(GetNextValueUnit(input), typeof(GameObject), ShouldCast(input)) + new ValueCode($"{GetComponent(Unit.target)}");
                    }
                    return GetNextValueUnit(input);
                }
                else
                {
                    if (Unit.target.nullMeansSelf)
                    {
                        if (input.type.IsSubclassOf(typeof(Component)))
                        {
                            return "gameObject".VariableHighlight() + new ValueCode($"{GetComponent(Unit.target)}");
                        }
                        else
                        {
                            return "gameObject".VariableHighlight();
                        }
                    }
                    else
                    {
                        return Unit.defaultValues[input.key].As().Code(true, true, true, "");
                    }
                }
            }
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
    }
}