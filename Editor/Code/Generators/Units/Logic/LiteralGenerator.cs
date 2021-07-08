using Unity.VisualScripting;
using Bolt.Addons.Libraries.Humility;

namespace Bolt.Addons.Community.Generation
{
    [UnitGenerator(typeof(Literal))]
    public sealed class LiteralGenerator : UnitGenerator<Literal>
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