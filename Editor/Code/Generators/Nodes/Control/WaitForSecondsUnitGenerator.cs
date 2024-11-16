using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

[NodeGenerator(typeof(WaitForSecondsUnit))]
public class WaitForSecondsUnitGenerator : NodeGenerator<WaitForSecondsUnit>
{
    public WaitForSecondsUnitGenerator(Unit unit) : base(unit)
    {
    }

    public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
    {
        var output = string.Empty;
        data.hasReturned = true;
        output += CodeBuilder.Indent(indent) + MakeSelectableForThisUnit("yield return".ControlHighlight() + " " + "CSharpUtility".TypeHighlight() + ".CreateWaitForSeconds(") + GenerateValue(Unit.seconds, data) + MakeSelectableForThisUnit(", ") + GenerateValue(Unit.unscaledTime, data) + MakeSelectableForThisUnit(");") + "\n";
        output += GetNextUnit(Unit.exit, data, indent);
        return output;
    }
}
