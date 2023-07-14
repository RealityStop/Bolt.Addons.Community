using UnityEngine;
using Unity.VisualScripting;

[UnitTitle("Negate Value")] // Node title
[UnitCategory("Community\\Math")] // Node category
[TypeIcon(typeof(Negate))] // Node category
public class NegativeValueNode : Unit
{
    [DoNotSerialize]
    public ValueInput input;

    [DoNotSerialize]
    public ValueOutput output;

    protected override void Definition()
    {
        input = ValueInput<float>(nameof(input));
        output = ValueOutput(nameof(output), GetNegativeValue);

        Requirement(input, output);
    }

    private float GetNegativeValue(Flow flow)
    {
        // Get the value from the input port and negate it
        float value = flow.GetValue<float>(input);
        return -value;
    }
}
