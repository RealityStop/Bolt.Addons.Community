using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public static class GraphTraversal
    {
        public static List<(GraphReference, SubgraphUnit)> GetReferencePath(GraphReference reference)
        {
            List<(GraphReference, SubgraphUnit)> nodePath = new List<(GraphReference, SubgraphUnit)>
            {
                (reference, !reference.isRoot ? reference.GetParent<SubgraphUnit>() : null)
            };
            while (reference.ParentReference(false) != null)
            {
                reference = reference.ParentReference(false);
                nodePath.Add((reference, !reference.isRoot ? reference.GetParent<SubgraphUnit>() : null));
            }
            nodePath.Reverse();
            return nodePath;
        }

        public static string GetElementPath(GraphReference reference, string pathPrefix = " -> ")
        {
            string BuildPrefix(GraphReference nodePath, GraphReference rootRef)
            {
                if (nodePath.graph == null)
                    return "::";

                // Named graph
                if (!string.IsNullOrEmpty(nodePath.graph.title))
                {
                    if (nodePath.isRoot)
                    {
                        if (rootRef.rootObject is IEventMachine eventMachine)
                        {
                            if (eventMachine.nest.source == GraphSource.Macro && eventMachine.nest.macro is Object @object)
                            {
                                if (@object.name != nodePath.graph.title) return $"<b>>></b> {nodePath.graph.title}";
                            }
                        }
                        else if (rootRef.rootObject is Object asset)
                        {
                            if (asset.name != nodePath.graph.title) return $"<b>>></b> {nodePath.graph.title}";
                        }
                        return nodePath.graph.title;
                    }
                    return nodePath.graph.title;
                }

                // Unnamed graph
                if (nodePath.isRoot)
                {
                    if (rootRef.rootObject is IEventMachine eventMachine)
                    {
                        if (eventMachine.nest.source == GraphSource.Macro && eventMachine.nest.macro is Object @object)
                        {
                            return @object.name;
                        }

                        int index;

                        if (eventMachine is ScriptMachine sm)
                        {
                            var comps = rootRef.rootObject.GetComponents<ScriptMachine>();
                            index = System.Array.IndexOf(comps, sm);
                        }
                        else if (eventMachine is StateMachine stm)
                        {
                            var comps = rootRef.rootObject.GetComponents<StateMachine>();
                            index = System.Array.IndexOf(comps, stm);
                        }
                        else
                        {
                            var comps = rootRef.rootObject.GetComponents<IEventMachine>();
                            index = System.Array.IndexOf(comps, eventMachine);
                        }


                        var typeName = eventMachine.GetType().Name;
                        return $"{typeName}{index}";
                    }
                    else if (rootRef.rootObject is Object asset)
                    {
                        return asset.name;
                    }
                }

                if (nodePath.isChild)
                {
                    return GetParentName(nodePath);
                }

                return nodePath.graph.GetType().Name;
            }

            var stack = new Stack<string>();
            var current = reference;

            while (current != null)
            {
                stack.Push(BuildPrefix(current, reference));
                current = current.ParentReference(false);
            }

            return string.Join(pathPrefix, stack);
        }

       public static string GetParentName(GraphReference reference)
        {
            if (reference.IsWithin<INesterUnit>())
            {
                var parent = reference.GetParent<INesterUnit>();
                return GetNesterUnitName(parent);
            }
            else if (reference.IsWithin<INesterState>())
            {
                var parent = reference.GetParent<INesterState>();
                return GetNesterStateName(parent);
            }
            else if (reference.IsWithin<INesterStateTransition>())
            {
                var parent = reference.GetParent<INesterStateTransition>();
                return GetNesterStateTransitionName(parent);
            }
            return "Embed " + reference.parent.GetType().Name;
        }

        public static string GetNesterUnitName(INesterUnit nester)
        {
            if (!string.IsNullOrEmpty(nester.nest.graph.title))
            {
                return nester.nest.graph.title;
            }
            else if (nester.nest.source == GraphSource.Macro && nester.nest.macro is Object @object)
            {
                return @object.name;
            }
            else if (nester is SubgraphUnit) return "Embed Subgraph";
            else if (nester is StateUnit) return "Embed StateUnit";
            else return $"Embed {nester.GetType().Name}";
        }

        public static string GetNesterStateName(INesterState nester)
        {
            if (!string.IsNullOrEmpty(nester.nest.graph.title))
            {
                return nester.nest.graph.title;
            }
            else if (nester.nest.source == GraphSource.Macro && nester.nest.macro is Object @object)
            {
                return @object.name;
            }
            else if (nester is FlowState) return "Embed FlowState";
            else if (nester is SuperState) return "Embed SuperState";
            else return $"Embed {nester.GetType().Name}";
        }

        public static string GetNesterStateTransitionName(INesterStateTransition nester)
        {
            if (!string.IsNullOrEmpty(nester.nest.graph.title))
            {
                return nester.nest.graph.title;
            }
            else if (nester.nest.source == GraphSource.Macro && nester.nest.macro is Object @object)
            {
                return @object.name;
            }
            else if (nester is FlowStateTransition) return "Embed FlowStateTransition";
            else return $"Embed {nester.GetType().Name}";
        }


        public static void TraverseFlowGraph(FlowGraph graph, System.Action<Unit> visit)
        {
            if (graph == null || visit == null) return;

            var visitedGraphs = new HashSet<FlowGraph>();
            TraverseInternal(graph, visit, visitedGraphs);
        }

        private static void TraverseInternal(FlowGraph graph, System.Action<Unit> visit, HashSet<FlowGraph> visitedGraphs)
        {
            if (!visitedGraphs.Add(graph))
                return;

            foreach (var unit in graph.units)
            {
                if (unit == null) continue;

                visit((Unit)unit);
                if (unit is SubgraphUnit subgraph)
                {
                    if (subgraph.nest.graph is FlowGraph flowGraph)
                    {
                        TraverseInternal(flowGraph, visit, visitedGraphs);
                    }
                }
            }
        }

        private static readonly Dictionary<GraphReference, (int ElementCount, Dictionary<IGraphElement, HashSet<(GraphReference, IGraphElement)>> ElementCache)> TraversalCache = new();

        public static IEnumerable<(GraphReference, T)> TraverseFlowGraph<T>(GraphReference graphReference) where T : IGraphElement
        {
            if (graphReference.graph is not FlowGraph flowGraph) yield break;

            if (!TraversalCache.TryGetValue(graphReference, out var graphCache))
            {
                graphCache = (flowGraph.elements.Count, new Dictionary<IGraphElement, HashSet<(GraphReference, IGraphElement)>>());
                TraversalCache[graphReference] = graphCache;
            }

            if (graphCache.ElementCount != flowGraph.elements.Count)
            {
                graphCache.ElementCache.Clear();
                graphCache.ElementCount = flowGraph.elements.Count;
            }

            foreach (var element in flowGraph.elements)
            {
                if (element == null || element is not T) continue;

                if (graphCache.ElementCache.TryGetValue(element, out var cachedResult))
                {
                    foreach (var cachedItem in cachedResult)
                    {
                        if (cachedItem.Item2 is T typedElement)
                            yield return (cachedItem.Item1, typedElement);
                    }
                    continue;
                }

                var unitResult = new HashSet<(GraphReference, IGraphElement)>();
                switch (element)
                {
                    case SubgraphUnit subgraphUnit:
                        {
                            var subGraph = subgraphUnit.nest.graph;
                            if (subGraph == null) continue;
                            var item = (graphReference, subgraphUnit as IGraphElement);
                            if (item.Item2 is T typedElement)
                            {
                                unitResult.Add(item);
                                yield return (item.graphReference, typedElement);
                            }

                            var childReference = graphReference.ChildReference(subgraphUnit, false);
                            foreach (var childItem in TraverseFlowGraph<T>(childReference))
                            {
                                unitResult.Add(childItem);
                                yield return childItem;
                            }
                            break;
                        }
                    case StateUnit stateUnit:
                        {
                            var stateGraph = stateUnit.nest.graph;
                            if (stateGraph == null) continue;

                            var item = (graphReference, stateUnit as IGraphElement);

                            if (item.Item2 is T typedElement)
                            {
                                unitResult.Add(item);
                                yield return (item.graphReference, typedElement);
                            }

                            var childReference = graphReference.ChildReference(stateUnit, false);
                            foreach (var childItem in TraverseStateGraph<T>(childReference))
                            {
                                unitResult.Add(childItem);
                                yield return childItem;
                            }
                            break;
                        }
                    default:
                        {
                            var defaultItem = (graphReference, element);
                            if (defaultItem.element is T typedElement)
                            {
                                unitResult.Add(defaultItem);
                                yield return (defaultItem.graphReference, typedElement);
                            }
                            break;
                        }
                }

                graphCache.ElementCache[element] = unitResult;
            }
        }

        public static IEnumerable<(GraphReference, T)> TraverseStateGraph<T>(GraphReference graphReference) where T : IGraphElement
        {
            if (graphReference.graph is not StateGraph stateGraph) yield break;

            if (!TraversalCache.TryGetValue(graphReference, out var graphCache))
            {
                graphCache = (stateGraph.states.Count + stateGraph.transitions.Count, new Dictionary<IGraphElement, HashSet<(GraphReference, IGraphElement)>>());
                TraversalCache[graphReference] = graphCache;
            }

            var totalElements = stateGraph.elements.Count;
            if (graphCache.ElementCount != totalElements)
            {
                graphCache.ElementCache.Clear();
                graphCache.ElementCount = totalElements;
            }

            foreach (var element in stateGraph.states)
            {
                if (element == null || element is not T) continue;

                if (graphCache.ElementCache.TryGetValue(element, out var cachedResult))
                {
                    foreach (var cachedItem in cachedResult)
                    {
                        if (cachedItem.Item2 is T typedElement)
                            yield return (cachedItem.Item1, typedElement);
                    }
                    continue;
                }

                var unitResult = new HashSet<(GraphReference, IGraphElement)>();

                switch (element)
                {
                    case FlowState flowState:
                        {
                            var graph = flowState.nest.graph;
                            if (graph == null) continue;

                            if (flowState is T _state)
                            {
                                unitResult.Add((graphReference, _state));
                                yield return (graphReference, _state);
                            }

                            var childReference = graphReference.ChildReference(flowState, false);
                            foreach (var childItem in TraverseFlowGraph<T>(childReference))
                            {
                                unitResult.Add(childItem);
                                yield return childItem;
                            }
                            break;
                        }
                    case SuperState superState:
                        {
                            var subStateGraph = superState.nest.graph;
                            if (subStateGraph == null) continue;

                            if (superState is T _state)
                            {
                                unitResult.Add((graphReference, _state));
                                yield return (graphReference, _state);
                            }
                            var childReference = graphReference.ChildReference(superState, false);
                            foreach (var childItem in TraverseStateGraph<T>(childReference))
                            {
                                unitResult.Add(childItem);
                                yield return childItem;
                            }
                            break;
                        }
                    case AnyState:
                        continue;
                    default:
                        {
                            var defaultItem = (graphReference, element);
                            if (defaultItem.element is T typedElement)
                            {
                                unitResult.Add(defaultItem);
                                yield return (defaultItem.graphReference, typedElement);
                            }
                            break;
                        }
                }

                graphCache.ElementCache[element] = unitResult;
            }

            foreach (var transition in stateGraph.transitions)
            {
                if (transition is not FlowStateTransition flowStateTransition) continue;

                if (graphCache.ElementCache.TryGetValue(transition, out var cachedResult))
                {
                    foreach (var cachedItem in cachedResult)
                    {
                        if (cachedItem.Item2 is T typedElement)
                        {
                            yield return (cachedItem.Item1, typedElement);
                        }
                    }
                    continue;
                }

                var unitResult = new HashSet<(GraphReference, IGraphElement)>();
                var graph = flowStateTransition.nest.graph;
                if (graph == null) continue;

                if (flowStateTransition is T _stateTransition)
                {
                    unitResult.Add((graphReference, _stateTransition));
                    yield return (graphReference, _stateTransition);
                }
                var childReference = graphReference.ChildReference(flowStateTransition, false);
                foreach (var childItem in TraverseFlowGraph<T>(childReference))
                {
                    unitResult.Add(childItem);
                    yield return childItem;
                }

                graphCache.ElementCache[transition] = unitResult;
            }
        }
    }
}