using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(GreaterOrEqual))]
    public sealed class GreaterOrEqualGenerator : NodeGenerator<GreaterOrEqual>
    {
        public GreaterOrEqualGenerator(GreaterOrEqual unit) : base(unit)
        {
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (output == Unit.comparison)
            {
                GenerateValue(Unit.a, data, writer);
                writer.Write(" >= ");
                GenerateValue(Unit.b, data, writer);
            }
        }
    }
}