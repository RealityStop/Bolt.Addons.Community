using Unity;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;
[NodeGenerator(typeof(Unity.VisualScripting.GraphOutput))]
public sealed class GraphOutputGenerator : NodeGenerator<Unity.VisualScripting.GraphOutput>
{
    public GraphOutputGenerator(Unity.VisualScripting.GraphOutput unit) : base(unit)
    {
    }

    public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
    {
        var output = string.Empty;

        var controloutput = connectedGraphOutputs.FirstOrDefault(output => output.key.Equals(input.key, System.StringComparison.OrdinalIgnoreCase));
        if (controloutput != null)
        {
            output += GetNextUnit(controloutput, data, indent);
        }

        return output;
    }


    public override string GenerateValue(ValueOutput output, ControlGenerationData data)
    {
        return GenerateValue(Unit.valueInputs[output.key], data);
    }
}