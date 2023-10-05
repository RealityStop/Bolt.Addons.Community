using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

[UnitTitle("YieldReturn")]
[UnitCategory("Community/Code")]
[TypeIcon(typeof(object))]
public class YieldReturn : Unit
{
    [DoNotSerialize]
    [PortLabelHidden]
    public ControlInput Return;

    [DoNotSerialize]
    [PortLabelHidden]
    public ValueInput value;

    protected override void Definition()
    {
        Return = ControlInput(nameof(Return), DoReturn);
        value = ValueInput<object>(nameof(value));
    }

    private ControlOutput DoReturn(Flow flow)
    {
        return null;
    }
}
