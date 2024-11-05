using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

[NodeGenerator(typeof(Sequence))]
public class SequenceGenerator : NodeGenerator<Sequence>
{
    public SequenceGenerator(Unit unit) : base(unit) { }

    public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
    {
        // Use StringBuilder to avoid repeated string concatenation
        var outputBuilder = new StringBuilder();

        // Cache multiOutputs to avoid repeated access
        var outputs = Unit.multiOutputs;

        // Iterate only over connected outputs
        foreach (var controlOutput in outputs)
        {
            if (controlOutput.hasValidConnection)
            {
                outputBuilder.Append(GetNextUnit(controlOutput, data, indent));
            }
        }

        // Return the final generated string
        return outputBuilder.ToString();
    }
}