using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

[UnitTitle("AssignControlOutput")]//Unit title
[UnitCategory("Community/CodeGenerators/Unit")]
[TypeIcon(typeof(Flow))]//Unit icon
public class AssignControlOutput : GeneratedUnit
{
    [DoNotSerialize]
    public ValueInput VariableName;

    protected override void Definition()
    {
        base.Definition();
        VariableName = ValueInput<string>(nameof(VariableName), default);
    }

    public override string GeneratorLogic(ControlGenerationData data, int indent, NodeGenerator generator)
    {
        return $"{generator.GenerateValue(VariableName)} = ControlOutput(nameof({generator.GenerateValue(VariableName)})); \n";
    }
}