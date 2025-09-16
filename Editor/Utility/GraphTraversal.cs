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
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
            var nodePath = reference;
            var pathNames = "";
            while (nodePath != null)
=======
            string BuildPrefix(GraphReference nodePath, GraphReference rootRef)
>>>>>>> Stashed changes
=======
            string BuildPrefix(GraphReference nodePath, GraphReference rootRef)
>>>>>>> Stashed changes
=======
            string BuildPrefix(GraphReference nodePath, GraphReference rootRef)
>>>>>>> Stashed changes
=======
            string BuildPrefix(GraphReference nodePath, GraphReference rootRef)
>>>>>>> Stashed changes
            {
                var prefix = "::";
                if (nodePath.graph != null)
                {
                    if (string.IsNullOrEmpty(nodePath.graph.title))
                    {
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
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
=======
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
            {
                var parent = reference.GetParent<INesterState>();
                return GetNesterStateName(parent);
            }
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
    
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
=======
            else if (reference.IsWithin<INesterStateTransition>())
            {
                var parent = reference.GetParent<INesterStateTransition>();
                return GetNesterStateTransitionName(parent);
>>>>>>> Stashed changes
=======
            else if (reference.IsWithin<INesterStateTransition>())
            {
                var parent = reference.GetParent<INesterStateTransition>();
                return GetNesterStateTransitionName(parent);
>>>>>>> Stashed changes
=======
            else if (reference.IsWithin<INesterStateTransition>())
            {
                var parent = reference.GetParent<INesterStateTransition>();
                return GetNesterStateTransitionName(parent);
>>>>>>> Stashed changes
=======
            else if (reference.IsWithin<INesterStateTransition>())
            {
                var parent = reference.GetParent<INesterStateTransition>();
                return GetNesterStateTransitionName(parent);
>>>>>>> Stashed changes
            }
            return "Embed " + reference.parent.GetType().Name;
        }
<<<<<<< Updated upstream
    
        public static IEnumerable<(GraphReference, Unit)> TraverseStateGraph(GraphReference graphReference)
        {
            var stateGraph = graphReference.graph as StateGraph;
            if (stateGraph == null) yield break;
    
=======

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
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
=======
=======
=======

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
>>>>>>> Stashed changes

            if (!TraversalCache.TryGetValue(graphReference, out var graphCache))
            {
                graphCache = (flowGraph.elements.Count, new Dictionary<IGraphElement, HashSet<(GraphReference, IGraphElement)>>());
                TraversalCache[graphReference] = graphCache;
            }

<<<<<<< Updated upstream
            if (graphCache.ElementCount != flowGraph.elements.Count)
            {
                graphCache.ElementCache.Clear();
                graphCache.ElementCount = flowGraph.elements.Count;
            }

            foreach (var element in flowGraph.elements)
=======
            var totalElements = stateGraph.elements.Count;
            if (graphCache.ElementCount != totalElements)
            {
                graphCache.ElementCache.Clear();
                graphCache.ElementCount = totalElements;
            }

            foreach (var element in stateGraph.states)
>>>>>>> Stashed changes
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

<<<<<<< Updated upstream
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
>>>>>>> Stashed changes

            if (!TraversalCache.TryGetValue(graphReference, out var graphCache))
            {
                graphCache = (flowGraph.elements.Count, new Dictionary<IGraphElement, HashSet<(GraphReference, IGraphElement)>>());
                TraversalCache[graphReference] = graphCache;
            }

<<<<<<< Updated upstream
            if (graphCache.ElementCount != flowGraph.elements.Count)
            {
                graphCache.ElementCache.Clear();
                graphCache.ElementCount = flowGraph.elements.Count;
            }

            foreach (var element in flowGraph.elements)
=======
            var totalElements = stateGraph.elements.Count;
            if (graphCache.ElementCount != totalElements)
            {
                graphCache.ElementCache.Clear();
                graphCache.ElementCount = totalElements;
            }

            foreach (var element in stateGraph.states)
>>>>>>> Stashed changes
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

<<<<<<< Updated upstream
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
>>>>>>> Stashed changes

            if (!TraversalCache.TryGetValue(graphReference, out var graphCache))
            {
                graphCache = (flowGraph.elements.Count, new Dictionary<IGraphElement, HashSet<(GraphReference, IGraphElement)>>());
                TraversalCache[graphReference] = graphCache;
            }

<<<<<<< Updated upstream
            if (graphCache.ElementCount != flowGraph.elements.Count)
            {
                graphCache.ElementCache.Clear();
                graphCache.ElementCount = flowGraph.elements.Count;
            }

            foreach (var element in flowGraph.elements)
=======
            var totalElements = stateGraph.elements.Count;
            if (graphCache.ElementCount != totalElements)
            {
                graphCache.ElementCache.Clear();
                graphCache.ElementCount = totalElements;
            }

            foreach (var element in stateGraph.states)
>>>>>>> Stashed changes
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

<<<<<<< Updated upstream
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

>>>>>>> Stashed changes
            if (!TraversalCache.TryGetValue(graphReference, out var graphCache))
            {
                graphCache = (stateGraph.states.Count + stateGraph.transitions.Count, new Dictionary<IGraphElement, List<(GraphReference, IGraphElement)>>());
                TraversalCache[graphReference] = graphCache;
            }
<<<<<<< Updated upstream
    
            var totalUnits = stateGraph.states.Count + stateGraph.transitions.Count;
            if (graphCache.UnitCount != totalUnits)
=======

            var totalElements = stateGraph.elements.Count;
            if (graphCache.ElementCount != totalElements)
>>>>>>> Stashed changes
            {
                graphCache.ElementCache.Clear();
                graphCache.ElementCount = totalElements;
            }
<<<<<<< Updated upstream
    
            foreach (var state in stateGraph.states)
=======

            foreach (var element in stateGraph.states)
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
    
                var unitResult = new List<(GraphReference, IGraphElement)>();
    
                switch (state)
=======

                var unitResult = new HashSet<(GraphReference, IGraphElement)>();

                switch (element)
>>>>>>> Stashed changes
=======
                switch (element)
>>>>>>> Stashed changes
=======
                switch (element)
>>>>>>> Stashed changes
=======
                switch (element)
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
    
                graphCache.UnitCache[state] = unitResult;
=======
=======
=======
=======

                graphCache.ElementCache[element] = unitResult;
            }
>>>>>>> Stashed changes

                graphCache.ElementCache[element] = unitResult;
            }
>>>>>>> Stashed changes

                graphCache.ElementCache[element] = unitResult;
            }
>>>>>>> Stashed changes

                graphCache.ElementCache[element] = unitResult;
>>>>>>> Stashed changes
            }
    
            foreach (var transition in stateGraph.transitions)
            {
                if (transition is not FlowStateTransition flowStateTransition) continue;
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
    
                if (graphCache.UnitCache.TryGetValue(transition, out var cachedResult))
=======

                if (graphCache.ElementCache.TryGetValue(transition, out var cachedResult))
>>>>>>> Stashed changes
=======

                if (graphCache.ElementCache.TryGetValue(transition, out var cachedResult))
>>>>>>> Stashed changes
=======

                if (graphCache.ElementCache.TryGetValue(transition, out var cachedResult))
>>>>>>> Stashed changes
=======

                if (graphCache.ElementCache.TryGetValue(transition, out var cachedResult))
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
    
                var unitResult = new List<(GraphReference, IGraphElement)>();
                var graph = flowStateTransition.nest.embed ?? flowStateTransition.nest.graph;
=======

                var unitResult = new HashSet<(GraphReference, IGraphElement)>();
                var graph = flowStateTransition.nest.graph;
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
    
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
=======

                graphCache.ElementCache[transition] = unitResult;
>>>>>>> Stashed changes
=======

                graphCache.ElementCache[transition] = unitResult;
>>>>>>> Stashed changes
=======

                graphCache.ElementCache[transition] = unitResult;
>>>>>>> Stashed changes
=======

                graphCache.ElementCache[transition] = unitResult;
>>>>>>> Stashed changes
            }
        }
    } 
}