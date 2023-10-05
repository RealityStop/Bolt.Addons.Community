using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

[UnitTitle("AsType")]
[TypeIcon(typeof(object))]
public class AsUnit : Unit
{
    [DoNotSerialize]
    [PortLabelHidden]
    public ValueInput value;

    [DoNotSerialize]
    [PortLabelHidden]
    public ValueOutput AsValue;

    [UnitHeaderInspectable]
    public Type AsType = typeof(float);

    public override bool canDefine => AsType != null;

    protected override void Definition()
    {
        value = ValueInput<object>(nameof(value));
        AsValue = ValueOutput(AsType, nameof(AsValue), GetValue);
    }

    private object GetValue(Flow flow)
    {
        return ConversionUtility.Convert(flow.GetValue(value), AsType);
    }
}
