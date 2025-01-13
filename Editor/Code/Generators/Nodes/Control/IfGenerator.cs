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
                if (!TrueIsUnreachable())
                {
                    trueData.NewScope();
                    trueCode = GetNextUnit(Unit.ifTrue, trueData, indent + 1);
                    trueData.ExitScope();
                    output.Append(trueCode);
                }
                else
                {
                    trueData.NewScope();
                    output.AppendLine(CodeBuilder.Indent(indent + 1) + MakeSelectableForThisUnit(CodeUtility.ToolTip($"The code in the 'True' branch is unreachable due to the output of the condition value: ({CodeUtility.CleanCode(GenerateValue(Unit.condition, data))}). Don't worry this does not break your code.", $"Unreachable Code in 'True' Branch: {Unit.ifTrue.key}", "")));
                    trueCode = GetNextUnit(Unit.ifTrue, trueData, indent + 1);
                    trueData.ExitScope();
                    output.Append(trueCode);
                }

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

                    if (!FalseIsUnreachable())
                    {
                        falseData.NewScope();
                        output.Append(GetNextUnit(Unit.ifFalse, falseData, indent + 1));
                        falseData.ExitScope();
                    }
                    else
                    {
                        falseData.NewScope();
                        output.AppendLine(CodeBuilder.Indent(indent + 1) + MakeSelectableForThisUnit(CodeUtility.ToolTip($"The code in the 'False' branch is unreachable due to the output of the condition value: ({CodeUtility.CleanCode(GenerateValue(Unit.condition, data))}). Don't worry this does not break your code.", $"Unreachable Code in 'False' Branch: {Unit.ifFalse.key}", "")));
                        output.Append(GetNextUnit(Unit.ifFalse, falseData, indent + 1));
                        falseData.ExitScope();
                    }

                    output.AppendLine()
                          .Append(cachedIndent)
                          .AppendLine(MakeSelectableForThisUnit("}") + "\n");
                }
            }

            // Updating the must-break status
            data.hasBroke = trueData.hasBroke && falseData.hasBroke;

            return output.ToString();
        }

        private bool TrueIsUnreachable()
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

        private bool FalseIsUnreachable()
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
                    data.SetExpectedType(typeof(bool));
                    var code = GetNextValueUnit(Unit.condition, data);
                    data.RemoveExpectedType();
                    return code;
                }
            }

            return base.GenerateValue(input, data);
        }
    }
}