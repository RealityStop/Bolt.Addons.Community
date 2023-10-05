using PlasticGui.Configuration.CloudEdition.Welcome;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

[UnitTitle("AssignControlInput")]//Unit title
[UnitCategory("Community/Code/Unit")]
[TypeIcon(typeof(Flow))]//Unit icon
public class AssignControlInput : Unit
{
    [DoNotSerialize]
    [PortLabelHidden]
    public ControlOutput Exit;

    [DoNotSerialize]
    [PortLabelHidden]
    public ControlInput Enter;

    [DoNotSerialize]
    [PortLabel("ControlInput")]
    [AllowsNull]
    public ValueInput _controlInput;

    [DoNotSerialize]
    public ValueInput MethodName;

    [DoNotSerialize]
    public ValueInput CoroutineMethodName;

    [UnitHeaderInspectable("Coroutine")]
    public bool Coroutine;

    protected override void Definition()
    {
        Enter = ControlInput(nameof(Enter), Logic);
        Exit = ControlOutput(nameof(Exit));
        _controlInput = ValueInput<ControlInput>(nameof(_controlInput)).AllowsNull();
        MethodName = ValueInput(nameof(MethodName), "Method");

        if (Coroutine)
        {
            CoroutineMethodName = ValueInput<string>(nameof(CoroutineMethodName), "CoroutineMethod");
            Requirement(CoroutineMethodName, Enter);
        }

        Requirement(_controlInput, Enter);
        Requirement(MethodName, Enter);
        Succession(Enter, Exit);
    }

    public ControlOutput Logic(Flow flow)
    {
        Debug.LogWarning("This node is only for the code generators to understand what to do it does not work in a normal graph");
        return Exit;
    }
}
