using System;
using Unity.VisualScripting;

[UnitTitle("Argument")]//Unit title
[TypeIcon(typeof(GraphInput))]//Unit icon
[SpecialUnit]
public class SnippetInputNode : Unity.VisualScripting.Unit
{
    [Inspectable]
    [UnitHeaderInspectable("Name")]
    public string argumentName;
    [DoNotSerialize]
    [PortLabelHidden]
    public ValueOutput output;

    protected override void Definition()
    {
        output = ValueOutput<object>(nameof(Literal.output));
    }
}