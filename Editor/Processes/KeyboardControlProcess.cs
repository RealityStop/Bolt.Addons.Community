using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;
namespace Unity.VisualScripting.Community
{
    public class KeyboardControlProcess : GraphProcess<FlowGraph, FlowCanvas>
    {
        bool wasControl = false;
        bool wasValue = false;
        bool wasAll = false;
        int index = -1;
        private double lastTabTime;
        private static HashSet<IGraphElement> visitedElements = new HashSet<IGraphElement>();
        private IGraphElement selectedElement;
        public override void Process(FlowGraph graph, FlowCanvas canvas)
        {
            if (canvas.selection.Count == 0 && visitedElements.Count > 0) visitedElements.Clear(); // Most likely the selection was manually stopped so just reset visited
            if (@event == null || GraphWindow.active == null || EditorWindow.focusedWindow != GraphWindow.active) return;

            if (canvas.selection.Count == 0)
            {
                if (EditorGUIUtility.editingTextField)
                {
                    return;
                }
                if (@event.keyCode == KeyCode.LeftArrow)
                {
                    graph.pan += new Vector2(-20, 0);
                    canvas.UpdateViewport();
                    return;
                }
                else if (@event.keyCode == KeyCode.RightArrow)
                {
                    graph.pan += new Vector2(20, 0);
                    canvas.UpdateViewport();
                    return;
                }
                else if (@event.keyCode == KeyCode.UpArrow)
                {
                    graph.pan += new Vector2(0, -20);
                    canvas.UpdateViewport();
                    return;
                }
                else if (@event.keyCode == KeyCode.DownArrow)
                {
                    graph.pan += new Vector2(0, 20);
                    canvas.UpdateViewport();
                    return;
                }
            }

            if (@event.CtrlOrCmd() && @event.keyCode == KeyCode.Tab && FuzzyWindow.instance == null)
            {
                if (canvas.selection.Count > 0 && selectedElement != canvas.selection.FirstOrDefault())
                {
                    visitedElements.Clear(); // Most likely the selection was manually changed so just reset visited
                    selectedElement = null;
                }

                if (EditorApplication.timeSinceStartup - lastTabTime < 0.2f)
                    return;

                lastTabTime = EditorApplication.timeSinceStartup;

                if (canvas.isCreatingConnection && canvas.connectionSource.hasValidConnection)
                {
                    var source = canvas.connectionSource;
                    if (source is ControlOutput controlOutput)
                    {
                        canvas.selection.Select(controlOutput.connection.destination.unit);
                        GraphUtility.OverrideContextIfNeeded(() => canvas.ViewElements(canvas.selection));
                        canvas.connectionSource = controlOutput.connection.destination;
                        return;
                    }
                    else if (source is ValueInput valueInput)
                    {
                        canvas.selection.Select(valueInput.connection.source.unit);
                        GraphUtility.OverrideContextIfNeeded(() => canvas.ViewElements(canvas.selection));
                        canvas.connectionSource = valueInput.connection.source;
                        return;
                    }
                    else if (source is ValueOutput valueOutput)
                    {
                        canvas.selection.Select(valueOutput.connections.FirstOrDefault()?.destination.unit);
                        GraphUtility.OverrideContextIfNeeded(() => canvas.ViewElements(canvas.selection));
                        canvas.connectionSource = valueOutput.connections.FirstOrDefault()?.destination;
                        return;
                    }
                    else if (source is ControlInput controlInput)
                    {
                        canvas.selection.Select(controlInput.connections.FirstOrDefault()?.source.unit);
                        GraphUtility.OverrideContextIfNeeded(() => canvas.ViewElements(canvas.selection));
                        canvas.connectionSource = controlInput.connections.FirstOrDefault()?.source;
                        return;
                    }
                }

                canvas.CancelConnection();

                var allElements = graph.elements
                    .Except(graph.valueConnections)
                    .Except(graph.controlConnections)
                    .Except(graph.invalidConnections)
                    .OrderBy(e => GetElementX(e))
                    .ToList();

                if (allElements.Count == 0)
                    return;

                var lastSelected = canvas.selection.LastOrDefault();

                List<IGraphElement> cycleElements;
                if (lastSelected is Unit lastUnit && lastUnit.valueInputs.Count > 0)
                {
                    cycleElements = lastUnit.valueInputs
                        .Where(input => input.hasValidConnection)
                        .Select(input => input.connection.source.unit)
                        .Where(u => u != null).Cast<IGraphElement>()
                        .Where(e => !visitedElements.Contains(e))
                        .ToList();

                    if (cycleElements.Count == 0)
                        cycleElements = allElements.Where(e => !visitedElements.Contains(e)).ToList();
                }
                else
                {
                    cycleElements = allElements.Where(e => !visitedElements.Contains(e)).ToList();
                }

                if (cycleElements.Count == 0)
                {
                    visitedElements.Clear();
                    cycleElements = allElements.ToList();
                }

                int currentIndex = lastSelected != null ? cycleElements.IndexOf(lastSelected) : -1;
                int nextIndex = (currentIndex + 1) % cycleElements.Count;

                var nextElement = cycleElements[nextIndex];

                visitedElements.Add(nextElement);
                selectedElement = nextElement;
                canvas.selection.Clear();
                canvas.selection.Add(nextElement);
                GraphUtility.OverrideContextIfNeeded(() => canvas.ViewElements(canvas.selection));
            }

            if (IsAnyArrowKey(@event.keyCode) && @event.type == EventType.KeyDown)
            {
                if (@event.CtrlOrCmd() && !@event.shift)
                {
                    canvas.CancelConnection();
                    var visited = new HashSet<IGraphElement>();
                    if (@event.keyCode == KeyCode.LeftArrow)
                    {
                        foreach (var element in canvas.selection)
                        {
                            MoveElement(element, new Vector2(-20, 0), canvas, visited);
                        }
                        return;
                    }
                    else if (@event.keyCode == KeyCode.RightArrow)
                    {
                        foreach (var element in canvas.selection)
                        {
                            MoveElement(element, new Vector2(20, 0), canvas, visited);
                        }
                        return;
                    }
                    else if (@event.keyCode == KeyCode.UpArrow)
                    {
                        foreach (var element in canvas.selection)
                        {
                            MoveElement(element, new Vector2(0, -20), canvas, visited);
                        }
                        return;
                    }
                    else if (@event.keyCode == KeyCode.DownArrow)
                    {
                        foreach (var element in canvas.selection)
                        {
                            MoveElement(element, new Vector2(0, 20), canvas, visited);
                        }
                        return;
                    }
                }

                var units = canvas.selection.Where(e => e is Unit).OrderBy(u => (u as Unit).position.x);
                if (!units.Any()) return;
                var unit = units.First() as Unit;

                bool isRight = @event.keyCode == KeyCode.RightArrow;

                if (IsArrowKey(@event.keyCode))
                {
                    if (@event.shift)
                    {
                        ResetStateExcept(ref wasControl);
                        var ports = unit.controlInputs.Cast<IUnitPort>()
                            .Concat(unit.controlOutputs).ToList();
                        NavigatePortList(ports, canvas, isRight);
                        return;
                    }
                    else if (@event.alt)
                    {
                        ResetStateExcept(ref wasValue);
                        var ports = unit.valueInputs.Cast<IUnitPort>()
                            .Concat(unit.valueOutputs).ToList();
                        NavigatePortList(ports, canvas, isRight);
                        return;
                    }
                    else
                    {
                        ResetStateExcept(ref wasAll);
                        var ports = new List<IUnitPort>();
                        ports.AddRange(unit.controlInputs);
                        ports.AddRange(unit.valueInputs);
                        ports.AddRange(unit.controlOutputs);
                        ports.AddRange(unit.valueOutputs);
                        NavigatePortList(ports, canvas, isRight);
                        return;
                    }
                }
                else if (@event.keyCode == KeyCode.UpArrow && @event.type == EventType.KeyDown && canvas.isCreatingConnection)
                {
                    if (FuzzyWindow.instance == null)
                        GraphUtility.OverrideContextIfNeeded(() => canvas.NewUnitContextual());
                    return;
                }
            }
        }

