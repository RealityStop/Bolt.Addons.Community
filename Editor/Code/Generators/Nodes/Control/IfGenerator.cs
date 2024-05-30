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

            var trueData = new ControlGenerationData(data);
            var falseData = new ControlGenerationData(data);

            if (input == Unit.enter)
            {
                output += CodeUtility.MakeSelectable(Unit, CodeBuilder.Indent(indent) + "if".ConstructHighlight() + " (" + GenerateValue(Unit.condition) + ")");
                output += "\n";
                output += CodeUtility.MakeSelectable(Unit, CodeBuilder.OpenBody(indent));
                output += "\n";
                trueData.NewScope();
                output += Unit.ifTrue.hasAnyConnection ? (Unit.ifTrue.connection.destination.unit as Unit).GenerateControl(Unit.ifTrue.connection.destination, trueData, indent + 1) : string.Empty;
                trueData.ExitScope();
                output += "\n";
                output += CodeUtility.MakeSelectable(Unit, CodeBuilder.CloseBody(indent));
                output += "\n";

                if (Unit.ifFalse.hasAnyConnection)
                {
                    output += "\n";
                    output += CodeUtility.MakeSelectable(Unit, CodeBuilder.Indent(indent) + "else".ConstructHighlight()) + (!Unit.ifTrue.hasValidConnection ? CodeBuilder.MakeRecommendation("You should use the negate node and connect the true input instead") : string.Empty);
                    output += "\n";
                    output += CodeUtility.MakeSelectable(Unit, CodeBuilder.OpenBody(indent));
                    output += "\n";
                    falseData.NewScope();
                    output += Unit.ifFalse.hasAnyConnection ? (Unit.ifFalse.connection.destination.unit as Unit).GenerateControl(Unit.ifFalse.connection.destination, falseData, indent + 1) : string.Empty;
                    falseData.ExitScope();
                    output += "\n";
                    output += CodeUtility.MakeSelectable(Unit, CodeBuilder.CloseBody(indent));
                    output += "\n";
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
                    return CodeUtility.MakeSelectable(input.connection.source.unit as Unit , (Unit.condition.connection.source.unit as Unit).GenerateValue(Unit.condition.connection.source));
                }
            }

            return base.GenerateValue(input);
        }
    }
}