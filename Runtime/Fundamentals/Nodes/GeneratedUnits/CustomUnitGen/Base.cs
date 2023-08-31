using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

[UnitTitle("Base")]//Unit title
[UnitCategory("Community/CodeGenerators/Unit")]
[TypeIcon(typeof(Flow))]//Unit icon
public class Base : GeneratedUnit
{
    public string method = "YourMethod";//

    [DoNotSerialize]
    [PortLabel("MethodName")]
    public ValueInput Input;

    protected override void Definition()
    {
        base.Definition();

        Input = ValueInput<string>(nameof(Input));
        Input.SetDefaultValue(method);
    }


    public override string GeneratorLogic(ControlGenerationData data, int indent)
    {
        return $"base.{GenerateValue(Input)}(); \n";
    }
}