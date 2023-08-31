using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

[UnitTitle("InvokeFlow")]//Unit title
[UnitCategory("Community/CodeGenerators/Unit")]
[TypeIcon(typeof(Flow))]//Unit icon
public class InvokeFlow : GeneratedUnit
{

    [DoNotSerialize]
    [PortLabel("ControlOutput")]
    public ValueInput Output;

    protected override void Definition()
    {
        base.Definition();

        Output = ValueInput(nameof(Output), "YourOutput");

        Requirement(Output, Enter);
    }

    public override string GeneratorLogic(ControlGenerationData data, int indent)
    {
        return CodeBuilder.Indent(indent) + $"flow".ConstructHighlight() + CodeBuilder.Highlight(".Invoke", "FFFF4D") + $"({GenerateValue(Output)}); \n";
    }
}