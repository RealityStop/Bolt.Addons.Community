using System;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;

[TypeIcon(typeof(CodeBuilder))]
[UnitCategory("Community/Code")]
public class CodeLiteral : GeneratedUnit
{
    [DoNotSerialize]
    [PortLabelHidden]
    public ValueOutput code;

    [Inspectable]
    [InspectorTextArea()]
    [UnitHeaderInspectable]
    public string CodeValue;

    [Inspectable]
    public Type type = typeof(object);

    protected override void Definition()
    {
        code = ValueOutput(type, nameof(code), GetCode);
    }

    private object GetCode(Flow flow)
    {
        return CodeValue;
    }

    public override string GeneratorOutput(ValueOutput output)
    {
        return CodeValue;
    }
}
