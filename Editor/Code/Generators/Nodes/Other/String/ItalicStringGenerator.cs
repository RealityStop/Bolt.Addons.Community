namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(ItalicString))]
    public class ItalicStringGenerator : NodeGenerator<ItalicString>
    {
        public ItalicStringGenerator(Unit unit) : base(unit) { }
        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.CallCSharpUtilityMethod("ItalicString", writer.Action(() =>
            {
                using (data.Expect(typeof(string)))
                    GenerateValue(Unit.Value, data, writer);
            }));
        }
    }
}