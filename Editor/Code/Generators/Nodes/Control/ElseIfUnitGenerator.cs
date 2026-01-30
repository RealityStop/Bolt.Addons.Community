using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(ElseIfUnit))]
    public sealed class ElseIfUnitGenerator : NodeGenerator<ElseIfUnit>
    {
        public ElseIfUnitGenerator(ElseIfUnit unit) : base(unit) { }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input != Unit.Enter)
                return;

            bool foundTrue = IsTrueLiteral(Unit.condition);

            writer.WriteIndented("if ".ControlHighlight());
            writer.Parentheses(w => GenerateValue(Unit.condition, data, w)).NewLine();
            writer.WriteLine("{");

            using (writer.IndentedScope(data))
            {
                if (IsFalseLiteral(Unit.condition))
                {
                    writer.WriteErrorDiagnostic(
                        "The code in the 'If' branch is unreachable because the condition is always false.",
                        $"Unreachable Code in 'If' Branch: {Unit.If.key}",
                        WriteOptions.IndentedNewLineAfter);
                }

                GenerateChildControl(Unit.If, data, writer);
            }

            writer.WriteLine("}");

            for (int i = 0; i < Unit.amount; i++)
            {
                var conditionInput = Unit.elseIfConditions[i];
                var controlOutput = Unit.elseIfs[i];

                if (!controlOutput.hasValidConnection)
                    continue;

                bool isSelfFalse = IsFalseLiteral(conditionInput);
                bool isSelfTrue = IsTrueLiteral(conditionInput);
                bool shortCircuited = foundTrue;
                bool isUnreachable = isSelfFalse || shortCircuited;

                if (isSelfTrue && !foundTrue)
                    foundTrue = true;

                writer.WriteIndented("else if ".ControlHighlight());
                writer.Parentheses(w => GenerateValue(conditionInput, data, w)).NewLine();
                writer.WriteLine("{");

                using (writer.IndentedScope(data))
                {
                    if (isUnreachable)
                    {
                        string reason;

                        if (shortCircuited && isSelfFalse)
                            reason = "This branch is unreachable because a prior condition is always true and this condition is always false.";
                        else if (shortCircuited)
                            reason = "This branch is unreachable because a prior condition is always true.";
                        else
                            reason = "This branch is unreachable because its condition is always false.";

                        writer.WriteErrorDiagnostic(
                            reason,
                            $"Unreachable Code in 'Else If' Branch: {controlOutput.key}",
                            WriteOptions.IndentedNewLineAfter);
                    }

                    GenerateChildControl(controlOutput, data, writer);
                }

                writer.WriteLine("}");
            }

            if (Unit.showElse && Unit.Else.hasValidConnection)
            {
                writer.WriteLine("else".ControlHighlight());
                writer.WriteLine("{");

                using (writer.IndentedScope(data))
                {
                    if (foundTrue)
                    {
                        writer.WriteErrorDiagnostic(
                            "This branch is unreachable because a prior condition is always true.",
                            "Unreachable Code in 'Else'",
                            WriteOptions.IndentedNewLineAfter);
                    }

                    GenerateChildControl(Unit.Else, data, writer);
                }

                writer.WriteLine("}");
            }
        }

        private bool IsTrueLiteral(ValueInput input)
        {
            if (input.GetPesudoSource().unit is Literal literal && literal.value is bool b && b)
                return true;

            if (input.GetPesudoSource() is ValueInput valueInput &&
                valueInput.hasDefaultValue &&
                !valueInput.hasValidConnection &&
                valueInput.unit.defaultValues[valueInput.key] is bool condition &&
                condition)
                return true;

            return false;
        }

        private bool IsFalseLiteral(ValueInput input)
        {
            if (input.GetPesudoSource().unit is Literal literal && literal.value is bool b && !b)
                return true;

            if (input.GetPesudoSource() is ValueInput valueInput &&
                valueInput.hasDefaultValue &&
                !valueInput.hasValidConnection &&
                valueInput.unit.defaultValues[valueInput.key] is bool condition &&
                !condition)
                return true;

            return false;
        }
    }
}