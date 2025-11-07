using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Literal))]
    public sealed class LiteralGenerator : NodeGenerator<Literal>
    {
        public LiteralGenerator(Literal unit) : base(unit)
        {

        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if (data.GetExpectedType()?.IsAssignableFrom(Unit.type) ?? false)
            {
                data.SetCurrentExpectedTypeMet(true, Unit.type);
            }
            if (Unit.value != null)
                NameSpaces = Unit.value.GetType().Namespace;
            var fromType = Unit.type;
            var toType = data.GetExpectedType();

            data.CreateSymbol(Unit, Unit.type);

            var code = Unit.value.As().Code(true, Unit, true, true, "", false, true);

            if (toType != null && fromType != null)
            {
                code = TypeConversionUtility.CastTo(code, fromType, toType, Unit);
            }

            return code;
        }
    }
}