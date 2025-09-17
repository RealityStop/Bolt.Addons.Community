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

            if (input == Unit.enter)
            {
                output.Append(CodeBuilder.Indent(indent))
                      .Append(MakeClickableForThisUnit("if".ConstructHighlight() + " ("))
                      .Append(GenerateValue(Unit.condition, data))
                      .Append(MakeClickableForThisUnit(")"))
                      .AppendLine()
                      .Append(CodeBuilder.Indent(indent))
                      .AppendLine(MakeClickableForThisUnit("{"));

                string trueCode;
                if (!TrueIsUnreachable())
                {
                    data.NewScope();
                    trueCode = GetNextUnit(Unit.ifTrue, data, indent + 1);
                    data.ExitScope();
                    output.Append(trueCode);
                }
                else
                {
                    data.NewScope();
                    output.AppendLine(CodeBuilder.Indent(indent + 1) + MakeClickableForThisUnit(CodeUtility.ToolTip($"The code in the 'True' branch is unreachable due to the output of the condition value: ({CodeUtility.CleanCode(GenerateValue(Unit.condition, data))}).", $"Unreachable Code in 'True' Branch: {Unit.ifTrue.key}", "")));
                    trueCode = GetNextUnit(Unit.ifTrue, data, indent + 1);
                    data.ExitScope();
                    output.Append(trueCode);
                }

                output.AppendLine()
                      .Append(CodeBuilder.Indent(indent))
                      .AppendLine(MakeClickableForThisUnit("}"));

                if (!Unit.ifFalse.hasAnyConnection)
                {
                    output.Append("\n");
                }

                if (Unit.ifFalse.hasAnyConnection)
                {
                    output.Append(CodeBuilder.Indent(indent))
                          .Append(MakeClickableForThisUnit("else".ConstructHighlight()));

                    if (!Unit.ifTrue.hasValidConnection || string.IsNullOrEmpty(trueCode))
                    {
                        output.Append(MakeClickableForThisUnit(CodeBuilder.MakeRecommendation(
                            "You should use the negate node and connect the true input instead")));
                    }

                    output.AppendLine()
                          .Append(CodeBuilder.Indent(indent))
                          .AppendLine(MakeClickableForThisUnit("{"));

                    if (!FalseIsUnreachable())
                    {
                        data.NewScope();
                        output.Append(GetNextUnit(Unit.ifFalse, data, indent + 1));
                        data.ExitScope();
                    }
                    else
                    {
                        data.NewScope();
                        output.AppendLine(CodeBuilder.Indent(indent + 1) + MakeClickableForThisUnit(CodeUtility.ToolTip($"The code in the 'False' branch is unreachable due to the output of the condition value: ({CodeUtility.CleanCode(GenerateValue(Unit.condition, data))}).", $"Unreachable Code in 'False' Branch: {Unit.ifFalse.key}", "")));
                        output.Append(GetNextUnit(Unit.ifFalse, data, indent + 1));
                        data.ExitScope();
                    }

                    output.AppendLine()
                          .Append(CodeBuilder.Indent(indent))
                          .AppendLine(MakeClickableForThisUnit("}") + "\n");
                }
            }

            return output.ToString();
        }

        private bool TrueIsUnreachable()
        {
            if (!Unit.condition.hasValidConnection) return false;

            if (Unit.condition.GetPesudoSource().unit is Literal literal && (bool)literal.value == false)
                return true;

            if (Unit.condition.GetPesudoSource() is ValueInput valueInput &&
                valueInput.hasDefaultValue && !valueInput.hasValidConnection &&
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

            if (Unit.condition.GetPesudoSource().unit is Literal literal && (bool)literal.value == true)
                return true;

            if (Unit.condition.GetPesudoSource() is ValueInput valueInput &&
                valueInput.hasDefaultValue && !valueInput.hasValidConnection &&
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