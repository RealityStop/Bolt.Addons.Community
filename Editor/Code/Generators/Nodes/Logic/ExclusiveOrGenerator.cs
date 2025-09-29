using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(ExclusiveOr))]
    public sealed class ExclusiveOrGenerator : NodeGenerator<ExclusiveOr>
    {
        public ExclusiveOrGenerator(ExclusiveOr unit) : base(unit)
        {
        }

        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input == Unit.a)
            {
                if (Unit.a.hasAnyConnection)
                {
                    return (Unit.a.connection.source.unit as Unit).GenerateValue(Unit.a.connection.source, data);
                }
            }

            if (input == Unit.b)
            {
                if (Unit.b.hasAnyConnection)
                {
                    return (Unit.b.connection.source.unit as Unit).GenerateValue(Unit.b.connection.source, data);
                }
            }

            return base.GenerateValue(input, data);
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {

            if (output == Unit.result)
            {
                return MakeClickableForThisUnit("(") + GenerateValue(Unit.a, data) + MakeClickableForThisUnit(" ^ ") + GenerateValue(Unit.b, data) + MakeClickableForThisUnit(")");
            }

            return base.GenerateValue(output, data);
        }
    }
}