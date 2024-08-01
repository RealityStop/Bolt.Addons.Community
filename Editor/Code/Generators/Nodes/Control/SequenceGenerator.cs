using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

[NodeGenerator(typeof(Sequence))]
public class SequenceGenerator : NodeGenerator<Sequence>
{
    public SequenceGenerator(Unit unit) : base(unit)
    {
    }

    public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
    {
        var output = string.Empty;
        foreach (var controlOutput in Unit.multiOutputs)
        {
            if(controlOutput.hasValidConnection)
            {
                output += CodeUtility.MakeSelectable(Unit, GetNextUnit(controlOutput, data, indent)) + "\n";
            }
        }
        return output;
    }
}
