using Unity.VisualScripting;
using Unity.VisualScripting.Community;

[UnitTitle("Requirement")]//Unit title
[UnitCategory("Community/CodeGenerators/Unit")]
[TypeIcon(typeof(Flow))]//Unit icon
public class Requirement : GeneratedUnit
{
    [DoNotSerialize]
    [PortLabel("ValueInput")]
    public ValueInput Input;

    [DoNotSerialize]
    [PortLabel("ControlInput")]
    public ValueInput _ControlInput;

    protected override void Definition()
    {
        base.Definition();

        Input = ValueInput(nameof(Input), "YourInput");
        _ControlInput = ValueInput(nameof(_ControlInput), "YourInput");

        Requirement(Input, Enter);
        Requirement(_ControlInput, Enter);
    }

    public override string GeneratorLogic(ControlGenerationData data, int indent)
    {
        return $"Requirement({GenerateValue(Input)}, {GenerateValue(_ControlInput)}); \n";
    }
}