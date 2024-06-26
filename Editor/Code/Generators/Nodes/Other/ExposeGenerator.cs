using Unity;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;
[NodeGenerator(typeof(Unity.VisualScripting.Expose))]
public sealed class ExposeGenerator : NodeGenerator<Unity.VisualScripting.Expose>
{
    public ExposeGenerator(Unity.VisualScripting.Expose unit) : base(unit)
    {
    }

    public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
    {
        return base.GenerateControl(input, data, indent);
    }

    public override string GenerateValue(ValueOutput output)
    {
        var _output = string.Empty;

        _output += GenerateValue(Unit.target) + "." + output.key;
        return _output;
    }


    public override string GenerateValue(ValueInput input)
    {
        if (input.hasValidConnection)
        {
            return input.connection.source.type == typeof(object) ? $"(({input.type.DisplayName().TypeHighlight()})" + (input.connection.source.unit as Unit).GenerateValue(input.connection.source) + ")" : string.Empty + (input.connection.source.unit as Unit).GenerateValue(input.connection.source);
        }
        else if (input.hasDefaultValue)
        {
            return unit.defaultValues[input.key].As().Code(false, true, true);
        }
        else
        {
            return $"/* {input.key} Requires Input ";
        }
    }
}