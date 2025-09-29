namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(BoldString))]
    public class BoldStringGenerator : NodeGenerator<BoldString>
    {
        public BoldStringGenerator(Unit unit) : base(unit) { }
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return MakeClickableForThisUnit($"$\"<b>{{") + GenerateValue(Unit.Value, data) + MakeClickableForThisUnit("}</b>\"");
        }
    }
}