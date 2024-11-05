using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Literal))]
    public sealed class LiteralGenerator : NodeGenerator<Literal>
    {
        public LiteralGenerator(Literal unit) : base(unit)
        {
            if (Unit.value != null)
                NameSpace = Unit.value.GetType().Namespace;
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return Unit.value.As().Code(true, Unit, true, true, "");
        }
    }
}