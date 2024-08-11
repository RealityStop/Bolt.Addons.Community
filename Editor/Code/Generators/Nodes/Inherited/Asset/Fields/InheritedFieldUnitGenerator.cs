using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    
    [NodeGenerator(typeof(AssetFieldUnit))]
    public class AssetFieldUnitGenerator : NodeGenerator<AssetFieldUnit>
    {
        public AssetFieldUnitGenerator(Unit unit) : base(unit)
        {
        }
    
        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;
            output += CodeUtility.MakeSelectable(Unit, CodeBuilder.Indent(indent) + CodeBuilder.Assign(Unit.field.FieldName.VariableHighlight(), GenerateValue(Unit.value, data)));
            output += (Unit.exit.hasValidConnection ? "\n" : string.Empty) + GetNextUnit(Unit.exit, data, indent);
            return output;
        }
    
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if(Unit.actionDirection == ActionDirection.Get)
            {
                return Unit.field.FieldName.VariableHighlight();
            }
    
            return base.GenerateValue(output, data);
        }
    }
    
}