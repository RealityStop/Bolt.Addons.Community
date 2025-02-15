using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community
{
    public static class NodeGeneration
    {
        /// <summary>
        /// Generates a value for a specified ValueInput node.
        /// Handles infinite recursion detection.
        /// </summary>
        public static string GenerateValue<T>(this T node, ValueInput input, ControlGenerationData data = null) where T : Unit
        {
            var generator = GetGenerator(node);
            generator.UpdateRecursion();

            if (!generator.recursion?.TryEnter(node) ?? false)
            {
                return generator.MakeSelectableForThisUnit(CodeUtility.ToolTip($"{input.key} is infinitely generating itself. Consider reviewing your graph logic.", "Infinite recursion detected!", ""));
            }

            try
            {
                return generator.GenerateValue(input, data);
            }
            finally
            {
                generator.recursion?.Exit(node);
            }
        }

        /// <summary>
        /// Generates a value for a specified ValueOutput node.
        /// Handles infinite recursion detection.
        /// </summary>
        public static string GenerateValue<T>(this T node, ValueOutput output, ControlGenerationData data = null) where T : Unit
        {
            var generator = GetGenerator(node);
            generator.UpdateRecursion();

            if (!generator.recursion?.TryEnter(node) ?? false)
            {
                return generator.MakeSelectableForThisUnit(CodeUtility.ToolTip($"{output.key} is infinitely generating itself. Consider reviewing your graph logic.", "Infinite recursion detected!", ""));
            }

            try
            {
                return generator.GenerateValue(output, data);
            }
            finally
            {
                generator.recursion?.Exit(node);
            }
        }

        /// <summary>
        /// Generates control flow logic for a specified ControlInput node.
        /// Handles infinite recursion detection.
        /// </summary>
        public static string GenerateControl<T>(this T node, ControlInput input, ControlGenerationData data, int indent) where T : Unit
        {
            var generator = GetGenerator(node);
            generator.UpdateRecursion();

            if (!generator.recursion?.TryEnter(node) ?? false)
            {
                return CodeBuilder.Indent(indent) + generator.MakeSelectableForThisUnit(CodeUtility.ToolTip("This node appears to cause infinite recursion(The flow is leading back to this node). Consider using a While loop instead.", "Infinite recursion detected!", ""));
            }

            try
            {
                return generator.GenerateControl(input, data, indent);
            }
            finally
            {
                generator.recursion?.Exit(node);
            }
        }

        private static readonly Dictionary<Unit, NodeGenerator> generatorCache = new();

        /// <summary>
        /// Retrieves or creates a NodeGenerator for a specified node.
        /// </summary>
        public static NodeGenerator GetGenerator(this Unit node)
        {
            if (generatorCache.ContainsKey(node))
            {
                return generatorCache[node];
            }

            generatorCache[node] = NodeGenerator.GetSingleDecorator(node, node);
            return generatorCache[node];
        }

        /// <summary>
        /// Retrieves the MethodNodeGenerator for a specified node, if applicable.
        /// </summary>
        public static MethodNodeGenerator GetMethodGenerator<T>(this T node) where T : Unit
        {
            var generator = GetGenerator(node);

            if (generator is MethodNodeGenerator methodNodeGenerator)
            {
                return methodNodeGenerator;
            }

            throw new InvalidOperationException($"{node.GetType()} is not a method generator.");
        }

        /// <summary>
        /// Retrieves the VariableNodeGenerator for a specified node, if applicable.
        /// </summary>
        public static VariableNodeGenerator GetVariableGenerator<T>(this T node) where T : Unit
        {
            var generator = GetGenerator(node);

            if (generator is VariableNodeGenerator variableNodeGenerator)
            {
                return variableNodeGenerator;
            }

            throw new InvalidOperationException($"{node.GetType()} is not a variable generator.");
        }

        /// <summary>
        /// Retrieves the LocalVariableGenerator for a specified node, if applicable.
        /// </summary>
        public static LocalVariableGenerator GetLocalVariableGenerator<T>(this T node) where T : Unit
        {
            var generator = GetGenerator(node);

            if (generator is LocalVariableGenerator localVariableGenerator)
            {
                return localVariableGenerator;
            }

            throw new InvalidOperationException($"{node.GetType()} is not a local variable generator.");
        }

        /// <summary>
        /// Gets the connected of port the GraphInput/Subgraph instead of giving the GraphInput/Subgraph port directly.
        /// </summary>
        public static IUnitValuePort GetPesudoSource(this ValueInput input)
        {
            if (!input.hasValidConnection)
                return input;

            var source = input.connection.source;

            if (source.unit is GraphInput graphInput)
            {
                var generator = GetGenerator(graphInput);

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