using Unity.VisualScripting;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(SelectUnit))]
    public sealed class SelectGenerator : NodeGenerator<SelectUnit>
    {
        public SelectGenerator(Unit unit) : base(unit)
        {
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (output == Unit.selection)
            {
                writer.Write("(");
                GenerateValue(Unit.condition, data, writer);
                writer.Write(" ? ");
                GenerateValue(Unit.ifTrue, data, writer);
                writer.Write(" : ");
                GenerateValue(Unit.ifFalse, data, writer);
                writer.Write(")");
            }
        }
    }
}