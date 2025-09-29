﻿using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;
#if VISUAL_SCRIPTING_1_7
using SUnit = Unity.VisualScripting.SubgraphUnit;
#else
using SUnit = Unity.VisualScripting.SuperUnit;
#endif
namespace Unity.VisualScripting.Community
{
    public static class NodeGeneration
    {
        public static string GenerateValue<T>(this T node, ValueInput input, ControlGenerationData data = null) where T : Unit
        {
            var generator = GetGenerator(node);
            generator.UpdateRecursion();

            if (!generator.recursion?.TryEnter(node) ?? false)
            {
                return generator.MakeClickableForThisUnit(CodeUtility.ErrorTooltip($"{input.key} is infinitely generating itself. Consider reviewing your graph logic or Increasing recursion depth in preview Settings.", "Infinite recursion detected!", ""));
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

        public static string GenerateValue<T>(this T node, ValueOutput output, ControlGenerationData data = null) where T : Unit
        {
            var generator = GetGenerator(node);
            generator.UpdateRecursion();

            if (!generator.recursion?.TryEnter(node) ?? false)
            {
                return generator.MakeClickableForThisUnit(CodeUtility.ErrorTooltip($"{output.key} is infinitely generating itself. Consider reviewing your graph logic or Increasing recursion depth in preview Settings.", "Infinite recursion detected!", ""));
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

        public static string GenerateControl<T>(this T node, ControlInput input, ControlGenerationData data, int indent) where T : Unit
        {
            var generator = GetGenerator(node);
            generator.UpdateRecursion();

            if (!generator.recursion?.TryEnter(node) ?? false)
            {
                return CodeBuilder.Indent(indent) + generator.MakeClickableForThisUnit(CodeUtility.ErrorTooltip("This node appears to cause infinite recursion(The flow is leading back to this node). Consider using a While loop instead or Increasing recursion depth in preview Settings.", "Infinite recursion detected!", ""));
            }

            try
            {
                var commentNode = node.graph.units.FirstOrDefault(u => u is CommentNode commentNode && commentNode.connectedElements.Any(c => c == node));
                string comment = commentNode != null ? GetComment(commentNode as CommentNode, indent) + "\n" : "";
                return comment + generator.GenerateControl(input, data, indent);
            }
            finally
            {
                generator.recursion?.Exit(node);
            }
        }

        private static string GetComment(CommentNode comment, int indent)
        {
            var result = string.Empty;

            if (comment.hasTitle && !string.IsNullOrEmpty(comment.title))
            {
                result += CodeBuilder.Indent(indent) + CodeUtility.MakeClickable(comment, ("// " + comment.title).CommentHighlight());

                if (!string.IsNullOrEmpty(comment.comment))
                    result += CodeUtility.MakeClickable(comment, " :".CommentHighlight());
                result += "\n";
            }

            if (!string.IsNullOrEmpty(comment.comment))
            {
                var lines = comment.comment.Split(
                    new[] { '\r', '\n' },
                    StringSplitOptions.RemoveEmptyEntries
                );

                foreach (var line in lines)
                {
                    result += CodeBuilder.Indent(indent) + CodeUtility.MakeClickable(comment, ("// " + line.TrimEnd()).CommentHighlight()) + "\n";
                }
            }

            return result.TrimEnd();
        }

        private static readonly Dictionary<Unit, NodeGenerator> generatorCache = new Dictionary<Unit, NodeGenerator>();

        public static NodeGenerator GetGenerator(this Unit node)
        {
            if (!generatorCache.TryGetValue(node, out var generator))
            {
                generator = NodeGenerator.GetSingleDecorator(node, node);
                generatorCache[node] = generator;
            }

            return generator;
        }

        public static MethodNodeGenerator GetMethodGenerator<T>(this T node, bool throwError = true) where T : Unit
        {
            if (GetGenerator(node) is MethodNodeGenerator methodNodeGenerator)
                return methodNodeGenerator;
            if (throwError)
                throw new InvalidOperationException($"{node.GetType()} is not a method generator.");
            else
                return null;
        }

        public static VariableNodeGenerator GetVariableGenerator<T>(this T node, bool throwError = true) where T : Unit
        {
            if (GetGenerator(node) is VariableNodeGenerator variableNodeGenerator)
                return variableNodeGenerator;
            if (throwError)
                throw new InvalidOperationException($"{node.GetType()} is not a variable generator.");
            else
                return null;
        }

        public static LocalVariableGenerator GetLocalVariableGenerator<T>(this T node, bool throwError = true) where T : Unit
        {
            if (GetGenerator(node) is LocalVariableGenerator localVariableGenerator)
                return localVariableGenerator;
            if (throwError)
                throw new InvalidOperationException($"{node.GetType()} is not a local variable generator.");
            else
                return null;
        }

        public static IUnitValuePort GetPesudoSource(this ValueInput input)
        {
            if (!input.hasValidConnection)
                return input;

            var source = input.connection.source;

            return source.unit switch
            {
                GraphInput graphInput => FindConnectedInput(GetGenerator(graphInput), source.key),
                SUnit subgraph => FindConnectedSubgraphOutput(subgraph, source.key),
                _ => source
            };
        }

        private static IUnitValuePort FindConnectedInput(NodeGenerator generator, string key)
        {
            foreach (var valueInput in generator.connectedValueInputs)
            {
                if (valueInput.key == key)
                {
                    if (valueInput.hasValidConnection)
                        return valueInput.connection.source;

                    if (valueInput.hasDefaultValue)
                        return valueInput;
                }
            }
            return null;
        }

        private static IUnitValuePort FindConnectedSubgraphOutput(SUnit subgraph, string key)
        {
            var graph = subgraph.nest?.graph;
            var outputCandidate = graph?.units.FirstOrDefault(u => u is GraphOutput);
            GraphOutput output = outputCandidate as GraphOutput;

            if (output == null)
                return null;

            foreach (var valueInput in output.valueInputs)
            {
                if (valueInput.key == key && valueInput.hasValidConnection)
                    return valueInput.connection.source;
            }

            return null;
        }

        public static bool IsValidRefUnit(this Unit unit)
        {
            return unit is GetVariable || (unit is AssetFieldUnit fieldUnit && fieldUnit.actionDirection == ActionDirection.Get) || (unit is InheritedFieldUnit inheritedField && inheritedField.actionDirection == ActionDirection.Get);
        }
        public static bool IsValidRefUnit(this IUnit unit)
        {
            return IsValidRefUnit(unit as Unit);
        }
    }
}