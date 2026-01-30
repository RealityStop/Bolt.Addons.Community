namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(ReverseStringNode))]
    public class ReverseStringNodeGenerator : NodeGenerator<ReverseStringNode>
    {
        public ReverseStringNodeGenerator(Unit unit) : base(unit) { }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.CallCSharpUtilityMethod("ReverseString", writer.Action(() => GenerateValue(Unit.input, data, writer)));
        }
    }
}