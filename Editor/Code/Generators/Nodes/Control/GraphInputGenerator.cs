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
            _output += $"/* Missing Value Input: {output.key} */";
        }

        return _output;
    }
}