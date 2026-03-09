using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(This))]
    public sealed class ThisGenerator : NodeGenerator<This>
    {
        public ThisGenerator(This unit) : base(unit)
        {
        }
    
        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.Write("gameObject".VariableHighlight());
        }
    }
}