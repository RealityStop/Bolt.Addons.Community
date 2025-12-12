using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.VisualScripting.Community
{
    public static class GraphUtility
    {
        public static void AddNewUnitContextual(FlowGraph graph, FlowCanvas canvas, System.Action<IGraphElement> added, System.Action canceled = null)
        {
            OverrideContextIfNeeded(async () =>
            {
                UndoUtility.RecordEditedObject("Added new unit");
                canvas.NewUnitContextual();
                var elementResult = await WaitForElementAddedOrCanceled(graph);
                if (elementResult.Canceled)
                {
                    canceled?.Invoke();
                    return;
                }

                added?.Invoke(elementResult.Element);
            });
        }

        /// <summary>
        /// Adds a new unit with its position based on the ports.
        /// </summary>
        public static void AddNewPositionedUnit(FlowGraph graph, FlowCanvas canvas, IUnitPort unitPort, System.Action<Unit> added, System.Action canceled = null)
        {
            OverrideContextIfNeeded(() =>
            {
                AddNewUnitContextual(graph, canvas, (element) =>
                {
                    var unit = element as Unit;
                    if (unit == null) return;

                    UndoUtility.RecordEditedObject("Added new positioned unit");
                    if (unitPort is ValueInput)
                    {
                        var unitPos = unitPort.unit.position;
                        unit.position = new Vector2(unitPos.x - 150, unitPos.y + 150);
                    }
                    else if (unitPort is ValueOutput)
                    {
                        var unitPos = unitPort.unit.position;
                        unit.position = new Vector2(unitPos.x + 150, unitPos.y + 150);
                    }
                    else if (unitPort is ControlOutput)
                    {
                        var unitPos = unitPort.unit.position;
                        unit.position = new Vector2(unitPos.x + 250, unitPos.y);
                    }
                    else if (unitPort is ControlInput)
                    {
                        var unitPos = unitPort.unit.position;
                        unit.position = new Vector2(unitPos.x - 250, unitPos.y);
                    }
                    graph.pan = unit.position;
                    added?.Invoke(unit);
                }, canceled);
            });
        }

        private static List<IGraph> waitingGraphs = new List<IGraph>();
        public static void WaitForNewUnit(FlowGraph graph, System.Action<IGraphElement> added, System.Action canceled = null)
        {
            if (waitingGraphs.Contains(graph)) return;
            OverrideContextIfNeeded(async () =>
            {
                waitingGraphs.Add(graph);

                var elementResult = await WaitForElementAddedOrCanceled(graph);

                waitingGraphs.Remove(graph);

                if (elementResult.Canceled)
                {
                    canceled?.Invoke();
                    return;
                }

                added?.Invoke(elementResult.Element);
            });
        }

        public static void OverrideContextIfNeeded(System.Action action)
        {
            bool manuallySet = false;
            if (LudiqGraphsEditorUtility.editedContext.value == null && GraphWindow.active != null && GraphWindow.active.reference != null)
            {
                var context = GraphWindow.active.reference.Context();
                if (context != null)
                {
                    manuallySet = true;
                    LudiqGraphsEditorUtility.editedContext.BeginOverride(context);
                }
            }
            action?.Invoke();
            if (manuallySet) LudiqGraphsEditorUtility.editedContext.EndOverride();
        }

        public static void DescribeAnalyzeAndDefineFlowGraph(this IGraphContext context)
        {
            if (context?.graph is FlowGraph graph)
            {
                context.DescribeAndAnalyze();
                foreach (var unit in graph.units)
                {
                    unit.Define();
                }
            }
            else
            {
                context?.DescribeAndAnalyze();
            }
        }

        public static void CleanGraphFrom(Unit startUnit)
        {
            var visited = new HashSet<Unit>();
            UndoUtility.RecordEditedObject("Cleaned Graph");
            ArrangeUnitRecursive(startUnit, startUnit.position, visited);
        }

        static void ArrangeUnitRecursive(Unit unit, Vector2 basePosition, HashSet<Unit> visited)
        {
            if (!visited.Add(unit)) return;

            unit.position = basePosition;
            var widget = FindNonOverlappingPosition(unit, 5);
            float spacingY = 150f + widget.position.height;
            float spacingX = 150f + widget.position.width;

            var controlOutputs = unit.controlOutputs.ToList();
            for (int i = 0; i < controlOutputs.Count; i++)
            {
                var port = controlOutputs[i];
                var connections = port.validConnections.ToList();
                for (int j = 0; j < connections.Count; j++)
                {
                    var connectedUnit = connections[j].destination.unit;
                    var offset = new Vector2(spacingX, (i - controlOutputs.Count / 2f) * spacingY);
                    ArrangeUnitRecursive(connectedUnit as Unit, basePosition + offset, visited);
                }
            }

            var valueInputs = unit.valueInputs.ToList();
            for (int i = 0; i < valueInputs.Count; i++)
            {
                var port = valueInputs[i];
                var connection = port.validConnections.FirstOrDefault();
                if (connection != null)
                {
                    var connectedUnit = connection.source.unit;
                    if (connectedUnit.controlInputs.Count == 0 && connectedUnit.controlOutputs.Count > 0) continue; // Most likely a source unit
                    var offset = new Vector2(-spacingX, (i - valueInputs.Count / 2f) * spacingY);
                    ArrangeUnitRecursive(connectedUnit as Unit, basePosition + offset, visited);
                }
            }

            widget?.Reposition();
        }

        static IUnitWidget FindNonOverlappingPosition(Unit unit, float minDistance)
        {
            var canvas = unit.graph.Canvas<FlowCanvas>();
            var unitWidget = canvas.Widget<IUnitWidget>(unit);
            int tries = 0;
            int maxTries = 50;
            while (PositionOverlaps(canvas, unitWidget, 8))
            {
                unitWidget.position = new Rect(unitWidget.position.position + new Vector2(minDistance, minDistance), unitWidget.position.size).PixelPerfect();

                unitWidget.CachePositionFirstPass();
                unitWidget.CachePosition();

                unitWidget.Reposition();
                if (++tries > maxTries)
                {
                    Debug.LogWarning($"Max repositioning attemptes reached! Could not find a non overlapping position for {unitWidget.element}.");
                    return unitWidget;
                }
            }
            return unitWidget;
        }

        public static bool PositionOverlaps(FlowCanvas canvas, IGraphElementWidget widget, float threshold = 3)
        {
            var position = widget.position;

            return canvas.graph.units.Any(otherElement =>
            {
                // Skip itself, which would by definition always overlap
                if (otherElement == widget.element)
                {
                    return false;
                }

                var positionA = canvas.Widget(otherElement).position;
                var positionB = position;

                return Mathf.Abs(positionA.xMin - positionB.xMin) < threshold &&
                Mathf.Abs(positionA.yMin - positionB.yMin) < threshold;
            });
        }

        private static Task<ElementAddResult> WaitForElementAddedOrCanceled(FlowGraph graph)
        {
            var tcs = new TaskCompletionSource<ElementAddResult>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            void AddedHandler(IGraphElement element)
            {
                graph.elements.ItemAdded -= AddedHandler;
                EditorApplication.update -= CheckClosed;
                tcs.TrySetResult(new ElementAddResult { Canceled = false, Element = element });
            }

            void CheckClosed()
            {
                if (FuzzyWindow.instance == null)
                {
                    graph.elements.ItemAdded -= AddedHandler;
                    EditorApplication.update -= CheckClosed;
                    tcs.TrySetResult(new ElementAddResult { Canceled = true, Element = null });
                }
            }

            void WaitForOpen()
            {
                if (FuzzyWindow.instance != null)
                {
                    EditorApplication.update -= WaitForOpen;

                    graph.elements.ItemAdded += AddedHandler;
                    EditorApplication.update += CheckClosed;
                }
            }

            EditorApplication.update += WaitForOpen;

            return tcs.Task;
        }
        private struct ElementAddResult
        {
            public bool Canceled;
            public IGraphElement Element;
        }

        /// <summary>
        /// Looks for the root a the flow.
        /// </summary>
        /// <param name="unit">The Unit to start searching from</param>
        /// <param name="reference">Optional: if null it will not check parent graphs if there is one</param>
        /// <returns>All the root's that were found</returns>
        public static IEnumerable<Unit> GetFlowSourceUnits(Unit unit, GraphReference reference = null)
        {
            return GetFlowSourceUnitsInternal(unit, new HashSet<Unit>(), reference);
        }

        private static IEnumerable<Unit> GetFlowSourceUnitsInternal(Unit unit, HashSet<Unit> visited = null, GraphReference reference = null, string key = null)
        {
            visited ??= new HashSet<Unit>();

            if (unit == null || visited.Contains(unit))
                yield break;

            visited.Add(unit);

            var flowInputs = unit.controlInputs.Where(i => i.hasValidConnection).ToList();
            if (flowInputs.Count > 0)
            {
                foreach (var input in flowInputs)
                {
                    foreach (var conn in input.connections)
                    {
                        if (conn?.source?.unit is Unit source)
                        {
                            foreach (var upstream in GetFlowSourceUnitsInternal(source, visited, reference, conn.source.key))
                                yield return upstream;
                        }
                    }
                }
                yield break;
            }

            if (unit is GraphInput gi && reference != null && reference.parentElement is INesterUnit nester)
            {
                var parent = reference.ParentReference(false);

                var correspondingParentInput = nester.controlInputs.FirstOrDefault(ci => ci.key == key);

                if (!string.IsNullOrEmpty(key) && correspondingParentInput != null && correspondingParentInput.hasValidConnection)
                {
                    if (correspondingParentInput != null)
                    {
                        foreach (var conn in correspondingParentInput.connections)
                        {
                            if (conn?.source?.unit is Unit src)
                            {
                                foreach (var upstream in GetFlowSourceUnitsInternal(src, visited, parent, key))
                                    yield return upstream;
                            }
                        }
                    }
                }
                else if (correspondingParentInput != null && !correspondingParentInput.hasValidConnection)
                {
                    yield return nester as Unit;
                }
                else
                {
                    foreach (var upstream in GetFlowSourceUnitsInternal(nester as Unit, visited, parent, key))
                        yield return upstream;
                }
                yield break;
            }

            var valueOutputs = unit.valueOutputs.Where(v => v.hasValidConnection).ToList();
            if (valueOutputs.Count > 0)
            {
                foreach (var output in valueOutputs)
                {
                    foreach (var conn in output.connections)
                    {
                        if (conn?.destination?.unit is Unit destination)
                        {
                            foreach (var upstream in GetFlowSourceUnitsInternal(destination, visited, reference, key))
                                yield return upstream;
                        }
                    }
                }

                yield break;
            }

            yield return unit;
        }

        public static List<T> GetUnitsOfType<T>(Unit unit, Func<T, bool> predicate = null, bool enterNests = true) where T : Unit
        {
            return GetUnitsOfTypeInternal(unit, predicate, new HashSet<Unit>(), enterNests);
        }

        private static List<T> GetUnitsOfTypeInternal<T>(Unit unit, Func<T, bool> predicate = null, HashSet<Unit> visited = null, bool enterNests = true, string key = null) where T : Unit
        {
            var results = new List<T>();

            if (unit == null || visited.Contains(unit))
                return results;

            visited.Add(unit);

            if (unit is T typedUnit && (predicate == null || predicate(typedUnit)))
                results.Add(typedUnit);

            if (enterNests && unit is INesterUnit nesterUnit && nesterUnit.nest != null && nesterUnit.nest.graph is FlowGraph graph)
            {
                if (graph.units.FirstOrDefault(e => e is GraphInput) is GraphInput graphInput)
                {
                    if (graphInput is T typed && (predicate == null || predicate(typed)))
                        results.Add(typed);

                    var correspondingControlOutput = graphInput.controlOutputs.FirstOrDefault(c => c.key == key);
                    if (correspondingControlOutput != null && correspondingControlOutput.hasValidConnection)
                        results.AddRange(GetUnitsOfTypeInternal<T>(correspondingControlOutput.connection.destination.unit as Unit, predicate, visited, enterNests, key));
                }
            }

            foreach (var output in unit.controlOutputs)
            {
                if (!output.hasValidConnection)
                    continue;

                var connection = output.connection;

                if (connection?.destination?.unit is Unit destUnit)
                {
                    if (destUnit is T typed && (predicate == null || predicate(typed)))
                        results.Add(typed);

                    results.AddRange(GetUnitsOfTypeInternal<T>(destUnit, predicate, visited, enterNests, connection.destination.key));
                }
            }

            foreach (var input in unit.valueInputs)
            {
                if (!input.hasValidConnection)
                    continue;

                var connection = input.connection;

                if (connection?.source?.unit is Unit sourceUnit)
                {
                    if (sourceUnit is T typed && (predicate == null || predicate(typed)))
                        results.Add(typed);

                    results.AddRange(GetUnitsOfTypeInternal<T>(sourceUnit, predicate, visited, enterNests, key));
                }
            }

            return results;
        }

        public static T GetFirstUnitOfType<T>(Unit unit, Func<T, bool> predicate = null, bool enterNests = true) where T : Unit
        {
            return GetFirstUnitOfTypeInternal(unit, predicate, new HashSet<Unit>(), enterNests);
        }

        private static T GetFirstUnitOfTypeInternal<T>(Unit unit, Func<T, bool> predicate = null, HashSet<Unit> visited = null, bool enterNests = true, string key = null) where T : Unit
        {
            if (unit == null || visited.Contains(unit))
                return null;

            visited.Add(unit);

            if (unit is T typedUnit && (predicate == null || predicate(typedUnit)))
                return typedUnit;

            if (enterNests && unit is INesterUnit nesterUnit && nesterUnit.nest != null && nesterUnit.nest.graph is FlowGraph graph)
            {
                if (graph.units.FirstOrDefault(e => e is GraphInput) is GraphInput graphInput)
                {
                    if (graphInput is T typed && (predicate == null || predicate(typed)))
                        return typed;
                    var correspondingControlOutput = graphInput.controlOutputs.FirstOrDefault(c => c.key == key);
                    if (correspondingControlOutput != null && correspondingControlOutput.hasValidConnection)
                    {
                        var result = GetFirstUnitOfTypeInternal<T>(correspondingControlOutput.connection.destination.unit as Unit, predicate, visited, enterNests, key);
                        if (result is T _typed && (predicate == null || predicate(_typed)))
                            return _typed;
                    }
                }
            }

            foreach (var output in unit.controlOutputs)
            {
                if (!output.hasValidConnection)
                    continue;

                var connection = output.connection;

                if (connection?.destination?.unit is Unit destUnit)
                {
                    if (destUnit is T typed && (predicate == null || predicate(typed)))
                        return typed;

                    var result = GetFirstUnitOfTypeInternal<T>(destUnit, predicate, visited, enterNests, connection.destination.key);
                    if (result != null)
                        return result;
                }
            }

            foreach (var input in unit.valueInputs)
            {
                if (!input.hasValidConnection)
                    continue;

                var connection = input.connection;

                if (connection?.source?.unit is Unit sourceUnit)
                {
                    if (sourceUnit is T typed && (predicate == null || predicate(typed)))
                        return typed;

                    var result = GetFirstUnitOfTypeInternal<T>(sourceUnit, predicate, visited, enterNests, key);
                    if (result != null)
                        return result;
                }
            }

            return null;
        }

        public static List<UnifiedVariableUnit> GetFlowVariablesRenameTargets(Unit currentUnit, string oldName, GraphReference reference = null)
        {
            List<UnifiedVariableUnit> variableUnits = new List<UnifiedVariableUnit>();
            foreach (var source in GetFlowSourceUnits(currentUnit, reference))
            {
                foreach (var unit in GetUnitsOfType<UnifiedVariableUnit>(source, (v) => v.kind == VariableKind.Flow))
                {
                    if (unit is UnifiedVariableUnit variableUnit && variableUnit.kind == VariableKind.Flow && variableUnit.isDefined)
                    {
                        if (!variableUnit.name.hasValidConnection && (string)variableUnit.defaultValues[variableUnit.name.key] == oldName)
                        {
                            variableUnits.Add(variableUnit);
                        }
                    }
                }
            }

            return variableUnits;
        }

        public static void UpdateAllGraphVariables(FlowGraph graph, string oldName, string newName)
        {
            foreach (var unit in graph.units)
            {
                if (unit is UnifiedVariableUnit variableUnit && variableUnit.kind == VariableKind.Graph && variableUnit.isDefined)
                {
                    if (!variableUnit.name.hasValidConnection && (string)variableUnit.defaultValues[variableUnit.name.key] == oldName)
                    {
                        variableUnit.name.SetDefaultValue(newName);
                    }
                }
            }
        }

        public static List<UnifiedVariableUnit> GetGraphVariablesRenameTargets(FlowGraph graph, string oldName)
        {
            List<UnifiedVariableUnit> variableUnits = new List<UnifiedVariableUnit>();
            foreach (var unit in graph.units)
            {
                if (unit is UnifiedVariableUnit variableUnit && variableUnit.kind == VariableKind.Graph && variableUnit.isDefined)
                {
                    if (!variableUnit.name.hasValidConnection && (string)variableUnit.defaultValues[variableUnit.name.key] == oldName)
                    {
                        variableUnits.Add(variableUnit);
                    }
                }
            }
            return variableUnits;
        }

        public static List<(UnifiedVariableUnit, UnityEngine.Object)> GetObjectVariablesRenameTargets(GameObject gameObject, string oldName)
        {
            if (gameObject.scene != null)
            {
                var results = new List<(UnifiedVariableUnit, UnityEngine.Object)>();
                foreach (var rootGameObject in gameObject.scene.GetRootGameObjects())
                {
                    foreach (var component in rootGameObject.GetComponentsInChildren<IMachine>(true))
                    {
                        results.AddRange(GetFromComponent(component));
                    }
                }
                return results;
            }
            else if (gameObject != null)
            {
                var results = new List<(UnifiedVariableUnit, UnityEngine.Object)>();
                var components = gameObject.GetComponents<IMachine>();

                foreach (var component in components)
                {
                    results.AddRange(GetFromComponent(component));
                }
                return results;
            }
            else
            {
                return new List<(UnifiedVariableUnit, UnityEngine.Object)>();
            }

            List<(UnifiedVariableUnit, UnityEngine.Object)> GetFromComponent(IMachine component)
            {
                if (component == null || component.GetReference() == null || component.nest == null) return new List<(UnifiedVariableUnit, UnityEngine.Object)>();

                List<(UnifiedVariableUnit, UnityEngine.Object)> variableUnits = new List<(UnifiedVariableUnit, UnityEngine.Object)>();

                GraphTraversal.TraverseGraph(component.GetReference().graph, (unit) =>
                {
                    if (unit is UnifiedVariableUnit variableUnit && variableUnit.kind == VariableKind.Object && variableUnit.isDefined)
                    {
                        if (!variableUnit.name.hasValidConnection && (string)variableUnit.defaultValues[variableUnit.name.key] == oldName)
                        {
                            var reference = GraphTraversal.GetChildReferenceWithGraph(component.GetReference().AsReference(), variableUnit.graph);
                            if (Flow.CanPredict(variableUnit.@object, reference))
                            {
                                var value = Flow.Predict(variableUnit.@object, reference);
                                if (value is GameObject @object)
                                {
                                    if (reference != null && @object == gameObject)
                                    {
                                        variableUnits.Add((variableUnit, reference.rootObject));
                                    }
                                }
                            }
                        }
                    }
                });

                return variableUnits;
            }
        }

        public static List<(UnifiedVariableUnit, UnityEngine.Object)> GetObjectVariablesRenameTargets(GraphReference reference, GameObject initialValue, string oldName)
        {
            if (reference == null || reference.graph == null) return new List<(UnifiedVariableUnit, UnityEngine.Object)>();

            List<(UnifiedVariableUnit, UnityEngine.Object)> variableUnits = new List<(UnifiedVariableUnit, UnityEngine.Object)>();

            if (initialValue != null) variableUnits.AddRange(GetObjectVariablesRenameTargets(initialValue, oldName));
            else
            {
                GraphTraversal.TraverseGraph(reference.graph, (unit) =>
                {
                    if (unit is UnifiedVariableUnit variableUnit && variableUnit.kind == VariableKind.Object && variableUnit.isDefined)
                    {
                        if (!variableUnit.name.hasValidConnection && (string)variableUnit.defaultValues[variableUnit.name.key] == oldName)
                        {
                            var graphReference = GraphTraversal.GetChildReferenceWithGraph(reference, variableUnit.graph);
                            if (Flow.CanPredict(variableUnit.@object, graphReference))
                            {
                                var value = Flow.Predict(variableUnit.@object, graphReference);
                                if (value is GameObject @object)
                                {
                                    if (graphReference != null && @object == initialValue)
                                    {
                                        variableUnits.Add((variableUnit, reference.rootObject));
                                    }
                                }
                            }
                        }
                    }
                });
            }

            return variableUnits;
        }

        public static List<(UnifiedVariableUnit, UnityEngine.Object)> GetSceneVariablesRenameTargets(Scene scene, string oldName)
        {
            if (scene != null)
            {
                var results = new List<(UnifiedVariableUnit, UnityEngine.Object)>();
                foreach (var rootGameObject in scene.GetRootGameObjects())
                {
                    foreach (var component in rootGameObject.GetComponentsInChildren<IMachine>(true))
                    {
                        results.AddRange(GetFromComponent(component));
                    }
                }
                return results;
            }
            else
            {
                return new List<(UnifiedVariableUnit, UnityEngine.Object)>();
            }

            List<(UnifiedVariableUnit, UnityEngine.Object)> GetFromComponent(IMachine component)
            {
                if (component == null || component.GetReference() == null || component.nest == null) return new List<(UnifiedVariableUnit, UnityEngine.Object)>();

                List<(UnifiedVariableUnit, UnityEngine.Object)> variableUnits = new List<(UnifiedVariableUnit, UnityEngine.Object)>();

                GraphTraversal.TraverseGraph(component.GetReference().graph, (unit) =>
                {
                    if (unit is UnifiedVariableUnit variableUnit && variableUnit.kind == VariableKind.Scene && variableUnit.isDefined)
                    {
                        if (!variableUnit.name.hasValidConnection && (string)variableUnit.defaultValues[variableUnit.name.key] == oldName)
                        {
                            variableUnits.Add((variableUnit, component.GetReference().rootObject));
                        }
                    }
                });

                return variableUnits;
            }
        }

        public static List<(UnifiedVariableUnit, UnityEngine.Object)> GetSceneVariablesRenameTargets(GraphReference reference, Scene? scene, string oldName)
        {
            if (reference == null || reference.graph == null) return new List<(UnifiedVariableUnit, UnityEngine.Object)>();

            List<(UnifiedVariableUnit, UnityEngine.Object)> variableUnits = new List<(UnifiedVariableUnit, UnityEngine.Object)>();

            if (scene != null) variableUnits.AddRange(GetSceneVariablesRenameTargets(scene.Value, oldName));
            else
            {
                GraphTraversal.TraverseGraph(reference.graph, (unit) =>
                {
                    if (unit is UnifiedVariableUnit variableUnit && variableUnit.kind == VariableKind.Scene && variableUnit.isDefined)
                    {
                        if (!variableUnit.name.hasValidConnection && (string)variableUnit.defaultValues[variableUnit.name.key] == oldName)
                        {
                            variableUnits.Add((variableUnit, reference.rootObject));
                        }
                    }
                });
            }

            return variableUnits;
        }

        public static bool IsSourceLiteral(ValueInput valueInput, out Type sourceType)
        {
            var source = CSharp.NodeGeneration.GetPesudoSource(valueInput);
            if (source != null)
            {
                if (source.unit is Literal literal)
                {
                    sourceType = literal.type;
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

        public static void UpdateAllObjectVariables(GameObject gameObject, string oldName, string newName)
        {
            var group = Undo.GetCurrentGroup();
            foreach (var target in GetObjectVariablesRenameTargets(gameObject, oldName))
            {
                if (target.Item1.name.hasValidConnection) continue;

                Undo.RecordObject(target.Item2, $"Renamed '{oldName}' variable to '{newName}'");

                target.Item1.name.SetDefaultValue(newName);
            }
            Undo.CollapseUndoOperations(group);
        }

        public static void UpdateAllSceneVariables(Scene scene, string oldName, string newName)
        {
            var group = Undo.GetCurrentGroup();
            foreach (var target in GetSceneVariablesRenameTargets(scene, oldName))
            {
                if (target.Item1.name.hasValidConnection) continue;

                Undo.RecordObject(target.Item2, $"Renamed '{oldName}' variable to '{newName}'");

                target.Item1.name.SetDefaultValue(newName);
            }
            Undo.CollapseUndoOperations(group);
        }
    }
}