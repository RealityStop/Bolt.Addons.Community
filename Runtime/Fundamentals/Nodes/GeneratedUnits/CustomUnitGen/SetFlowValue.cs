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

    [UnitHeaderInspectable("Type")]
    public Type ValueType;

    protected override void Definition()
    {
        base.Definition();

        VariableName = ValueInput<string>(nameof(VariableName), default);
        Value = ValueInput<object>(nameof(Value));

        Requirement(VariableName, Enter);
    }

    public override ControlOutput Logic(Flow flow)
    {
        flow.SetValue(Value, "");
        return base.Logic(flow);
    }
    public override string GeneratorLogic(ControlGenerationData data, int indent, NodeGenerator generator)
    {
        return $"flow.SetValue({generator.GenerateValue(VariableName)}, {generator.GenerateValue(Value)});";
    }
}