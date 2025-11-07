using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

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
    }
}
