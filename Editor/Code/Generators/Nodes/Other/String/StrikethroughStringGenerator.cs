namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(StrikethroughString))]
    public class StrikethroughStringGenerator : NodeGenerator<StrikethroughString>
    {
        public StrikethroughStringGenerator(Unit unit) : base(unit) { }
        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.CallCSharpUtilityMethod("StrikethroughString", writer.Action(() =>
            {
                using (data.Expect(typeof(string)))
                    GenerateValue(Unit.Value, data, writer);
            }));
        }
    }
}