using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
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
            var nodePath = reference;
            var pathNames = "";
            while (nodePath != null)
            {
                var prefix = "::";
                if (nodePath.graph != null)
                {
                    if (string.IsNullOrEmpty(nodePath.graph.title))
                    {
                        if (!nodePath.isRoot)
                        {
                            prefix = nodePath.graph.GetType().ToString().Split(".").Last();
                        }
                        else
                        {
                            if (reference.root is MethodDeclaration methodDeclaration)
                                prefix = methodDeclaration.methodName;
                            else if (reference.root is ConstructorDeclaration constructorDeclaration)
                                prefix = constructorDeclaration.name;
                            else if (reference.root is PropertyGetterMacro propertyGetterMacro)
                                prefix = propertyGetterMacro.name;
                            else if (reference.root is PropertySetterMacro propertySetterMacro)
                                prefix = propertySetterMacro.name;
                            else if (reference.IsWithin<SubgraphUnit>())
                            {
                                var parent = reference.GetParent<SubgraphUnit>();
                                prefix = parent.nest.source == GraphSource.Macro ? parent.nest.macro.name : nodePath.graph.GetType().ToString().Split(".").Last();
                            }
                            else if (reference.isRoot && reference.root is Object asset && reference.root is not IEventMachine)
                            {
                                prefix = asset.name;
                            }
                            else if (reference.isRoot && reference.root is IEventMachine eventMachine)
                            {
                                prefix = eventMachine.GetType().ToString();
                            }
                            else
                                prefix = nodePath.graph.GetType().ToString().Split(".").Last();
                        }
                    }
                    else
                    {
                        prefix = nodePath.graph.title;
                    }
    
                    prefix += pathPrefix;
                }
    
                pathNames = prefix + pathNames;
                nodePath = nodePath.ParentReference(false);
            }
    
            return pathNames;
        }
    
    
        private static readonly Dictionary<GraphReference, (int UnitCount, Dictionary<IGraphElement, List<(GraphReference, IGraphElement)>> UnitCache)> TraversalCache = new();
    
        public static IEnumerable<(GraphReference, Unit)> TraverseFlowGraph(GraphReference graphReference)
        {
            var flowGraph = graphReference.graph as FlowGraph;
            if (flowGraph == null) yield break;
    
            if (!TraversalCache.TryGetValue(graphReference, out var graphCache))
            {
                graphCache = (flowGraph.units.Count, new Dictionary<IGraphElement, List<(GraphReference, IGraphElement)>>());
                TraversalCache[graphReference] = graphCache;
            }
    
            if (graphCache.UnitCount != flowGraph.units.Count)
            {
                graphCache.UnitCache.Clear();
                graphCache.UnitCount = flowGraph.units.Count;
            }
    
            foreach (var element in flowGraph.units)
            {
                var unit = element as Unit;
                if (unit == null) continue;
    
                if (graphCache.UnitCache.TryGetValue(unit, out var cachedResult))
                {
                    foreach (var cachedItem in cachedResult)
                    {
                        yield return ((GraphReference, Unit))cachedItem;
                    }
                    continue;
                }
    
                var unitResult = new List<(GraphReference, IGraphElement)>();
                switch (unit)
                {
                    case SubgraphUnit subgraphUnit:
                        {
                            var subGraph = subgraphUnit.nest.embed ?? subgraphUnit.nest.graph;
                            if (subGraph == null) continue;
                            var item = (graphReference, subgraphUnit);
                            unitResult.Add(item);
                            yield return item;
    
                            var childReference = graphReference.ChildReference(subgraphUnit, false);
                            foreach (var childItem in TraverseFlowGraph(childReference))
                            {
                                unitResult.Add(childItem);
                                yield return childItem;
                            }
                            break;
                        }
                    case StateUnit stateUnit:
                        {
                            var stateGraph = stateUnit.nest.embed ?? stateUnit.nest.graph;
                            if (stateGraph == null) continue;
                            var childReference = graphReference.ChildReference(stateUnit, false);
                            foreach (var childItem in TraverseStateGraph(childReference))
                            {
                                unitResult.Add(childItem);
                                yield return childItem;
                            }
                            break;
                        }
                    default:
                        {
                            var defaultItem = (graphReference, unit);
                            unitResult.Add(defaultItem);
                            yield return defaultItem;
                            break;
                        }
                }
    
                graphCache.UnitCache[unit] = unitResult;
            }
        }
    
        public static IEnumerable<(GraphReference, Unit)> TraverseStateGraph(GraphReference graphReference)
        {
            var stateGraph = graphReference.graph as StateGraph;
            if (stateGraph == null) yield break;
    
            if (!TraversalCache.TryGetValue(graphReference, out var graphCache))
            {
                graphCache = (stateGraph.states.Count + stateGraph.transitions.Count, new Dictionary<IGraphElement, List<(GraphReference, IGraphElement)>>());
                TraversalCache[graphReference] = graphCache;
            }
    
            var totalUnits = stateGraph.states.Count + stateGraph.transitions.Count;
            if (graphCache.UnitCount != totalUnits)
            {
                graphCache.UnitCache.Clear();
                graphCache.UnitCount = totalUnits;
            }
    
            foreach (var state in stateGraph.states)
            {
                if (graphCache.UnitCache.TryGetValue(state, out var cachedResult))
                {
                    foreach (var cachedItem in cachedResult)
                    {
                        yield return ((GraphReference, Unit))cachedItem;
                    }
                    continue;
                }
    
                var unitResult = new List<(GraphReference, IGraphElement)>();
    
                switch (state)
                {
                    case FlowState flowState:
                        {
                            var graph = flowState.nest.embed ?? flowState.nest.graph;
                            if (graph == null) continue;
                            var childReference = graphReference.ChildReference(flowState, false);
                            foreach (var childItem in TraverseFlowGraph(childReference))
                            {
                                unitResult.Add(childItem);
                                yield return childItem;
                            }
                            break;
                        }
                    case SuperState superState:
                        {
                            var subStateGraph = superState.nest.embed ?? superState.nest.graph;
                            if (subStateGraph == null) continue;
                            var childReference = graphReference.ChildReference(superState, false);
                            foreach (var childItem in TraverseStateGraph(childReference))
                            {
                                unitResult.Add(childItem);
                                yield return childItem;
                            }
                            break;
                        }
                    case AnyState:
                        continue;
                }
    
                graphCache.UnitCache[state] = unitResult;
            }
    
            foreach (var transition in stateGraph.transitions)
            {
                if (transition is not FlowStateTransition flowStateTransition) continue;
    
                if (graphCache.UnitCache.TryGetValue(transition, out var cachedResult))
                {
                    foreach (var cachedItem in cachedResult)
                    {
                        yield return ((GraphReference, Unit))cachedItem;
                    }
                    continue;
                }
    
                var unitResult = new List<(GraphReference, IGraphElement)>();
                var graph = flowStateTransition.nest.embed ?? flowStateTransition.nest.graph;
                if (graph == null) continue;
                var childReference = graphReference.ChildReference(flowStateTransition, false);
                foreach (var childItem in TraverseFlowGraph(childReference))
                {
                    unitResult.Add(childItem);
                    yield return childItem;
                }
    
                graphCache.UnitCache[transition] = unitResult;
            }
        }
    
        private static readonly Dictionary<(GraphElementCollection<IState>, GraphConnectionCollection<IStateTransition, IState, IState>, SuperState), (int UnitCount, List<(List<SuperState>, FlowStateTransition, FlowGraph)>)> SubStateCache = new();
    
        public static IEnumerable<(List<SuperState>, FlowStateTransition, FlowGraph)> GetSubStates(
            GraphElementCollection<IState> states,
            GraphConnectionCollection<IStateTransition, IState, IState> transitions,
            SuperState parent,
            List<SuperState> nestParent)
        {
            var cacheKey = (states, transitions, parent);
    
            if (SubStateCache.TryGetValue(cacheKey, out var cache))
            {
                var totalUnits = states.Count + transitions.Count;
                if (cache.UnitCount != totalUnits)
                {
                    SubStateCache[cacheKey] = (totalUnits, new List<(List<SuperState>, FlowStateTransition, FlowGraph)>());
                }
                else
                {
                    foreach (var item in cache.Item2)
                    {
                        yield return item;
                    }
                    yield break;
                }
            }
            else
            {
                SubStateCache[cacheKey] = (states.Count + transitions.Count, new List<(List<SuperState>, FlowStateTransition, FlowGraph)>());
            }
    
            var result = SubStateCache[cacheKey].Item2;
            nestParent = new List<SuperState>(nestParent) { parent };
    
            foreach (var state in states)
            {
                if (state is not FlowState flowState) continue;
    
                var graph = flowState.nest.embed ?? flowState.nest.graph;
                if (graph == null) continue;
    
                (List<SuperState>, FlowStateTransition, FlowGraph) item = (nestParent, null, graph);
                result.Add(item);
                yield return item;
            }
    
            foreach (var transition in transitions)
            {
                if (transition is not FlowStateTransition flowStateTransition) continue;
    
                var graph = flowStateTransition.nest.embed ?? flowStateTransition.nest.graph;
                if (graph == null) continue;
    
                var item = (nestParent, flowStateTransition, graph);
                result.Add(item);
                yield return item;
            }
    
            foreach (var subState in states)
            {
                if (subState is not SuperState subSuperState) continue;
    
                var subStateGraph = subSuperState.nest.graph;
                var subTransitions = subStateGraph.transitions;
    
                foreach (var item in GetSubStates(subStateGraph.states, subTransitions, subSuperState, nestParent))
                {
                    result.Add(item);
                    yield return item;
                }
            }
        }
    } 
}