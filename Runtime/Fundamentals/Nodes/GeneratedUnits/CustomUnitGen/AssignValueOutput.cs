using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

[UnitTitle("AssignValueOutput")]//Unit title
[UnitCategory("Community/CodeGenerators/Unit")]
[TypeIcon(typeof(Flow))]//Unit icon
public class AssignValueOutput : GeneratedUnit
{
    [DoNotSerialize]
    [PortLabelHidden]
    public ValueInput VariableName;

    [UnitHeaderInspectable("Type")]
    public Type VariableType;

    protected override void Definition()
    {
        base.Definition();
        VariableName = ValueInput<string>(nameof(VariableName), default);
    }

    public override string GeneratorLogic(ControlGenerationData data, int indent)
    {
        return $"{GenerateValue(VariableName)} = ValueOutput<{VariableType.CSharpFullName()}>(nameof({GenerateValue(VariableName)})); \n";
    }
}