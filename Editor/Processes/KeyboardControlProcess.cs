using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;

namespace Unity.VisualScripting.Community
{
    public class KeyboardControlProcess : GraphProcess<FlowGraph, FlowCanvas>
    {
        #region Fields

        private bool wasControl = false;
        private bool wasValue = false;
        private bool wasAll = false;
        private int index = -1;

        private double lastTabTime;

        private static HashSet<IGraphElement> visitedElements = new HashSet<IGraphElement>();
        private IGraphElement selectedElement;

        private readonly HashSet<FlowGraph> registeredGraphs = new HashSet<FlowGraph>();

        private bool currentlyCreatingConnection = false;

        private CanvasControlScheme? _originalScheme = null;
        private bool _connectionModeActive = false;

        #endregion

        private bool IsMiddleMouseButton(Event e)
        {
            return e.button == (int)MouseButton.Middle;
        }
        bool wasZooming;
        Vector2 originalPan;
        public override void Process(FlowGraph graph, FlowCanvas canvas)
        {
            if (@event != null && @event.alt && IsMiddleMouseButton(@event))
            {
                if (!wasZooming)
                {
                    wasZooming = true;
                    originalPan = graph.pan;
                }
                var zoomDelta = MathfEx.NearestMultiple(@event.delta.y * BoltCore.Configuration.zoomSpeed, GraphGUI.ZoomSteps) * 0.1f;
                zoomDelta = Mathf.Clamp(graph.zoom + zoomDelta, GraphGUI.MinZoom, GraphGUI.MaxZoom) - graph.zoom;
                if (zoomDelta != 0)
                {
                    var oldZoom = graph.zoom;
                    var newZoom = graph.zoom + zoomDelta;

                    var matrix = MathfEx.ScaleAroundPivot(canvas.mousePosition, (oldZoom / newZoom) * Vector3.one);
                    graph.pan = matrix.MultiplyPoint(originalPan);
                    graph.zoom = newZoom;
                }
            }
            else
            {
                wasZooming = false;
                originalPan = graph.pan;
            }
            HandleControlSchemeOverride(canvas);

            RegisterGraphEventsIfNeeded(graph, canvas);

            if (canvas.selection.Count == 0 && visitedElements.Count > 0)
            {
                // Most likely the selection was manually stopped so just reset visited
                visitedElements.Clear();
            }

            if (@event == null || GraphWindow.active == null || EditorWindow.focusedWindow != GraphWindow.active)
            {
                return;
            }

            if (canvas.selection.Count == 0)
            {
                if (EditorGUIUtility.editingTextField)
                {
                    return;
                }

                if (HandlePanKeysWhenNoSelection(graph, canvas))
                {
                    return;
                }
            }

            if (@event.CtrlOrCmd() && @event.keyCode == KeyCode.Tab && FuzzyWindow.instance == null)
            {
                HandleCtrlTab(graph, canvas);
            }

            if (IsAnyArrowKey(@event.keyCode) && @event.type == EventType.KeyDown)
            {
                HandleArrowKeyActions(graph, canvas);
            }
        }

        private void HandleControlSchemeOverride(FlowCanvas canvas)
        {
            bool creating = canvas.isCreatingConnection;

            if (creating && !_connectionModeActive)
            {
                _connectionModeActive = true;
                _originalScheme = BoltCore.Configuration.controlScheme;
#if VISUAL_SCRIPTING_1_7
                BoltCore.Configuration.controlScheme = CanvasControlScheme.Default;
#else
                BoltCore.Configuration.controlScheme = CanvasControlScheme.Unity;
#endif
            }
            else if (!creating && _connectionModeActive)
            {
                _connectionModeActive = false;
                if (_originalScheme.HasValue)
                {
                    BoltCore.Configuration.controlScheme = _originalScheme.Value;
                    _originalScheme = null;
                }
            }
        }

