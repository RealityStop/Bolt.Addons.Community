using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Negate))]
    public sealed class NegateGenerator : NodeGenerator<Negate>
    {
        public NegateGenerator(Negate unit) : base(unit)
        {
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input == Unit.input)
            {
                if (Unit.input.hasAnyConnection)
                {
                    return (Unit.input.connection.source.unit as Unit).GenerateValue(Unit.input.connection.source);
                }
            }

            return base.GenerateValue(input);
        }

        public override string GenerateValue(ValueOutput output)
        {
            if (output == Unit.output)
            {
                return "!" + GenerateValue(Unit.input);
            }

            return base.GenerateValue(output);
        }
    }
}