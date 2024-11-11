using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
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
            output += CodeBuilder.Indent(indent) + MakeSelectableForThisUnit("this".ConstructHighlight() + "." + Unit.member.name.VariableHighlight() + " = ") + GenerateValue(Unit.value, data) + MakeSelectableForThisUnit(";");
            output += "\n" + GetNextUnit(Unit.exit, data, indent);
            return output;
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if (Unit.actionDirection == ActionDirection.Get)
            {
                return MakeSelectableForThisUnit("this".ConstructHighlight() + "." + Unit.member.name.VariableHighlight());
            }

            return base.GenerateValue(output, data);
        }

        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input.hasValidConnection)
            {
                return GetNextValueUnit(input, data);
            }
            else if (input.hasDefaultValue)
            {
                return MakeSelectableForThisUnit(input.unit.defaultValues[input.key].As().Code(true, true, true, "", false));
            }
            else
            {
                return $"/* \"{input.key} Requires Input\" */".WarningHighlight();
            }
        }
    }
}