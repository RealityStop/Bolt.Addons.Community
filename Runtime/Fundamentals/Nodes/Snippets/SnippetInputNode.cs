namespace Unity.VisualScripting.Community 
{
    [UnitTitle("Argument")]
    [TypeIcon(typeof(GraphInput))]
    [SpecialUnit]
    [RenamedFrom("SnippetInputNode")]
    public sealed class SnippetInputNode : Unity.VisualScripting.Unit
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
}