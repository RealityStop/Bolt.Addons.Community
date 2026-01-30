using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(While))]
    public sealed class WhileGenerator : NodeGenerator<While>
    {
        public WhileGenerator(Unit unit) : base(unit)
        {
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            writer.WriteIndented("while".ControlHighlight()).Parentheses(w => GenerateValue(Unit.condition, data, w)).NewLine();
            writer.WriteLine("{");
            using (writer.IndentedScope(data))
            {
                GenerateChildControl(Unit.body, data, writer);
            }
            writer.WriteLine("}");
            GenerateExitControl(Unit.exit, data, writer);
        }
    }
}