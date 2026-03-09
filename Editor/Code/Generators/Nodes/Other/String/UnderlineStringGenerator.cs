namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(UnderlineString))]
    public class UnderlineStringGenerator : NodeGenerator<UnderlineString>
    {
        public UnderlineStringGenerator(Unit unit) : base(unit) { }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.CallCSharpUtilityMethod("UnderlineString", writer.Action(() =>
            {
                using (data.Expect(typeof(string)))
                    GenerateValue(Unit.Value, data, writer);
            }));
        }
    }
}