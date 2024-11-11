using Unity;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;
[NodeGenerator(typeof(Unity.VisualScripting.GraphInput))]
public sealed class GraphInputGenerator : NodeGenerator<Unity.VisualScripting.GraphInput>
{
    public GraphInputGenerator(Unity.VisualScripting.GraphInput unit) : base(unit)
    {
    }

    public override string GenerateValue(ValueOutput output, ControlGenerationData data)
    {
        var _output = string.Empty;
        var ValueInput = connectedValueInputs.FirstOrDefault(valueInput => valueInput.key == output.key);

        if (ValueInput != null)
        {
            _output += GenerateValue(ValueInput, data);
        }
        else
        {
            var defaultValue = output.type.PseudoDefault();
            if (defaultValue == null)
                _output += MakeSelectableForThisUnit($"/* Missing Value Input: {output.key} */".WarningHighlight());
            else
            {
                _output += defaultValue.As().Code(true, unit, true, true, "", false, true);
            }
        }

        return _output;
    }
}