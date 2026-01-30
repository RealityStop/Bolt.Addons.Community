using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Negate))]
    public sealed class NegateGenerator : NodeGenerator<Negate>
    {
        public NegateGenerator(Negate unit) : base(unit)
        {
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (output == Unit.output)
            {
                writer.Write("!(");
                GenerateValue(Unit.input, data, writer);
                writer.Write(")");
            }
        }
    }
}