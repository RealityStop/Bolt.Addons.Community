namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(BoldString))]
    public class BoldStringGenerator : NodeGenerator<BoldString>
    {
        public BoldStringGenerator(Unit unit) : base(unit) { }
        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.CallCSharpUtilityMethod("BoldString", writer.Action(() =>
            {
                using (data.Expect(typeof(string)))
                    GenerateValue(Unit.Value, data, writer);
            }));
        }
    }
}