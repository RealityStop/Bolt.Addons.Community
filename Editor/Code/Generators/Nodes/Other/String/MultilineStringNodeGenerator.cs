using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(MultilineStringNode))]
    public class MultilineStringNodeGenerator : NodeGenerator<MultilineStringNode>
    {
        public MultilineStringNodeGenerator(Unit unit) : base(unit) { }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (data.GetExpectedType() == typeof(string))
            {
                data.MarkExpectedTypeMet(typeof(string));
            }
            writer.Write(Unit.stringLiteral.As().Code(true, true, true, "", true, true));
        }
    }
}