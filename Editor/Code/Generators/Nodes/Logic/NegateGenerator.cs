using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Negate))]
    public sealed class NegateGenerator : NodeGenerator<Negate>
    {
        public NegateGenerator(Negate unit) : base(unit)
        {
        }

        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input == Unit.input)
            {
                if (Unit.input.hasAnyConnection)
                {
                    return (Unit.input.connection.source.unit as Unit).GenerateValue(Unit.input.connection.source, data);
                }
            }

            return base.GenerateValue(input, data);
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {

            if (output == Unit.output)
            {
                return MakeClickableForThisUnit("!(") + GenerateValue(Unit.input, data) + MakeClickableForThisUnit(")");
            }

            return base.GenerateValue(output, data);
        }
    }
}