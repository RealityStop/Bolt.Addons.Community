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
            output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit(Unit.field.FieldName.VariableHighlight() + " = ") + GenerateValue(Unit.value, data) + MakeClickableForThisUnit(";");
            output += "\n" + GetNextUnit(Unit.exit, data, indent);
            return output;
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if (Unit.actionDirection == ActionDirection.Get)
            {
                return MakeClickableForThisUnit(Unit.field.FieldName.VariableHighlight());
            }

            return base.GenerateValue(output, data);
        }
    }

}