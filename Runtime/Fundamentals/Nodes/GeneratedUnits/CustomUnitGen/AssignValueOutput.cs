using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

[UnitTitle("AssignValueOutput")]
[UnitCategory("Community/Code/Unit")]
[TypeIcon(typeof(Flow))]
public class AssignValueOutput : Unit
{
    [DoNotSerialize]
    [PortLabelHidden]
    public ControlOutput Exit;

    [DoNotSerialize]
    [PortLabelHidden]
    public ControlInput Enter;

    [DoNotSerialize]
    [PortLabelHidden]
    public ValueInput valueOutput;

    [DoNotSerialize]
    [PortLabelHidden]
    public ValueInput MethodName;

    [UnitHeaderInspectable("Type")]
    public Type VariableType = typeof(object);

    [UnitHeaderInspectable("TriggersMethod")]
    public bool triggersMethod;

    protected override void Definition()
    {
        Enter = ControlInput(nameof(Enter), Logic);
        Exit = ControlOutput(nameof(Exit));
        valueOutput = ValueInput<ValueOutput>(nameof(valueOutput));

        if (triggersMethod)
        {
            MethodName = ValueInput(nameof(MethodName), "Method");
        }

        Succession(Enter, Exit);
    }

    public ControlOutput Logic(Flow flow)
    {
        Debug.LogWarning("This node is only for the code generators to understand what to do it does not work in a normal graph");
        return Exit;
    }
}