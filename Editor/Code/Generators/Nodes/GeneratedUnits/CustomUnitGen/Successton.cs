using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

[UnitTitle("Succession")]//Unit title
[UnitCategory("Community/CodeGenerators/Unit")]
[TypeIcon(typeof(Flow))]//Unit icon
public class Succession : GeneratedUnit
{
    [DoNotSerialize]
    [PortLabel("ControlInput")]
    public ValueInput Input;

    [DoNotSerialize]
    [PortLabel("ControlOutput")]
    public ValueInput Output;

    protected override void Definition()
    {
        base.Definition();

        Input = ValueInput(nameof(Input), "YourInput");
        Output = ValueInput(nameof(Output), "YourOutput");

        Requirement(Input, Enter);
        Requirement(Output, Enter);
    }

    public override string GeneratorLogic(ControlGenerationData data, int indent, NodeGenerator generator)
    {
        return $"Succession({generator.GenerateValue(Input)}, {generator.GenerateValue(Output)}); \n";
    }
}