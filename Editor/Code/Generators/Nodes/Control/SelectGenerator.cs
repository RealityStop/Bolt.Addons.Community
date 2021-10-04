using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(SelectUnit))]
    public sealed class SelectGenerator : NodeGenerator<SelectUnit>
    {
        public SelectGenerator(Unit unit) : base(unit)
        {
        }

        public override string GenerateValue(ValueOutput output)
        {
            if (output == Unit.selection)
            {
                var str = string.Empty;
                var @true = base.GenerateValue(Unit.ifTrue);
                var @false = base.GenerateValue(Unit.ifFalse);
                var condition = base.GenerateValue(Unit.condition);

                if (Unit.condition.hasValidConnection)
                {
                    condition = GenerateValue(Unit.condition);
                }

                if (Unit.ifTrue.hasValidConnection)
                {
                    @true = GenerateValue(Unit.ifTrue);
                }

                if (Unit.ifFalse.hasValidConnection)
                {
                    @false = GenerateValue(Unit.ifFalse);
                }

                str = "(" + condition + " ? " + @true + " : " + @false + ")";
                return str;
            }

            return base.GenerateValue(output);
        }

        public override string GenerateValue(ValueInput input)
        {
            var @true = base.GenerateValue(Unit.ifTrue);
            var @false = base.GenerateValue(Unit.ifFalse);
            var condition = base.GenerateValue(Unit.condition);

            if (input == Unit.condition)
            {
                condition = ((Unit)Unit.condition.connection.source.unit).GenerateValue(Unit.condition.connection.source);
                return condition;
            }

            if (input == Unit.ifTrue)
            {
                @true = ((Unit)Unit.ifTrue.connection.source.unit).GenerateValue(Unit.ifTrue.connection.source);
                return @true;
            }

            if (input == Unit.ifFalse)
            {
                @false = ((Unit)Unit.ifFalse.connection.source.unit).GenerateValue(Unit.ifFalse.connection.source);
                return @false;
            }

            return base.GenerateValue(input);
        }
    }
}