using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Literal))]
    public sealed class LiteralGenerator : NodeGenerator<Literal>
    {
        public LiteralGenerator(Literal unit) : base(unit)
        {
        }

        public override string GenerateValue(ValueOutput output)
        {
            return Unit.value.As().Code(true, true, true, "");
        }
    }
}