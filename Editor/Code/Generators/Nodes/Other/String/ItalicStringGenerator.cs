namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(ItalicString))]
    public class ItalicStringGenerator : NodeGenerator<ItalicString>
    {
        public ItalicStringGenerator(Unit unit) : base(unit) { }
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return MakeClickableForThisUnit($"$\"<i>{{") + GenerateValue(Unit.Value, data) + MakeClickableForThisUnit("}</i>\"");
        }
    }
}