using System.Text;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Sequence))]
    public sealed class SequenceGenerator : NodeGenerator<Sequence>
    {
        public SequenceGenerator(Unit unit) : base(unit) { }
    
        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            var outputs = Unit.multiOutputs;
    
            foreach (var controlOutput in outputs)
            {
                if (controlOutput.hasValidConnection)
                {
                    GenerateChildControl(controlOutput, data, writer);
                }
            }
        }
    }
}