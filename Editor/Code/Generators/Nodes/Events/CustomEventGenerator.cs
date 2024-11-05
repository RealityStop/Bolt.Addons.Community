using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

[NodeGenerator(typeof(CustomEvent))]
public class CustomEventGenerator : NodeGenerator<CustomEvent>
{
    public CustomEventGenerator(Unit unit) : base(unit)
    {
    }

    public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
    {
        return GetNextUnit(Unit.trigger, data, indent);
    }

    public override string GenerateValue(ValueInput input, ControlGenerationData data)
    {
        if (input == Unit.target && !input.hasValidConnection)
            return MakeSelectableForThisUnit("gameObject".VariableHighlight());
        return base.GenerateValue(input, data);
    }

    public override string GenerateValue(ValueOutput output, ControlGenerationData data)
    {
        if (Unit.argumentPorts.Contains(output))
        {
            return MakeSelectableForThisUnit("args".VariableHighlight() + "." + "arguments".VariableHighlight() + $"[{Unit.argumentPorts.IndexOf(output)}]");
        }
        return base.GenerateValue(output, data);
    }
}
