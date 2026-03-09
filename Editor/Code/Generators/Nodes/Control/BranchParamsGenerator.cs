using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
#if VISUAL_SCRIPTING_1_7
using SUnit = Unity.VisualScripting.SubgraphUnit;
#else
using SUnit = Unity.VisualScripting.SuperUnit;
#endif
namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(BranchParams))]
    public class BranchParamsGenerator : NodeGenerator<BranchParams>
    {
        public BranchParamsGenerator(Unit unit) : base(unit) { }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input != Unit.enter)
                return;

            writer.WriteIndented("if ".ControlHighlight());
            writer.Parentheses(w =>
            {
                GenerateArguments(w, data);
            }).NewLine();

            writer.WriteLine("{");

            using (writer.IndentedScope(data))
            {
                GenerateChildControl(Unit.exitTrue, data, writer);
            }

            writer.WriteLine("}");

            if (Unit.exitFalse.hasAnyConnection)
            {
                writer.WriteIndented("else".ControlHighlight());

                var ifTrueDestination = Unit.exitTrue.GetPesudoDestination();
                if (!Unit.exitTrue.hasValidConnection || (ifTrueDestination.unit is SUnit && !ifTrueDestination.hasValidConnection))
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
                    GenerateChildControl(Unit.exitFalse, data, writer);
                }

                writer.WriteLine("}");
            }

            if (Unit.exitNext != null && Unit.exitNext.hasAnyConnection)
            {
                GenerateExitControl(Unit.exitNext, data, writer);
            }
        }

        private void GenerateArguments(CodeWriter writer, ControlGenerationData data)
        {
            string op;

            switch (Unit.BranchingType)
            {
                case LogicParamNode.BranchType.And:
                    op = " && ";
                    using (data.Expect(typeof(bool)))
                    {
                        WriteJoined(writer, data, op);
                    }
                    break;

                case LogicParamNode.BranchType.Or:
                    op = " || ";
                    using (data.Expect(typeof(bool)))
                    {
                        WriteJoined(writer, data, op);
                    }
                    break;

                case LogicParamNode.BranchType.GreaterThan:
                    op = Unit.AllowEquals ? " >= " : " > ";
                    using (data.Expect(typeof(float)))
                    {
                        WriteJoined(writer, data, op);
                    }
                    break;

                case LogicParamNode.BranchType.LessThan:
                    op = Unit.AllowEquals ? " <= " : " < ";
                    using (data.Expect(typeof(float)))
                    {
                        WriteJoined(writer, data, op);
                    }
                    break;

                case LogicParamNode.BranchType.Equal:
                    op = " == ";
                    if (Unit.Numeric)
                    {
                        using (data.Expect(typeof(float)))
                        {
                            WriteJoined(writer, data, op);
                        }
                    }
                    else
                    {
                        WriteJoined(writer, data, op);
                    }
                    break;
            }
        }

        private void WriteJoined(CodeWriter writer, ControlGenerationData data, string op)
        {
            for (int i = 0; i < Unit.arguments.Count; i++)
            {
                if (i != 0)
                    writer.Write(op);

                GenerateValue(Unit.arguments[i], data, writer);
            }
        }
    }
}