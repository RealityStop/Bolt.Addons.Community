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
            NameSpace = Unit.value.GetType().Namespace;
        }

        public override string GenerateValue(ValueOutput output)
        {
            if (!(Unit.type.GenericTypeArguments.Length > 0))
            {
                return Unit.value.As().Code(true, true, true, "");
            }
            else 
            {
                return "new ".ConstructHighlight() + HUMType_Children.GenericDeclaration(Unit.type) + "()";
            }
        }
    }
}