        private float GetElementX(IGraphElement element)
        {
            if (element is Unit unit) return unit.position.x;
            if (element is GraphGroup graphGroup) return graphGroup.position.position.x;
#if VISUAL_SCRIPTING_1_8_0_OR_GREATER
            if (element is StickyNote note) return note.position.position.x;
#endif
            return 0f;
        }

        private void MoveElement(IGraphElement element, Vector2 delta, FlowCanvas canvas, HashSet<IGraphElement> visited)
        {
            if (element == null || !visited.Add(element)) return;

            if (element is Unit unit)
            {
                if (BoltCore.Configuration.carryChildren)
                {
                    var targets = HashSetPool<IGraphElement>.New();
                    canvas.Widget<IUnitWidget>(element).ExpandDragGroup(targets);
                    targets.Add(unit);
                    foreach (var unitToMove in targets.Cast<Unit>())
                    {
                        unitToMove.position += delta;
                        canvas.Widget(unitToMove).Reposition();
                    }
                    HashSetPool<IGraphElement>.Free(targets);
                    return;
                }
                unit.position += delta;
            }
            else if (element is GraphGroup group)
            {
                foreach (var graphElement in canvas.graph.elements.OfType<IGraphElement>())
                {
                    if (group.position.Encompasses(canvas.Widget(graphElement).position))
                    {
                        MoveElement(graphElement, delta, canvas, visited);
                    }
                }

                group.position = new Rect(group.position.position + delta, group.position.size);
            }
#if VISUAL_SCRIPTING_1_8_0_OR_GREATER
            else if (element is StickyNote note)
                note.position = new Rect(note.position.position + delta, note.position.size);
#endif
            canvas.Widget(element).Reposition();
        }

        private void ResetStateExcept(ref bool keep)
        {
            if (!keep)
            {
                index = -1;
            }
            wasControl = false;
            wasValue = false;
            wasAll = false;
            keep = true;
        }

        private void NavigatePortList<T>(List<T> ports, FlowCanvas canvas, bool forward) where T : IUnitPort
        {
            if (ports.Count == 0) return;

            index = Mathf.Clamp(forward ? index + 1 : index - 1, 0, ports.Count - 1);
            canvas.connectionSource = ports[index];
        }

        private bool IsArrowKey(KeyCode keyCode)
        {
            return keyCode == KeyCode.LeftArrow || keyCode == KeyCode.RightArrow;
        }

        private bool IsAnyArrowKey(KeyCode keyCode)
        {
            return keyCode == KeyCode.LeftArrow || keyCode == KeyCode.RightArrow || keyCode == KeyCode.UpArrow || keyCode == KeyCode.DownArrow;
        }
    }
}