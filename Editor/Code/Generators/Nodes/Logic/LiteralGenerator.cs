using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Literal))]
    public sealed class LiteralGenerator : NodeGenerator<Literal>
    {
        public LiteralGenerator(Literal unit) : base(unit)
        {

        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if (Unit.value != null)
                NameSpace = Unit.value.GetType().Namespace;
            return Unit.value.As().Code(true, Unit, true, true, "", false, true);
        }
    }
}