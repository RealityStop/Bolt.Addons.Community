using System.Linq;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(GraphOutput))]
    public sealed class GraphOutputGenerator : NodeGenerator<GraphOutput>
    {
        public GraphOutputGenerator(GraphOutput unit) : base(unit)
        {
        }
    
        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;
    
            var controloutput = connectedGraphOutputs.FirstOrDefault(output => output.key.Equals(input.key, System.StringComparison.OrdinalIgnoreCase));
            if (controloutput != null)
            {
                output += GetNextUnit(controloutput, data, indent);
            }
    
            return output;
        }
    
    
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return GenerateValue(Unit.valueInputs[output.key], data);
        }
    } 
}