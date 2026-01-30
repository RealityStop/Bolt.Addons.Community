using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(ChanceFlow))]
    public class ChanceFlowGenerator : NodeGenerator<ChanceFlow>
    {
        public ChanceFlowGenerator(Unit unit) : base(unit) { }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (Unit.trueOutput.hasValidConnection)
            {
                writer.WriteIndented("if ".ControlHighlight());
                writer.Parentheses(w =>
                {
                    w.InvokeMember(
                        typeof(CSharpUtility),
                        "Chance",
                        w.Action(() => GenerateValue(Unit.value, data, w)));
                }).NewLine();

                writer.WriteLine("{");

                using (writer.IndentedScope(data))
                {
                    GenerateChildControl(Unit.trueOutput, data, writer);
                }

                writer.WriteLine("}");

                if (!Unit.falseOutput.hasValidConnection)
                    return;

                writer.WriteLine("else".ControlHighlight());
                writer.WriteLine("{");

                using (writer.IndentedScope(data))
                {
                    GenerateChildControl(Unit.falseOutput, data, writer);
                }

                writer.WriteLine("}");
            }
            else if (Unit.falseOutput.hasValidConnection)
            {
                writer.WriteIndented("if ".ControlHighlight());
                writer.Parentheses(w =>
                {
                    w.Write("!");
                    w.InvokeMember(
                        typeof(CSharpUtility),
                        "Chance",
                        w.Action(() => GenerateValue(Unit.value, data, w)));
                }).NewLine();

                writer.WriteLine("{");

                using (writer.IndentedScope(data))
                {
                    GenerateChildControl(Unit.falseOutput, data, writer);
                }

                writer.WriteLine("}");
            }
        }
    }
}
