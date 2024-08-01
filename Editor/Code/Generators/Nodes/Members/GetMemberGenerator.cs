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
            NameSpace = Unit.member.declaringType.Namespace;
        }
    
        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return base.GenerateControl(input, data, indent);
        }
    
        public override string GenerateValue(ValueOutput output)
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
                        type = Unit.member.ToPseudoDeclarer().ToString();
                    }
    
                    string outputCode;
    
                    if (Unit.member.pseudoDeclaringType.IsSubclassOf(typeof(Component)))
                    {
                        outputCode = new ValueCode($"{GenerateValue(Unit.target)}{GetComponent(Unit.target)}.{type}");
                    }
                    else
                    {
                        outputCode = new ValueCode($"{GenerateValue(Unit.target)}.{type}");
                    }
    
                    return outputCode;
                }
                else
                {
                    return $"{GenerateValue(Unit.target)}.{Unit.member.name}";
                }
            }
            else
            {
                return Unit.member.ToString();
            }
        }
    
    
        public override string GenerateValue(ValueInput input)
        {
            if (Unit.target != null)
            {
                if (input == Unit.target)
                {
                    if (Unit.target.hasValidConnection)
                    {
                        if (Unit.member.pseudoDeclaringType.IsSubclassOf(typeof(Component)))
                        {
                            return new ValueCode((input.connection.source.unit as Unit).GenerateValue(input.connection.source), typeof(GameObject), ShouldCast(input));
                        }
                        return new ValueCode((input.connection.source.unit as Unit).GenerateValue(input.connection.source), input.type, ShouldCast(input));
                    }
                    else if (Unit.target.hasDefaultValue)
                    {
                        var defaultValue = Unit.defaultValues[input.key];
    
                        if (Unit.target.type == typeof(GameObject) || input.type.IsSubclassOf(typeof(Component)))
                        {
                            return "gameObject".VariableHighlight() + new ValueCode($"{GetComponent(Unit.target)}");
                        }
                        else
                        {
                            return defaultValue.As().Code(false, true, true);
                        }
    
                    }
                    else
                    {
                        return "/* Target Requires Input */";
                    }
                }
            }
    
            return base.GenerateValue(input);
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