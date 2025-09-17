using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community 
{
    [NodeGenerator(typeof(GraphInput))]
    public sealed class GraphInputGenerator : NodeGenerator<GraphInput>
    {
        public GraphInputGenerator(GraphInput unit) : base(unit)
        {
        }
    
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            var _output = string.Empty;
            var ValueInput = connectedValueInputs.FirstOrDefault(valueInput => valueInput.key == output.key);
    
            if (ValueInput != null)
            {
                _output += GenerateValue(ValueInput, data);
            }
            else
            {
                var defaultValue = output.type.PseudoDefault();
                if (defaultValue == null)
                    _output += MakeClickableForThisUnit($"/* Missing Value Input: {output.key} */".WarningHighlight());
                else
                {
                    _output += defaultValue.As().Code(true, unit, true, true, "", false, true);
                }
            }
    
            return _output;
        }
    } 
}