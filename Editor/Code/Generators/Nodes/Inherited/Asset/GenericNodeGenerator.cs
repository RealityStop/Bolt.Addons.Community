using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community 
{
    [NodeGenerator(typeof(GenericNode))]
    public class GenericNodeGenerator : NodeGenerator<GenericNode>
    {
        public GenericNodeGenerator(Unit unit) : base(unit)
        {
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            var parameter = Unit.Method.genericParameters.FirstOrDefault(g => g == Unit.genericParameter);
            if(parameter != null)
            {
                MakeClickableForThisUnit("typeof".ConstructHighlight() + "(" + parameter.name.TypeHighlight() + ")");
            }
            return MakeClickableForThisUnit("typeof".ConstructHighlight() + "(" + Unit.genericParameter.name.TypeHighlight() + ")");
        }
    } 
}