using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Literal))]
    public sealed class LiteralGenerator : NodeGenerator<Literal>
    {
        public LiteralGenerator(Literal unit) : base(unit)
        {
        }

        public override IEnumerable<string> GetNamespaces()
        {
            var @namespace = Unit.type?.Namespace ?? Unit.value.GetType().Namespace;

            if (!string.IsNullOrEmpty(@namespace))
                yield return @namespace;
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (data.GetExpectedType()?.IsAssignableFrom(Unit.type) ?? false)
            {
                data.MarkExpectedTypeMet(Unit.type);
            }

            var fromType = Unit.type;
            var toType = data.GetExpectedType();

            data.CreateSymbol(Unit, Unit.type);

            var code = Unit.value.As().Code(true, true, true, "", false, true);

            if (toType != null && fromType != null)
            {
                code = TypeConversionUtility.CastTo(code, fromType, toType);
            }
            writer.Write(code);
        }
    }
}