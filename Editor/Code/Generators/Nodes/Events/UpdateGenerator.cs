using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

[NodeGenerator(typeof(Update))]
public class UpdateGenerator : NodeGenerator<Update>
{
    public UpdateGenerator(Unit unit) : base(unit)
    {
    }

    public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
    {
        return GetNextUnit(Unit.trigger, new ControlGenerationData(), 0);
    }
}
