using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

[NodeGenerator(typeof(GetListItem))]
public class GetListItemGenerator : NodeGenerator<GetListItem>
{
    public GetListItemGenerator(Unit unit) : base(unit)
    {
    }

    public override string GenerateValue(ValueOutput output, ControlGenerationData data)
    {
        return new ValueCode(GenerateValue(Unit.list, data) + $"[{GenerateValue(Unit.index, data)}]", data.GetExpectedType(), data.GetExpectedType() != null);
    }
}
