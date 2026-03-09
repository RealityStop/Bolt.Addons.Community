using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Or))]
    public sealed class OrGenerator : NodeGenerator<Or>
    {
        public OrGenerator(Or unit) : base(unit)
        {
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (output == Unit.result)
            {
                writer.Write("(");
                GenerateValue(Unit.a, data, writer);
                writer.Write(" || ");
                GenerateValue(Unit.b, data, writer);
                writer.Write(")");
            }
        }
    }
}