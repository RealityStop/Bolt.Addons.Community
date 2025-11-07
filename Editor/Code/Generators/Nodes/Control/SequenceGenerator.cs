using System.Text;
namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Sequence))]
    public sealed class SequenceGenerator : NodeGenerator<Sequence>
    {
        public SequenceGenerator(Unit unit) : base(unit) { }
    
        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var outputBuilder = new StringBuilder();

            var outputs = Unit.multiOutputs;
    
            foreach (var controlOutput in outputs)
            {
                if (controlOutput.hasValidConnection)
                {
                    outputBuilder.Append(GetNextUnit(controlOutput, data, indent));
                }
            }
    
            return outputBuilder.ToString();
        }
    } 
}