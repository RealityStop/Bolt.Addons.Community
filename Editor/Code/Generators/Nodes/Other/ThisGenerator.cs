using Unity;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;
[NodeGenerator(typeof(This))]
public sealed class ThisGenerator : NodeGenerator<This>
{
    public ThisGenerator(This unit) : base(unit)
    {
    }

    public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
    {
        return base.GenerateControl(input, data, indent);
    }

    public override string GenerateValue(ValueOutput output, ControlGenerationData data)
    {
        return MakeClickableForThisUnit("gameObject".VariableHighlight());
    }
}