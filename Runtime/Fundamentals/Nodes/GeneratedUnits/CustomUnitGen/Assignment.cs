using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

[UnitTitle("Assignment")]//Unit title
[UnitCategory("Community/CodeGenerators/Unit")]
[TypeIcon(typeof(Flow))]//Unit icon
public class Assignment : GeneratedUnit
{
    [DoNotSerialize]
    [PortLabel("ControlInput")]
    public ValueInput Input;

    [DoNotSerialize]
    [PortLabel("ValueOutput")]
    public ValueInput Output;

    protected override void Definition()
    {
        base.Definition();

        Input = ValueInput(nameof(Input), "YourInput");
        Output = ValueInput(nameof(Output), "YourOutput");

        Requirement(Input, Enter);
        Requirement(Output, Enter);
    }

    public override string GeneratorLogic(ControlGenerationData data, int indent)
    {
        return $"Assignment({GenerateValue(Input)}, {GenerateValue(Output)}); \n";
    }
}