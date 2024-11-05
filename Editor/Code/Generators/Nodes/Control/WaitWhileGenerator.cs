using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

[NodeGenerator(typeof(WaitWhileUnit))]
public class WaitWhileGenerator : NodeGenerator<WaitWhileUnit>
{
    public WaitWhileGenerator(Unit unit) : base(unit)
    {
    }

    public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
    {
        var output = string.Empty;
        data.SetExpectedType(typeof(bool));
        var condition = GenerateValue(Unit.condition, data);
        data.RemoveExpectedType();
        output += CodeBuilder.Indent(indent) + MakeSelectableForThisUnit("yield return ".ControlHighlight() + "new ".ConstructHighlight() + "WaitWhile".TypeHighlight() + "(() => {") + condition + MakeSelectableForThisUnit(");") + "\n";
        output += GetNextUnit(Unit.exit, data, indent);
        return output;
    }
}
