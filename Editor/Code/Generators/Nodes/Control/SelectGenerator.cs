using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(SelectUnit))]
    public sealed class SelectGenerator : NodeGenerator<SelectUnit>
    {
        public SelectGenerator(Unit unit) : base(unit)
        {
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {

            if (output == Unit.selection)
            {
                var str = string.Empty;
                var @true = base.GenerateValue(Unit.ifTrue, data);
                var @false = base.GenerateValue(Unit.ifFalse, data);
                var condition = base.GenerateValue(Unit.condition, data);

                if (Unit.condition.hasValidConnection)
                {
                    condition = GenerateValue(Unit.condition, data);
                }

                if (Unit.ifTrue.hasValidConnection)
                {
                    @true = GenerateValue(Unit.ifTrue, data);
                }

                if (Unit.ifFalse.hasValidConnection)
                {
                    @false = GenerateValue(Unit.ifFalse, data);
                }

                str = MakeClickableForThisUnit("(") + condition + MakeClickableForThisUnit(" ? ") + @true + MakeClickableForThisUnit(" : ") + @false + MakeClickableForThisUnit(")");
                return str;
            }

            return base.GenerateValue(output, data);
        }

        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            var @true = base.GenerateValue(Unit.ifTrue, data);
            var @false = base.GenerateValue(Unit.ifFalse, data);
            var condition = base.GenerateValue(Unit.condition, data);

            if (input == Unit.condition)
            {
                condition = ((Unit)Unit.condition.connection.source.unit).GenerateValue(Unit.condition.connection.source, data);
                return condition;
            }

            if (input == Unit.ifTrue)
            {
                @true = ((Unit)Unit.ifTrue.connection.source.unit).GenerateValue(Unit.ifTrue.connection.source, data);
                return @true;
            }

            if (input == Unit.ifFalse)
            {
                @false = ((Unit)Unit.ifFalse.connection.source.unit).GenerateValue(Unit.ifFalse.connection.source, data);
                return @false;
            }

            return base.GenerateValue(input, data);
        }
    }
}