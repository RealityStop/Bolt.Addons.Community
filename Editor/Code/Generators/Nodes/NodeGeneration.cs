using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community
{
    public static class NodeGeneration
    {
        public static string GenerateValue<T>(this T node, ValueInput input, ControlGenerationData data = null) where T : Unit
        {
            return NodeGenerator<T>.GetSingleDecorator(node, node).GenerateValue(input, data);
        }

        public static string GenerateValue<T>(this T node, ValueOutput output, ControlGenerationData data = null) where T : Unit
        {
            return NodeGenerator<T>.GetSingleDecorator(node, node).GenerateValue(output, data);
        }

        public static string GenerateControl<T>(this T node, ControlInput input, ControlGenerationData data, int indent) where T : Unit
        {
            var generator = NodeGenerator<T>.GetSingleDecorator(node, node);
            //  if (!generator.recursion?.TryEnter(generator.unit) ?? false)
            //      return CodeBuilder.Indent(indent) + $"/* Recursion reached max depth (10) most likely from attempting to generate a infinite loop please fix this on : {(node is SubgraphUnit subgraph ? subgraph.nest.graph.title : node is null ? "null" : node.GetType())} */";
            var code = generator.GenerateControl(input, data, indent);
            // generator.recursion.Exit(generator.unit);
            return code;
        }
    }
}