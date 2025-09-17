using System.Text;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(ElseIfUnit))]
    public sealed class ElseIfUnitGenerator : NodeGenerator<ElseIfUnit>
    {
        public ElseIfUnitGenerator(ElseIfUnit unit) : base(unit) { }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = new StringBuilder();
            string cachedIndent = CodeBuilder.Indent(indent);

            if (input == Unit.Enter)
            {
                bool foundTrue = IsTrueLiteral(Unit.condition);
                output.Append(cachedIndent)
                      .Append(MakeClickableForThisUnit("if".ControlHighlight() + " ("))
                      .Append(GenerateValue(Unit.condition, data))
                      .Append(MakeClickableForThisUnit(")"))
                      .AppendLine()
                      .Append(cachedIndent)
                      .AppendLine(MakeClickableForThisUnit("{"));

                data.NewScope();
                if (IsFalseLiteral(Unit.condition))
                {
                    output.AppendLine(CodeBuilder.Indent(indent + 1) +
                        MakeClickableForThisUnit(CodeUtility.ToolTip(
                            $"The code in the 'If' branch is unreachable because the condition is always false.",
                            $"Unreachable Code in 'If' Branch: {Unit.If.key}", "")));
                }
                output.Append(GetNextUnit(Unit.If, data, indent + 1));
                data.ExitScope();

                output.AppendLine(cachedIndent + MakeClickableForThisUnit("}"));

                for (int i = 0; i < Unit.amount; i++)
                {
                    var conditionInput = Unit.elseIfConditions[i];
                    var controlOutput = Unit.elseIfs[i];

                    if (!controlOutput.hasValidConnection) continue;

                    bool isSelfFalse = IsFalseLiteral(conditionInput);
                    bool isSelfTrue = IsTrueLiteral(conditionInput);
                    bool shortCircuited = foundTrue;
                    bool isUnreachable = isSelfFalse || shortCircuited;

                    if (isSelfTrue && !foundTrue)
                    {
                        foundTrue = true;
                    }

                    output.Append(cachedIndent).Append(MakeClickableForThisUnit("else if".ControlHighlight() + " (")).Append(GenerateValue(conditionInput, data)).Append(MakeClickableForThisUnit(")")).AppendLine().Append(cachedIndent).AppendLine(MakeClickableForThisUnit("{"));

                    data.NewScope();
                    if (isUnreachable)
                    {
                        string reason;
                        if (shortCircuited && isSelfFalse)
                        {
                            reason = "This branch is unreachable because a prior condition is always true and this condition is always false.";
                        }
                        else if (shortCircuited)
                        {
                            reason = "This branch is unreachable because a prior condition is always true.";
                        }
                        else
                        {
                            reason = "This branch is unreachable because its condition is always false.";
                        }

                        output.AppendLine(CodeBuilder.Indent(indent + 1) +
                            MakeClickableForThisUnit(CodeUtility.ToolTip(reason, $"Unreachable Code in 'Else If' Branch: {controlOutput.key}", "")));
                    }

                    output.Append(GetNextUnit(controlOutput, data, indent + 1));
                    data.ExitScope();

                    output.AppendLine(cachedIndent + MakeClickableForThisUnit("}"));
                }

                if (Unit.Else.hasValidConnection)
                {
                    output.Append(cachedIndent)
                          .AppendLine(MakeClickableForThisUnit("else".ControlHighlight()))
                          .Append(cachedIndent)
                          .AppendLine(MakeClickableForThisUnit("{"));

                    data.NewScope();
                    if (foundTrue)
                        output.AppendLine(CodeBuilder.Indent(indent + 1) + MakeClickableForThisUnit(CodeUtility.ToolTip("This branch is unreachable because a prior condition is always true.", $"Unreachable Code in 'Else'", "")));
                    output.Append(GetNextUnit(Unit.Else, data, indent + 1));
                    data.ExitScope();

                    output.AppendLine(cachedIndent + MakeClickableForThisUnit("}"));
                }
            }

            return output.ToString();
        }

        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            data.SetExpectedType(typeof(bool));
            var result = base.GenerateValue(input, data);
            data.RemoveExpectedType();
            return result;
        }

        private bool IsTrueLiteral(ValueInput input)
        {
            if (input.GetPesudoSource().unit is Literal literal && literal.value is bool b && b)
                return true;

            if (input.GetPesudoSource() is ValueInput valueInput &&
                valueInput.hasDefaultValue && !valueInput.hasValidConnection &&
                valueInput.unit.defaultValues[valueInput.key] is bool condition && condition)
            {
                return true;
            }

            return false;
        }

        private bool IsFalseLiteral(ValueInput input)
        {
            if (input.GetPesudoSource().unit is Literal literal && literal.value is bool b && !b)
                return true;

            if (input.GetPesudoSource() is ValueInput valueInput &&
                valueInput.hasDefaultValue && !valueInput.hasValidConnection &&
                valueInput.unit.defaultValues[valueInput.key] is bool condition && !condition)
            {
                return true;
            }

            return false;
        }
    }
}