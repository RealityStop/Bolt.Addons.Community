using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

[NodeGenerator(typeof(RemoveListItem))]
public class RemoveListItemGenerator : NodeGenerator<RemoveListItem>
{
    public RemoveListItemGenerator(Unit unit) : base(unit)
    {
    }

    public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
    {
        var output = string.Empty;
        output += CodeBuilder.Indent(indent) + CodeUtility.MakeSelectable(Unit, GenerateValue(Unit.listInput, data) + $".Remove({GenerateValue(Unit.item, data)});") + "\n";
        output += GetNextUnit(Unit.exit, data, indent); 
        return output;
    }
}
