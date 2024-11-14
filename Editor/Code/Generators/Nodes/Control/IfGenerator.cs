using System.Text;
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
            var output = new StringBuilder();
            string cachedIndent = CodeBuilder.Indent(indent);
            string cachedIndentPlusOne = CodeBuilder.Indent(indent + 1);

            var trueData = new ControlGenerationData(data);
            var falseData = new ControlGenerationData(data);
            string trueCode = "";

            if (input == Unit.enter)
            {
                // Constructing the "if" statement
                output.Append(cachedIndent)
                      .Append(MakeSelectableForThisUnit("if".ConstructHighlight() + " ("))
                      .Append(GenerateValue(Unit.condition, data))
                      .Append(MakeSelectableForThisUnit(")"))
                      .AppendLine()
                      .Append(cachedIndent)
                      .AppendLine(MakeSelectableForThisUnit("{"));

                // Handling the true branch
                // if (!ShouldSkipTrueBranch())
                // {
                    trueData.NewScope();
                    trueCode = GetNextUnit(Unit.ifTrue, trueData, indent + 1);
                    trueData.ExitScope();
                    output.Append(trueCode);
                // }
                // else
                // {
                //     trueData.NewScope();
                //     trueCode = CodeUtility.ToolTip("Unreachable Code", GetNextUnit(Unit.ifTrue, trueData, indent + 1).NamespaceHighlight());
                //     trueData.ExitScope();
                //     output.Append(trueCode);
                // }

                output.AppendLine()
                      .Append(cachedIndent)
                      .AppendLine(MakeSelectableForThisUnit("}"));

                if (!Unit.ifFalse.hasAnyConnection)
                {
                    output.Append("\n");
                }

                if (Unit.ifFalse.hasAnyConnection)
                {
                    output.Append(cachedIndent)
                          .Append(MakeSelectableForThisUnit("else".ConstructHighlight()));

                    if (!Unit.ifTrue.hasValidConnection || string.IsNullOrEmpty(trueCode))
                    {
                        output.Append(MakeSelectableForThisUnit(CodeBuilder.MakeRecommendation(
                            "You should use the negate node and connect the true input instead")));
                    }

                    output.AppendLine()
                          .Append(cachedIndent)
                          .AppendLine(MakeSelectableForThisUnit("{"));

                    // if (!ShouldSkipFalseBranch())
                    // {
                        falseData.NewScope();
                        output.Append(GetNextUnit(Unit.ifFalse, falseData, indent + 1));
                        falseData.ExitScope();
                    // }
                    // else
                    // {
                    //     output.Append(cachedIndentPlusOne)
                    //           .AppendLine(MakeSelectableForThisUnit("/* Unreachable code skipping for performance */".WarningHighlight()));
                    // }

                    output.AppendLine()
                          .Append(cachedIndent)
                          .AppendLine(MakeSelectableForThisUnit("}") + "\n");
                }
            }

            // Updating the must-break status
            data.hasBroke = trueData.hasBroke && falseData.hasBroke;

            return output.ToString();
        }

        private bool ShouldSkipTrueBranch()
        {
            if (!Unit.condition.hasValidConnection) return false;

            if (Unit.condition.connection.source.unit is Literal literal && (bool)literal.value == false)
                return true;

            if (Unit.condition.GetPsudoSource() is ValueInput valueInput &&
                valueInput.hasDefaultValue &&
                valueInput.unit.defaultValues[valueInput.key] is bool condition &&
                condition == false)
            {
                return true;
            }

            return false;
        }

        private bool ShouldSkipFalseBranch()
        {
            if (!Unit.condition.hasValidConnection) return false;

            if (Unit.condition.connection.source.unit is Literal literal && (bool)literal.value == true)
                return true;

            if (Unit.condition.GetPsudoSource() is ValueInput valueInput &&
                valueInput.hasDefaultValue &&
                valueInput.unit.defaultValues[valueInput.key] is bool condition &&
                condition == true)
            {
                return true;
            }
            return false;
        }

        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input == Unit.condition)
            {
                if (Unit.condition.hasAnyConnection)
                {
                    return GetNextValueUnit(Unit.condition, data);
                }
            }

            return base.GenerateValue(input, data);
        }
    }
}