using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

[UnitTitle("AssignControlInput")]//Unit title
[UnitCategory("Community/CodeGenerators/Unit")]
[TypeIcon(typeof(Flow))]//Unit icon
public class AssignControlInput : GeneratedUnit
{
    [DoNotSerialize]
    public ValueInput VariableName;
    
    [DoNotSerialize]
    public ValueInput MethodName;
    
    protected override void Definition()
    {
        base.Definition();
        VariableName = ValueInput<string>(nameof(VariableName), default);
        MethodName = ValueInput<string>(nameof(MethodName), default);
    }

    public override string GeneratorLogic(ControlGenerationData data, int indent, NodeGenerator generator)
    {
        return $"{generator.GenerateValue(VariableName)} = ControlInput(nameof({generator.GenerateValue(VariableName)}), {generator.GenerateValue(MethodName)}); \n";
    }
}