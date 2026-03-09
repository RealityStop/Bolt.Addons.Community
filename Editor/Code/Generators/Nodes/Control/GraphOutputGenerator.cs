using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
#if VISUAL_SCRIPTING_1_7
using SUnit = Unity.VisualScripting.SubgraphUnit;
#else
using SUnit = Unity.VisualScripting.SuperUnit;
#endif


namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(GraphOutput))]
    public sealed class GraphOutputGenerator : NodeGenerator<GraphOutput>
    {
        public GraphOutputGenerator(GraphOutput unit) : base(unit)
        {
        }

        internal List<ControlOutput> connectedControlOutputs = new List<ControlOutput>();
        public SUnit parent;

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            var controloutput = connectedControlOutputs.FirstOrDefault(output => output.key.Equals(input.key, System.StringComparison.OrdinalIgnoreCase));

            if (data.TryGetGraphPointer(out var graphPointer) && graphPointer.isChild)
            {
                data.ParentReference(false);
            }

            if (controloutput != null)
            {
                GenerateExitControl(controloutput, data, writer);
            }
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            GenerateValue(Unit.valueInputs[output.key], data, writer);
        }

        public void AddConnectedControlOutput(ControlOutput output)
        {
            if (!connectedControlOutputs.Contains(output))
            {
                connectedControlOutputs.Add(output);
            }
        }

        public void ClearConnectedGraphOutputs()
        {
            connectedControlOutputs.Clear();
        }
    }
}