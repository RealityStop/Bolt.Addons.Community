using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
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
                output += CodeUtility.MakeSelectable(Unit, data.hasBroke || data.hasReturned ? GetNextUnit(controlOutput, data, indent).RemoveHighlights().RemoveMarkdown().NamespaceHighlight() : GetNextUnit(controlOutput, data, indent));
            }
        }
        return output;
    }
}