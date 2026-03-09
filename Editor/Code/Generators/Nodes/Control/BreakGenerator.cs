namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Break))]
    public sealed class BreakGenerator : NodeGenerator<Break>
    {
        public BreakGenerator(Break unit) : base(unit)
        {
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            data.SetHasBroke(true);

            if (input == Unit.enter)
            {
                writer.Break(WriteOptions.Indented);
            }
        }
    }
}