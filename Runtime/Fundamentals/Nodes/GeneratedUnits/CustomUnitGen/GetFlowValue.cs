using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

[UnitTitle("FlowGetValue")]//Unit title
[UnitCategory("Community/CodeGenerators/Unit")]
[TypeIcon(typeof(Flow))]//Unit icon
public class FlowGetValue : GeneratedUnit
{
    [DoNotSerialize]
    [PortLabelHidden]
    public ValueInput VariableName;

    [DoNotSerialize]
    [PortLabelHidden]
    public ValueOutput VariableResult;

    [UnitHeaderInspectable("Type")]
    public Type ValueType;

    protected override void Definition()
    {
        base.Definition();

        VariableName = ValueInput<string>(nameof(VariableName), default);
        VariableResult = ValueOutput<object>(nameof(VariableResult));

        Requirement(VariableName, Enter);
    }

    public override string GeneratorLogic(int indent)
    {
        return "";
    }

    public override string GeneratorOutput(ValueOutput output)
    {
        return $"flow.GetValue<{ValueType.CSharpFullName()}>({GenerateValue(VariableName)}){(Enter.hasValidConnection ? ";" : "")}";
    }
}