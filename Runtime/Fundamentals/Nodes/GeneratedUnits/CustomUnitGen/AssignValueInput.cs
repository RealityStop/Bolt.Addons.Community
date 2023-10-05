using System;
using Unity.VisualScripting;
using UnityEngine;

[UnitTitle("AssignValueInput")]
[UnitCategory("Community/Code/Unit")]
[TypeIcon(typeof(Flow))]
public class AssignValueInput : Unit
{
    [DoNotSerialize]
    [PortLabelHidden]
    public ControlOutput Exit;

    [DoNotSerialize]
    [PortLabelHidden]
    public ControlInput Enter;

    [DoNotSerialize]
    [PortLabelHidden]
    [AllowsNull]
    public ValueInput Input;

    [DoNotSerialize]
    [PortLabel("NullMeansSelf")]
    public ValueInput NullMeansSelf;

    [DoNotSerialize]
    [PortLabel("Default")]
    public ValueInput Default;

    [UnitHeaderInspectable("Type")]
    public Type VariableType = typeof(float);

    [UnitHeaderInspectable("HasDefault")]
    public bool DefaultValue;

    protected override void Definition()
    {
        Enter = ControlInput(nameof(Enter), Logic);
        Exit = ControlOutput(nameof(Exit));

        Input = ValueInput<ValueInput>(nameof(Input), default).AllowsNull();

        if (DefaultValue)
        {
            Default = ValueInput(VariableType, nameof(Default));

            if (VariableType != typeof(Color))
            {
                Default.SetDefaultValue(VariableType.PseudoDefault());
            }
        }

        if (VariableType == typeof(GameObject))
        {
            NullMeansSelf = ValueInput<bool>(nameof(NullMeansSelf), default);
        }

        Requirement(Input, Enter);
        Succession(Enter, Exit);
    }

    public ControlOutput Logic(Flow flow)
    {
        Debug.LogWarning("This node is only for the code generators to understand what to Generate it does not work in a normal graph");
        return Exit;
    }
}