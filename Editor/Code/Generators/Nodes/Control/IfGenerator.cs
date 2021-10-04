using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(If))]
    public sealed class IfGenerator : NodeGenerator<If>
    {
        public IfGenerator(If unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;

            var trueData = new ControlGenerationData() { mustBreak = data.mustBreak, mustReturn = data.mustReturn, returns = data.returns };
            var falseData = new ControlGenerationData() { mustBreak = data.mustBreak, mustReturn = data.mustReturn, returns = data.returns };

            if (input == Unit.enter)
            {
                output += CodeBuilder.Indent(indent) + "if".ConstructHighlight() + " (" + GenerateValue(Unit.condition) + ")";
                output += "\n";
                output += CodeBuilder.OpenBody(indent);
                output += "\n";
                output += (Unit.ifTrue.hasAnyConnection ? (Unit.ifTrue.connection.destination.unit as Unit).GenerateControl(Unit.ifTrue.connection.destination, trueData, indent + 1) : string.Empty);
                output += "\n";
                output += CodeBuilder.CloseBody(indent);

                if (Unit.ifFalse.hasAnyConnection)
                {
                    output += "\n";
                    output += CodeBuilder.Indent(indent) + "else".ConstructHighlight();
                    output += "\n";
                    output += CodeBuilder.OpenBody(indent);
                    output += "\n";
                    
                    output += (Unit.ifFalse.hasAnyConnection ? (Unit.ifFalse.connection.destination.unit as Unit).GenerateControl(Unit.ifFalse.connection.destination, falseData, indent + 1) : string.Empty);
                    output += "\n";
                    output += CodeBuilder.CloseBody(indent);
                }
            }

            if (data.mustBreak)
            {
                if (!trueData.hasBroke || !falseData.hasBroke)
                {
                    data.hasBroke = false;
                }
                else
                {
                    data.hasBroke = true;
                }
            }

            return output;
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input == Unit.condition)
            {
                if (Unit.condition.hasAnyConnection)
                {
                    return (Unit.condition.connection.source.unit as Unit).GenerateValue(Unit.condition.connection.source);
                }
            }

            return base.GenerateValue(input);
        }
    }
}