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
            trueData.NewScope();
            var trueCode = GetNextUnit(Unit.ifTrue, trueData, indent + 1);
            trueData.ExitScope();

            if (input == Unit.enter)
            {
                output += CodeUtility.MakeSelectable(Unit, CodeBuilder.Indent(indent) + "if".ConstructHighlight() + " (" + GenerateValue(Unit.condition, data) + ")");
                output += "\n";
                output += CodeUtility.MakeSelectable(Unit, CodeBuilder.OpenBody(indent));
                output += "\n";
                output += trueCode;
                output += "\n";
                output += CodeUtility.MakeSelectable(Unit, CodeBuilder.CloseBody(indent));
                output += "\n";

                if (Unit.ifFalse.hasAnyConnection)
                {
                    output += CodeUtility.MakeSelectable(Unit, CodeBuilder.Indent(indent) + "else".ConstructHighlight()) + (!Unit.ifTrue.hasValidConnection || string.IsNullOrEmpty(trueCode) ? CodeBuilder.MakeRecommendation("You should use the negate node and connect the true input instead") : string.Empty);
                    output += "\n";
                    output += CodeUtility.MakeSelectable(Unit, CodeBuilder.OpenBody(indent));
                    output += "\n";
                    falseData.NewScope();
                    output += GetNextUnit(Unit.ifFalse, falseData, indent + 1);
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

        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input == Unit.condition)
            {
                if (Unit.condition.hasAnyConnection)
                {
                    return GetNextValueUnit(Unit.condition, data, true);
                }
            }

            return base.GenerateValue(input, data);
        }
    }
}