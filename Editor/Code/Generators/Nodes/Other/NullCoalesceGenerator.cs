
namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(NullCoalesce))]
    public sealed class NullCoalesceGenerator : NodeGenerator<NullCoalesce>
    {
        public NullCoalesceGenerator(NullCoalesce unit) : base(unit) { }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            GenerateValue(Unit.input, data, writer);
            writer.Write(" ?? ");
            GenerateValue(Unit.fallback, data, writer);
        }
    }
}