        private void RegisterGraphEventsIfNeeded(FlowGraph graph, FlowCanvas canvas)
        {
            if (!registeredGraphs.Add(graph))
            {
                return;
            }

            graph.valueConnections.ItemAdded += c =>
            {
                if (Serialization.isSerializing) return;

                var source = c.source;
                var destination = c.destination;

                if (!@event.alt)
                {
                    return;
                }

                // ALT pressed: if we're creating a connection and the connection source or destination matches,
                // then create a hidden ValueReroute and connect to it.
                if (canvas.isCreatingConnection &&
                    (canvas.connectionSource == source || canvas.connectionSource == destination) &&
                    !currentlyCreatingConnection)
                {
                    TryInsertReroute(graph, canvas, source, destination);
                }

                @event.Use();
            };
        }

        private void TryInsertReroute(FlowGraph graph, FlowCanvas canvas, IUnitPort source, IUnitPort destination)
        {
            currentlyCreatingConnection = true;

            var reroute = new ValueReroute() { hideConnection = true, SnapToGrid = BoltCore.Configuration.snapToGrid };
            graph.units.Add(reroute);

            var destinationWidget = canvas.Widget(destination);
            var portPosition = destinationWidget.position.position;
            reroute.position = portPosition - new Vector2(130, 4);

            // If the destination widget's position seems uninitialized (0,0) we need to Cache and reposition reroute
            if (Mathf.Approximately(portPosition.x, 0) && Mathf.Approximately(portPosition.y, 0))
            {
                canvas.Cache();
                var rerouteWidget = canvas.Widget(reroute);
                rerouteWidget.Reposition();
                reroute.position = destinationWidget.position.position - new Vector2(130, 4);
            }

            source.ValidlyConnectTo(reroute.input);
            destination.ValidlyConnectTo(reroute.output);

            currentlyCreatingConnection = false;
        }

        private bool HandlePanKeysWhenNoSelection(FlowGraph graph, FlowCanvas canvas)
        {
            switch (@event.keyCode)
            {
                case KeyCode.LeftArrow:
                    graph.pan += new Vector2(-20, 0);
                    canvas.UpdateViewport();
                    return true;
                case KeyCode.RightArrow:
                    graph.pan += new Vector2(20, 0);
                    canvas.UpdateViewport();
                    return true;
                case KeyCode.UpArrow:
                    graph.pan += new Vector2(0, -20);
                    canvas.UpdateViewport();
                    return true;
                case KeyCode.DownArrow:
                    graph.pan += new Vector2(0, 20);
                    canvas.UpdateViewport();
                    return true;
                default:
                    return false;
            }
        }

