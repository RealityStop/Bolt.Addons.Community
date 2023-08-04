using UnityEngine;
using Unity.VisualScripting;
using System.Numerics;
using UnityEngine.Windows;

[UnitTitle("Negate")] // Node title
[UnitCategory("Community\\Math")] // Node category
[TypeIcon(typeof(Negate))] // Node category
public class NegativeValueNode : Unit
{
    [UnitHeaderInspectable]
    [Inspectable]
    public NegateType type;
    [DoNotSerialize]
    public ValueInput Float;
    [DoNotSerialize]
    public ValueInput Int;
    [DoNotSerialize]
    public ValueInput Vector2;
    [DoNotSerialize]
    public ValueInput Vector3;

    [DoNotSerialize]
    public ValueOutput output;

    protected override void Definition()
    {
        switch (type)
        {
            case NegateType.Float:
                Float = ValueInput<float>(nameof(Float));
                break;
            case NegateType.Int:
                Int = ValueInput<int>(nameof(Int));
                break;
            case NegateType.Vector2:
                Vector2 = ValueInput<UnityEngine.Vector2>(nameof(Vector2));
                break;
            case NegateType.Vector3:
                Vector3 = ValueInput<UnityEngine.Vector3>(nameof(Vector3));
                break;
            default:
                Float = ValueInput<float>(nameof(Float));
                break;
        }

        output = ValueOutput(nameof(output), GetNegativeValue);
    }

    private object GetNegativeValue(Flow flow)
    {
        switch (type)
        {
            case NegateType.Float:
                float floatvalue = flow.GetValue<float>(Float);
                return -floatvalue;
            case NegateType.Int:
                int intvalue = flow.GetValue<int>(Int);
                return -intvalue;
            case NegateType.Vector2:
                UnityEngine.Vector2 vector2value = flow.GetValue<UnityEngine.Vector2>(Vector2);
                return -vector2value;
            case NegateType.Vector3:
                UnityEngine.Vector3 vector3value = flow.GetValue<UnityEngine.Vector3>(Vector3);
                return -vector3value;
            default:
                float value = flow.GetValue<float>(Float);
                return -value;
        }

    }
}

public enum NegateType
{
    Float,
    Int,
    Vector2,
    Vector3,
}
