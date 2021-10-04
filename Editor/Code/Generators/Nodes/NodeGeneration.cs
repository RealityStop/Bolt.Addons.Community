using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    public static class NodeGeneration
    {
        public static string GenerateValue<T>(this T node, ValueInput input) where T : Unit
        {
            return NodeGenerator<T>.GetSingleDecorator(node, node).GenerateValue(input);
        }

        public static string GenerateValue<T>(this T node, ValueOutput output) where T : Unit
        {
            return NodeGenerator<T>.GetSingleDecorator(node, node).GenerateValue(output);
        }

        public static string GenerateControl<T>(this T node, ControlInput input, ControlGenerationData data, int indent) where T : Unit
        {
            return NodeGenerator<T>.GetSingleDecorator(node, node).GenerateControl(input, data, indent);
        }
    }
}