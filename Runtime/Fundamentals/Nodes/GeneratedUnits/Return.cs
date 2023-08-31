using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;

[UnitTitle("Return")]//Unit title
[TypeIcon(typeof(object))]//Unit icon
public class Return : GeneratedUnit
{
    
    [DoNotSerialize]
    public ValueInput Data;
    protected override void Definition()
    {
        Enter = ControlInput(nameof(Enter), Logic);
        Data = ValueInput<object>(nameof(Data), default);
    }

    public override string GeneratorLogic(int indent)
    {
        return CodeBuilder.Indent(indent) + CodeBuilder.Highlight($"return", "FF6BE8") + $" {GenerateValue(Data)};";
    }
}