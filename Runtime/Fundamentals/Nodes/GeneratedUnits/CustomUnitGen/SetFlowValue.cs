using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

[UnitTitle("FlowSetValue")]//Unit title
[UnitCategory("Community/CodeGenerators/Unit")]
[TypeIcon(typeof(Flow))]//Unit icon
public class FlowSetValue : GeneratedUnit
{
    [DoNotSerialize]
    [PortLabelHidden]
    public ValueInput VariableName;

    [DoNotSerialize]
    [PortLabelHidden]
    public ValueInput Value;

    [DoNotSerialize]
    [PortLabelHidden]
    public ValueInput Flow;

    [UnitHeaderInspectable("Type")]
    public Type ValueType;

    protected override void Definition()
    {
        base.Definition();

        Flow = ValueInput<Flow>(nameof(Flow));
        VariableName = ValueInput(nameof(VariableName), "Port");
        Value = ValueInput<object>(nameof(Value));

        Requirement(VariableName, Enter);
        Requirement(Flow, Enter);
        Requirement(Value, Enter);
    }

    public override string GeneratorLogic(int indent)
    {
        return $"{GenerateValue(Flow)}.SetValue({GenerateValue(VariableName)}, {GenerateValue(Value)});";
    }
}