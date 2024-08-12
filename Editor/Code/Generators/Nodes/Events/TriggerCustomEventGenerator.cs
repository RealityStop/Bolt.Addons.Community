using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

[NodeGenerator(typeof(TriggerCustomEvent))]
public class TriggerCustomEventGenerator : NodeGenerator<TriggerCustomEvent>
{
    public TriggerCustomEventGenerator(Unit unit) : base(unit)
    {
    }

    public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
    {
        var output = string.Empty;
        var customEvent = typeof(CustomEvent).As().CSharpName(false, true);
        data.SetExpectedType(typeof(GameObject));
        output += CodeBuilder.Indent(indent) + CodeUtility.MakeSelectable(Unit, customEvent + ".Trigger(" + GenerateValue(Unit.target, data) + $", {GenerateValue(Unit.name, data)}{(Unit.argumentCount > 0 ? ", " : "")}{string.Join(", ", Unit.arguments.Select(arg => GenerateValue(arg, data)))}" + ");") + "\n";
        data.SetExpectedType(null);
        output += GetNextUnit(Unit.exit, data, indent);
        return output;
    }

    public override string GenerateValue(ValueInput input, ControlGenerationData data)
    {
        if (input == Unit.target && !Unit.target.hasValidConnection)
        {
            return "gameObject".VariableHighlight();
        }
        return base.GenerateValue(input, data);
    }
}
