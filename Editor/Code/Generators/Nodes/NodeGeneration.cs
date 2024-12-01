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
            var comments = "";

            if (node.graph.units.Any(unit => unit is CommentNode commentNode))
            {
                var commentNodes = node.graph.units.Where(unit => unit is CommentNode).Cast<CommentNode>();
                foreach (var unit in commentNodes)
                {
                    if (unit.connectedUnits.Contains(node))
                    {
                        comments += CodeUtility.MakeSelectable(unit, $"/* {unit.comment} */ ".CommentHighlight());
                    }
                }
            }
            var generator = NodeGenerator<T>.GetSingleDecorator(node, node);
            return comments + generator.GenerateValue(input, data);
        }

        public static string GenerateValue<T>(this T node, ValueOutput output, ControlGenerationData data = null) where T : Unit
        {
            var comments = "";

            if (node.graph.units.Any(unit => unit is CommentNode commentNode))
            {
                var commentNodes = node.graph.units.Where(unit => unit is CommentNode).Cast<CommentNode>();
                foreach (var unit in commentNodes)
                {
                    if (unit.connectedUnits.Contains(node))
                    {
                        comments += CodeUtility.MakeSelectable(unit, $"/* {unit.comment} */ ".CommentHighlight());
                    }
                }
            }
            var generator = NodeGenerator<T>.GetSingleDecorator(node, node);
            return comments + generator.GenerateValue(output, data);
        }

        public static string GenerateControl<T>(this T node, ControlInput input, ControlGenerationData data, int indent) where T : Unit
        {
            var comments = "";

            if (node.graph.units.Any(unit => unit is CommentNode commentNode))
            {
                var commentNodes = node.graph.units.Where(unit => unit is CommentNode).Cast<CommentNode>();
                foreach (var unit in commentNodes)
                {
                    if (unit.connectedUnits.Contains(node))
                    {
                        comments += CodeBuilder.Indent(indent) + CodeUtility.MakeSelectable(unit, $"// {unit.comment}".CommentHighlight()) + "\n";
                    }
                }
            }
            var generator = NodeGenerator<T>.GetSingleDecorator(node, node);
            return comments + generator.GenerateControl(input, data, indent);
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
