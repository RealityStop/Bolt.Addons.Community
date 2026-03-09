using System.Text;
using Unity.VisualScripting.Community.Libraries.CSharp;
#if VISUAL_SCRIPTING_1_7
using SUnit = Unity.VisualScripting.SubgraphUnit;
#else
using SUnit = Unity.VisualScripting.SuperUnit;
#endif

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(If))]
    public sealed class IfGenerator : NodeGenerator<If>
    {
        public IfGenerator(If unit) : base(unit)
        {
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input != Unit.enter)
                return;

            writer.WriteIndented("if ".ControlHighlight() + "(");
            GenerateValue(Unit.condition, data, writer);

            var condition = writer.LastGeneratedCode;

            writer.Write(")").NewLine();
            writer.WriteLine("{");

            using (writer.IndentedScope(data))
            {
                if (!TrueIsUnreachable())
                {
                    GenerateChildControl(Unit.ifTrue, data, writer);
                }
                else
                {
                    using (writer.CodeDiagnosticScope($"The code in the 'True' branch is unreachable due to the output of the condition value: ({condition}).", CodeDiagnosticKind.Error))
                    {
                        writer.WriteDiagnosticComment("Unreachable Code in 'True' Branch", CodeDiagnosticKind.Error, WriteOptions.Indented | WriteOptions.NewLineAfter);
                        GenerateChildControl(Unit.ifTrue, data, writer);
                    }
                }
            }

            writer.WriteLine("}");

            if (!Unit.ifFalse.hasValidConnection)
                return;

            writer.WriteIndented("else".ControlHighlight());

            var ifTrueDestination = Unit.ifTrue.GetPesudoDestination();
            if (Unit.condition.hasValidConnection && (!Unit.ifTrue.hasValidConnection || (ifTrueDestination.unit is SUnit && !ifTrueDestination.hasValidConnection)))
            {
                writer.WriteRecommendationDiagnostic("You can simplify this by negating the condition and using the True output, which improves the generated code.", "Condition can be negated", WriteOptions.NewLineAfter);
            }
            else
            {
                writer.NewLine();
            }

            writer.WriteLine("{");

            using (writer.IndentedScope(data))
            {
                if (!FalseIsUnreachable())
                {
                    GenerateChildControl(Unit.ifFalse, data, writer);
                }
                else
                {
                    using (writer.CodeDiagnosticScope($"The code in the 'False' branch is unreachable due to the output of the condition value: ({condition}).", CodeDiagnosticKind.Error))
                    {
                        writer.WriteDiagnosticComment("Unreachable Code in 'False' Branch", CodeDiagnosticKind.Error, WriteOptions.Indented | WriteOptions.NewLineAfter);
                        GenerateChildControl(Unit.ifFalse, data, writer);
                    }
                }
            }

            writer.WriteLine("}");
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
    }
}