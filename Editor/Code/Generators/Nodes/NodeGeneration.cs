using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;
#if VISUAL_SCRIPTING_1_7
using SUnit = Unity.VisualScripting.SubgraphUnit;
#else
using SUnit = Unity.VisualScripting.SuperUnit;
#endif
namespace Unity.VisualScripting.Community.CSharp
{
    public static class NodeGeneration
    {
        public static void GenerateValue<T>(this T node, ValueInput input, CodeWriter writer, ControlGenerationData data) where T : Unit
        {
            var generator = GetGenerator(node);

            generator.GenerateValue(input, data, writer);
        }

        public static void GenerateValue<T>(this T node, ValueOutput output, CodeWriter writer, ControlGenerationData data) where T : Unit
        {
            var generator = GetGenerator(node);

            generator.GenerateValue(output, data, writer);
        }

        public static void GenerateControl<T>(this T node, ControlInput input, ControlGenerationData data, CodeWriter writer) where T : Unit
        {
            var generator = GetGenerator(node);

            generator.GenerateControl(input, data, writer);
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

        private static string GetComponentCode(Type type, CodeWriter writer, bool includeDot, bool includeParentheses)
        {
            if (type == typeof(Transform))
            {
                return (includeDot ? "." : "") + "transform".VariableHighlight();
            }
            else
            {
                return (includeDot ? "." : "") + $"GetComponent<{writer.GetTypeNameHighlighted(type)}>" + (includeParentheses ? "()" : "");
            }
        }

        public static string GetComponent(this ValueInput input, CodeWriter writer, Type sourceType, Type requiredType, bool includeDot, bool includeParentheses)
        {
            if (requiredType == typeof(GameObject) || !typeof(Component).IsStrictlyAssignableFrom(requiredType))
            {
                return string.Empty;
            }

            if (!input.hasValidConnection && sourceType == typeof(GameObject))
            {
                // If required is a Component (or subclass), we need GetComponent on the implicit gameObject.
                return GetComponentCode(requiredType, writer, includeDot, includeParentheses);
            }

            // If the source already returns something assignable to the required type -> NO GetComponent
            if (sourceType != null && requiredType.IsStrictlyAssignableFrom(sourceType))
            {
                return string.Empty;
            }

            // If the source is a GameObject -> we need GetComponent<T>()
            if (sourceType == typeof(GameObject))
            {
                return GetComponentCode(requiredType, writer, includeDot, includeParentheses);
            }

            // If the source unit is a MemberUnit that is already a GetComponent call, don't add another one.
            // This guards against chained GetComponent calls.
            if (input.connection?.source?.unit is MemberUnit srcMemberUnit)
            {
                var srcMember = srcMemberUnit.member;
                if (srcMember != null && string.Equals(srcMember.name, "GetComponent", StringComparison.Ordinal) &&
                (srcMember.declaringType == typeof(GameObject) || srcMember.declaringType.IsStrictlyAssignableFrom(typeof(Component))))
                {
                    return string.Empty;
                }
            }

            // If the source unit is a InheritedMemberUnit that is already a GetComponent call, don't add another one.
            // This guards against chained GetComponent calls.
            if (input.connection?.source?.unit is InheritedMemberUnit inheritedMember)
            {
                var srcMember = inheritedMember.member;
                if (srcMember != null && string.Equals(srcMember.name, "GetComponent", StringComparison.Ordinal) &&
                (srcMember.declaringType == typeof(GameObject) || srcMember.declaringType.IsStrictlyAssignableFrom(typeof(Component))))
                {
                    return string.Empty;
                }
            }

            // As a cautious fallback: if the required type is a Component (and we haven't proven the source satisfies it),
            // return GetComponent to be safe.
            if (typeof(Component).IsStrictlyAssignableFrom(requiredType))
            {
                return GetComponentCode(requiredType, writer, includeDot, includeParentheses);
            }

            return string.Empty;
        }

        public static IUnitValuePort GetPesudoSource(this ValueInput input)
        {
            if (!input.hasValidConnection)
                return input;

            var source = input.connection.source;

            return source.unit switch
            {
                GraphInput graphInput => FindConnectedInput(GetGenerator(graphInput) as GraphInputGenerator, source.key) ?? source,
                SUnit subgraph => FindConnectedSubgraphOutput(subgraph, source.key) ?? source,
                _ => source
            };
        }

        public static IUnitControlPort GetPesudoDestination(this ControlOutput output)
        {
            if (!output.hasValidConnection)
                return output;

            var destination = output.connection.destination;

            return destination.unit switch
            {
                GraphOutput graphOutput => FindConnectedOutput(GetGenerator(graphOutput) as GraphOutputGenerator, destination.key) ?? destination,
                SUnit subgraph => FindConnectedSubgraphInput(subgraph, destination.key) ?? destination,
                _ => destination
            };
        }

        private static IUnitControlPort FindConnectedOutput(GraphOutputGenerator generator, string key)
        {
            foreach (var output in generator.parent.controlOutputs)
            {
                if (output.key == key)
                {
                    if (output.hasValidConnection)
                        return output.connection.destination;

                    return output;
                }
            }
            return null;
        }

        private static IUnitControlPort FindConnectedSubgraphInput(SUnit subgraph, string key)
        {
            var graph = subgraph.nest?.graph;
            var inputCandidate = graph?.units.FirstOrDefault(u => u is GraphInput);
            GraphInput input = inputCandidate as GraphInput;

            if (input == null)
                return null;

            foreach (var controlOutput in input.controlOutputs)
            {
                if (controlOutput.key == key)
                {
                    if (controlOutput.hasValidConnection)
                        return controlOutput.connection.destination;

                    return controlOutput;
                }
            }

            return null;
        }

        private static IUnitValuePort FindConnectedInput(GraphInputGenerator generator, string key)
        {
            foreach (var valueInput in generator.parent.valueInputs)
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
                if (valueInput.key == key)
                {
                    if (valueInput.hasValidConnection)
                        return valueInput.connection.destination;
                }
            }

            return null;
        }

        public static bool IsSourceLiteral(ValueInput valueInput, out Type sourceType)
        {
            var source = GetPesudoSource(valueInput);
            if (source != null)
            {
                if (source.unit is Literal literal)
                {
                    sourceType = literal.type;
                    return true;
                }
                else if (source.unit is MultilineStringNode)
                {
                    sourceType = typeof(string);
                    return true;
                }
                else if (source is ValueInput v && !v.hasValidConnection && v.hasDefaultValue)
                {
                    sourceType = v.type;
                    return true;
                }
            }
            sourceType = null;
            return false;
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