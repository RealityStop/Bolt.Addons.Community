using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(GraphInput))]
    public sealed class GraphInputGenerator : NodeGenerator<GraphInput>
    {
        public GraphInputGenerator(GraphInput unit) : base(unit)
        {
        }

        internal List<ValueInput> connectedValueInputs = new List<ValueInput>();
        public SubgraphUnit parent;

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            var matching = unit.controlOutputs.FirstOrDefault(o => o.key.Equals(input.key, StringComparison.OrdinalIgnoreCase));

            if (matching != null)
                GenerateExitControl(matching, data, writer);
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            var _output = string.Empty;
            var ValueInput = connectedValueInputs.FirstOrDefault(valueInput => valueInput.key == output.key);

            if (ValueInput != null)
            {
                GenerateValue(ValueInput, data, writer);
            }
            else
            {
                var defaultValue = output.type.PseudoDefault();
                if (defaultValue == null)
                {
                    writer.Error($"Missing Value Input: {output.key}");
                }
                else
                {
                    writer.Object(defaultValue, true, true, true, true, "", false, true);
                }
            }
        }

        public void AddConnectedValueInput(ValueInput input)
        {
            if (!connectedValueInputs.Contains(input))
            {
                connectedValueInputs.Add(input);
            }
        }

        public void ClearConnectedValueInputs()
        {
            connectedValueInputs.Clear();
        }
    }
}