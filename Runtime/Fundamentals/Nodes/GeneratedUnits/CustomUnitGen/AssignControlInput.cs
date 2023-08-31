using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

[UnitTitle("AssignControlInput")]//Unit title
[UnitCategory("Community/CodeGenerators/Unit")]
[TypeIcon(typeof(Flow))]//Unit icon
public class AssignControlInput : GeneratedUnit
{
    [DoNotSerialize]
    [PortLabel("ControlInput")]
    public ValueInput VariableName;

    [DoNotSerialize]
    public ValueInput MethodName;

    [DoNotSerialize]
    public ValueInput CoroutineMethodName;

    [UnitHeaderInspectable("Coroutine")]
    public bool Coroutine;

    protected override void Definition()
    {
        base.Definition();
        VariableName = ValueInput(nameof(VariableName), "Input");
        MethodName = ValueInput(nameof(MethodName), "Method");

        if (Coroutine) 
        {
            CoroutineMethodName = ValueInput<string>(nameof(CoroutineMethodName), "CoroutineMethod");
            Requirement(CoroutineMethodName, Enter);
        }

        Requirement(VariableName, Enter);
        Requirement(MethodName, Enter);
    }

    public override string GeneratorLogic(int indent)
    {
        if (!Coroutine)
        {
            return $"{GenerateValue(VariableName)} = ControlInput(nameof({GenerateValue(VariableName)}), {(GenerateValue(MethodName).Length == 0 ? "/* Requires Method Name */": $"{GenerateValue(MethodName)}")}); \n";
        }
        else 
        {
            if (string.IsNullOrEmpty(GenerateValue(MethodName)) || string.IsNullOrWhiteSpace(GenerateValue(MethodName)))
            {
                return $"{GenerateValue(VariableName)} = ControlInputCoroutine(nameof({GenerateValue(VariableName)}), {(GenerateValue(CoroutineMethodName).Length == 0 ? "/* Requires CoroutineMethod Name */": $"{GenerateValue(CoroutineMethodName)}")}); \n";
            }
            else 
            {
                return $"{GenerateValue(VariableName)} = ControlInputCoroutine(nameof({GenerateValue(VariableName)}), {GenerateValue(MethodName)}, {(GenerateValue(CoroutineMethodName).Length == 0 ? "/* Requires CoroutineMethod Name */" : $"{GenerateValue(CoroutineMethodName)}")}); \n";
            }
        }
    }
}
