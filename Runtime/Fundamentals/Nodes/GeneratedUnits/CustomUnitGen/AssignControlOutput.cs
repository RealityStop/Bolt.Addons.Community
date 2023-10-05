using Unity.VisualScripting;
using UnityEngine;

[UnitTitle("AssignControlOutput")]
[UnitCategory("Community/Code/Unit")]
[TypeIcon(typeof(Flow))]
public class AssignControlOutput : Unit
{
    [DoNotSerialize]
    [PortLabelHidden]
    public ControlOutput Exit;

    [DoNotSerialize]
    [PortLabelHidden]
    public ControlInput Enter;

    [DoNotSerialize]
    public ValueInput controlOutput;

    protected override void Definition()
    {
        Enter = ControlInput(nameof(Enter), Logic);
        Exit = ControlOutput(nameof(Exit));
        controlOutput = ValueInput<ControlOutput>(nameof(controlOutput));
        Succession(Enter, Exit);
    }

    public ControlOutput Logic(Flow flow)
    {
        Debug.LogWarning("This node is only for the code generators to understand what to do it does not work in a normal graph");
        return Exit;
    }
}