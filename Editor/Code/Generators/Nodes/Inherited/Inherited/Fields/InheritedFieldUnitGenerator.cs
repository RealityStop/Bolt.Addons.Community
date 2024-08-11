using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    
    [NodeGenerator(typeof(InheritedFieldUnit))]
    public class InheritedFieldUnitGenerator : NodeGenerator<InheritedFieldUnit>
    {
        public InheritedFieldUnitGenerator(Unit unit) : base(unit)
        {
        }
    
        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;
            output += CodeUtility.MakeSelectable(Unit, CodeBuilder.Indent(indent) + CodeBuilder.Assign("this".ConstructHighlight() + "." + Unit.member.name.VariableHighlight(), GenerateValue(Unit.value, data)));
            output += "\n" + GetNextUnit(Unit.exit, data, indent);
            return output;
        }
    
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if(Unit.actionDirection == ActionDirection.Get)
            {
                return "this".ConstructHighlight() + "." + Unit.member.name.VariableHighlight();
            }
    
            return base.GenerateValue(output, data);
        }
    }
    
}