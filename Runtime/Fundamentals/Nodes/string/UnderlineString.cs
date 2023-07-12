using Bolt.Addons.Community.Fundamentals;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[UnitTitle("Underline")]//Unit title
[UnitCategory("Community\\String")]
[TypeIcon(typeof(string))]//Unit icon
public class UnderlineString : Unit
{
    
    [DoNotSerialize]
    [PortLabelHidden]
    public ValueInput Value;
    [DoNotSerialize]
    [PortLabelHidden]
    public ValueOutput Result;
    protected override void Definition()
    {
        Value = ValueInput<string>(nameof(Value), default);
        Result = ValueOutput<string>(nameof(Result), Enter_);
    }

    public string Enter_(Flow flow)
    {
        var value = flow.GetValue(Value);

        var NewValue = "<u>" + value + "</u>";

        return NewValue;
    }
}