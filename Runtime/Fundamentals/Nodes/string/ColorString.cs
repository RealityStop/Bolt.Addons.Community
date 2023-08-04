using Bolt.Addons.Community.Fundamentals;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[UnitTitle("Color")]//Unit title
[UnitCategory("Community\\Utility\\string")]
[TypeIcon(typeof(string))]//Unit icon
public class ColorString : Unit
{

    [UnitHeaderInspectable("Color")]
    [Inspectable]
    public TextColor color;

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
        var _color = color.ToString();

        var value = flow.GetValue(Value);

        var NewValue = $"<color={_color}>" + value + "</color>";

        return NewValue;
    }
}

public enum TextColor
{
    Black,
    Blue,
    Green,
    Red,
    White,
    Yellow
}
