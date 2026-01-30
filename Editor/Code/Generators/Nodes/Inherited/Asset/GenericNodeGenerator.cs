using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(GenericNode))]
    public class GenericNodeGenerator : NodeGenerator<GenericNode>
    {
        public GenericNodeGenerator(Unit unit) : base(unit)
        {
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            var parameter = Unit.Method.genericParameters.FirstOrDefault(g => g == Unit.genericParameter);
            writer.Write("typeof".ConstructHighlight());
            writer.Write("(");
            if(parameter != null)
            {
                writer.Write(parameter.name.TypeHighlight());
            }
            else
            {
                writer.Write(Unit.genericParameter.name.TypeHighlight());
            }
            writer.Write(")");
        }
    } 
}