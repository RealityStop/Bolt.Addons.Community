using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community
{
    public static class NodeGeneration
    {
        public static string GenerateValue<T>(this T node, ValueInput input, ControlGenerationData data = null) where T : Unit
        {
            var generator = NodeGenerator<T>.GetSingleDecorator(node, node);
            return generator.GenerateValue(input, data);
        }

        public static string GenerateValue<T>(this T node, ValueOutput output, ControlGenerationData data = null) where T : Unit
        {
            var generator = NodeGenerator<T>.GetSingleDecorator(node, node);
            return generator.GenerateValue(output, data);
        }

        public static string GenerateControl<T>(this T node, ControlInput input, ControlGenerationData data, int indent) where T : Unit
        {
            var generator = NodeGenerator<T>.GetSingleDecorator(node, node);
            return generator.GenerateControl(input, data, indent);
        }

        public static NodeGenerator GetGenerator(this Unit node)
        {
            var generator = NodeGenerator.GetSingleDecorator(node, node);
            return generator;
        }

        public static MethodNodeGenerator GetMethodGenerator<T>(this T node) where T : Unit
        {
            var generator = MethodNodeGenerator.GetSingleDecorator(node, node);
            return (MethodNodeGenerator)generator;
        }

        public static VariableNodeGenerator GetVariableGenerator<T>(this T node) where T : Unit
        {
            var generator = VariableNodeGenerator.GetSingleDecorator(node, node);
            return (VariableNodeGenerator)generator;
        }

        public static LocalVariableGenerator GetLocalVariableGenerator<T>(this T node) where T : Unit
        {
            var generator = LocalVariableGenerator.GetSingleDecorator(node, node);
            return (LocalVariableGenerator)generator;
        }

        public static IUnitValuePort GetPsudoSource(this ValueInput input)
        {
            if (!input.hasValidConnection)
                return null;

            var source = input.connection.source;

            if (source.unit is GraphInput graphInput)
            {
                var generator = NodeGenerator.GetSingleDecorator(graphInput, graphInput);
                foreach (var valueInput in generator.connectedValueInputs)
                {
                    if (valueInput.key == source.key)
                    {
                        if (valueInput.hasValidConnection)
                            return valueInput.connection.source;
                        if (valueInput.hasDefaultValue)
                            return valueInput;
                    }
                }
            }
            else if (source.unit is SubgraphUnit subgraph)
            {
                if (subgraph.nest?.graph.units.FirstOrDefault(unit => unit is GraphOutput) is GraphOutput graphOutput)
                {
                    foreach (var valueInput in graphOutput.valueInputs)
                    {
                        if (valueInput.key == source.key && valueInput.hasValidConnection)
                        {
                            return valueInput.connection.source;
                        }
                    }
                }
            }
            else
            {
                return source;
            }

            return null;
        }
    }
}