        private void HandleCtrlTab(FlowGraph graph, FlowCanvas canvas)
        {
            if (canvas.selection.Count > 0 && selectedElement != canvas.selection.FirstOrDefault())
            {
                visitedElements.Clear();
                selectedElement = null;
            }

            if (EditorApplication.timeSinceStartup - lastTabTime < 0.2f)
            {
                return;
            }
            lastTabTime = EditorApplication.timeSinceStartup;

            if (canvas.isCreatingConnection && canvas.connectionSource.hasValidConnection)
            {
                FollowConnectionAndSelect(canvas);
                return;
            }

            canvas.CancelConnection();

            var allElements = graph.elements
                                   .Except(graph.valueConnections)
                                   .Except(graph.controlConnections)
                                   .Except(graph.invalidConnections)
                                   .OrderBy(e => GetElementX(e))
                                   .ToList();

            if (allElements.Count == 0)
            {
                return;
            }

            var lastSelected = canvas.selection.LastOrDefault();
            List<IGraphElement> cycleElements = BuildCycleElements(allElements, lastSelected);

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

        private void FollowConnectionAndSelect(FlowCanvas canvas)
        {
            var source = canvas.connectionSource;

            if (source is ControlOutput controlOutput)
            {
                canvas.selection.Select(controlOutput.connection.destination.unit);
                GraphUtility.OverrideContextIfNeeded(() => canvas.ViewElements(canvas.selection));
                canvas.connectionSource = controlOutput.connection.destination;
            }
            else if (source is ValueInput valueInput)
            {
                canvas.selection.Select(valueInput.connection.source.unit);
                GraphUtility.OverrideContextIfNeeded(() => canvas.ViewElements(canvas.selection));
                canvas.connectionSource = valueInput.connection.source;
            }
            else if (source is ValueOutput valueOutput)
            {
                canvas.selection.Select(valueOutput.connections.FirstOrDefault()?.destination.unit);
                GraphUtility.OverrideContextIfNeeded(() => canvas.ViewElements(canvas.selection));
                canvas.connectionSource = valueOutput.connections.FirstOrDefault()?.destination;
            }
            else if (source is ControlInput controlInput)
            {
                canvas.selection.Select(controlInput.connections.FirstOrDefault()?.source.unit);
                GraphUtility.OverrideContextIfNeeded(() => canvas.ViewElements(canvas.selection));
                canvas.connectionSource = controlInput.connections.FirstOrDefault()?.source;
            }
        }

        private List<IGraphElement> BuildCycleElements(List<IGraphElement> allElements, IGraphElement lastSelected)
        {
            if (lastSelected is Unit lastUnit && lastUnit.valueInputs.Count > 0)
            {
                var cycleElements = lastUnit.valueInputs
                    .Where(input => input.hasValidConnection)
                    .Select(input => input.connection.source.unit)
                    .Where(u => u != null)
                    .Cast<IGraphElement>()
                    .Where(e => !visitedElements.Contains(e))
                    .ToList();

                if (cycleElements.Count > 0)
                {
                    return cycleElements;
                }
            }

            return allElements.Where(e => !visitedElements.Contains(e)).ToList();
        }

        private void HandleArrowKeyActions(FlowGraph graph, FlowCanvas canvas)
        {
            if (@event.CtrlOrCmd() && !@event.shift)
            {
                canvas.CancelConnection();
                var visited = new HashSet<IGraphElement>();

                var delta = Vector2.zero;
                switch (@event.keyCode)
                {
                    case KeyCode.LeftArrow: delta = new Vector2(-20, 0); break;
                    case KeyCode.RightArrow: delta = new Vector2(20, 0); break;
                    case KeyCode.UpArrow: delta = new Vector2(0, -20); break;
                    case KeyCode.DownArrow: delta = new Vector2(0, 20); break;
                }

                foreach (var element in canvas.selection)
                {
                    MoveElement(element, delta, canvas, visited);
                }
                return;
            }

            var units = canvas.selection.Where(e => e is Unit).OrderBy(u => (u as Unit).position.x);
            if (!units.Any())
            {
                return;
            }

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

            if (@event.keyCode == KeyCode.UpArrow && @event.type == EventType.KeyDown && canvas.isCreatingConnection)
            {
                if (FuzzyWindow.instance == null)
                {
                    GraphUtility.OverrideContextIfNeeded(() => canvas.NewUnitContextual());
                }
            }
        }

        private void MoveElement(IGraphElement element, Vector2 delta, FlowCanvas canvas, HashSet<IGraphElement> visited)
        {
            if (element == null || !visited.Add(element))
            {
                return;
            }

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
            {
                note.position = new Rect(note.position.position + delta, note.position.size);
            }
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

        private float GetElementX(IGraphElement element)
        {
            if (element is Unit unit) return unit.position.x;
            if (element is GraphGroup graphGroup) return graphGroup.position.position.x;
#if VISUAL_SCRIPTING_1_8_0_OR_GREATER
            if (element is StickyNote note) return note.position.position.x;
#endif
            return 0f;
        }

        private bool IsArrowKey(KeyCode keyCode)
        {
            return keyCode == KeyCode.LeftArrow || keyCode == KeyCode.RightArrow;
        }

        private bool IsAnyArrowKey(KeyCode keyCode)
        {
            return keyCode == KeyCode.LeftArrow || keyCode == KeyCode.RightArrow ||
                   keyCode == KeyCode.UpArrow || keyCode == KeyCode.DownArrow;
        }
    }
}