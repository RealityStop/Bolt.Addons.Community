using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

[UnitTitle("CastType")]
[UnitCategory("Community/Code")]
[TypeIcon(typeof(object))]
public class Cast : Unit
{
    [DoNotSerialize]
    [PortLabelHidden]
    public ValueInput value;

    [DoNotSerialize]
    [PortLabelHidden]
    public ValueOutput castValue;

    [UnitHeaderInspectable("Type")]
    public Type CastType = typeof(float);

    public override bool canDefine => CastType != null;

    protected override void Definition()
    {
        value = ValueInput<object>(nameof(value));
        castValue = ValueOutput(CastType, nameof(castValue), GetValue);
    }

    private object GetValue(Flow flow)
    {
        return ConversionUtility.Convert(flow.GetValue(value), CastType);
    }
}
