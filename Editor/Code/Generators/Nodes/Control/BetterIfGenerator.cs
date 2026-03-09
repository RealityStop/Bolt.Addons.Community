using Unity.VisualScripting.Community.Libraries.CSharp;
#if VISUAL_SCRIPTING_1_7
using SUnit = Unity.VisualScripting.SubgraphUnit;
#else
using SUnit = Unity.VisualScripting.SuperUnit;
#endif

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(BetterIf))]
    public sealed class BetterIfGenerator : NodeGenerator<BetterIf>
    {
        public BetterIfGenerator(BetterIf unit) : base(unit) { }

        private bool TrueIsUnreachable()
        {
            if (!Unit.Condition.hasValidConnection) return false;

            if (Unit.Condition.GetPesudoSource().unit is Literal literal && (bool)literal.value == false)
                return true;

            if (Unit.Condition.GetPesudoSource() is ValueInput valueInput &&
                valueInput.hasDefaultValue && !valueInput.hasValidConnection &&
                valueInput.unit.defaultValues[valueInput.key] is bool condition &&
                condition == false)
            {
                return true;
            }

            return false;
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input != Unit.In)
                return;

            writer.WriteIndented("if ".ControlHighlight() + "(");
            GenerateValue(Unit.Condition, data, writer);

            var condition = writer.LastGeneratedCode;

            writer.Write(")").NewLine();
            writer.WriteLine("{");

            using (writer.IndentedScope(data))
            {
                if (!TrueIsUnreachable())
                {
                    GenerateChildControl(Unit.True, data, writer);
                }
                else
                {
                    using (writer.CodeDiagnosticScope($"The code in the 'True' branch is unreachable due to the output of the condition value: ({condition}).", CodeDiagnosticKind.Error))
                    {
                        writer.WriteDiagnosticComment("Unreachable Code in 'True' Branch", CodeDiagnosticKind.Error, WriteOptions.Indented | WriteOptions.NewLineAfter);
                        GenerateChildControl(Unit.True, data, writer);
                    }
                }
            }

            writer.WriteLine("}");

            if (Unit.False.hasValidConnection)
            {
                writer.WriteIndented("else".ControlHighlight());

                var ifTrueDestination = Unit.True.GetPesudoDestination();
                if (Unit.Condition.hasValidConnection && (!Unit.True.hasValidConnection || (ifTrueDestination.unit is SUnit && !ifTrueDestination.hasValidConnection)))
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
                        GenerateChildControl(Unit.False, data, writer);
                    }
                    else
                    {
                        using (writer.CodeDiagnosticScope($"The code in the 'False' branch is unreachable due to the output of the condition value: ({condition}).", CodeDiagnosticKind.Error))
                        {
                            writer.WriteDiagnosticComment("Unreachable Code in 'False' Branch", CodeDiagnosticKind.Error, WriteOptions.Indented | WriteOptions.NewLineAfter);
                            GenerateChildControl(Unit.False, data, writer);
                        }
                    }
                }

                writer.WriteLine("}");
            }

            GenerateExitControl(Unit.Finished, data, writer);
        }


        private bool FalseIsUnreachable()
        {
            if (!Unit.Condition.hasValidConnection) return false;

            if (Unit.Condition.GetPesudoSource().unit is Literal literal && (bool)literal.value == true)
                return true;

            if (Unit.Condition.GetPesudoSource() is ValueInput valueInput &&
                valueInput.hasDefaultValue && !valueInput.hasValidConnection &&
                valueInput.unit.defaultValues[valueInput.key] is bool condition &&
                condition == true)
            {
                return true;
            }

            return false;
        }

        protected override void GenerateValueInternal(ValueInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input == Unit.Condition && Unit.Condition.hasValidConnection)
            {
                GenerateConnectedValueCasted(input, data, writer, input.type, () => ShouldCast(input, data, writer));
                return;
            }
            base.GenerateValueInternal(input, data, writer);
        }
    }
}
