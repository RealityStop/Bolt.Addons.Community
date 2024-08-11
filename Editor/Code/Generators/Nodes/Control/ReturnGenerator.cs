using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

[NodeGenerator(typeof(EventReturn))]
public class ReturnGenerator : NodeGenerator<EventReturn>
{
    public ReturnGenerator(Unit unit) : base(unit)
    {
    }

    public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
    {
        string output = string.Empty;
        if(input == Unit.enter)
        {
            data.hasReturned = true;
            output += CodeBuilder.Indent(indent) + CodeUtility.MakeSelectable(Unit, CodeBuilder.Return(GenerateValue(Unit.value, data)));
            return output;
        }
        return base.GenerateControl(input, data, indent);
    }
}
