using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class SurroundWithProcess : GraphProcess<FlowGraph, FlowCanvas>
    {
        public override void Process(FlowGraph graph, FlowCanvas canvas)
        {
            if (@event != null && @event.CtrlOrCmd() && @event.shift && @event.keyCode == KeyCode.T && canvas.selection.Count > 0 && !canvas.isCreatingConnection && SurroundWithWindow.Window == null)
            {
                var sourceOutputs = new List<ControlOutput>();
                var sourceUnits = graph.units.Where(unit => (!canvas.selection.Contains(unit) || (canvas.selection.Contains(unit) && unit.controlInputs.Count == 0)) && unit.controlOutputs.Any(output =>
                {
                    if (output.hasValidConnection && canvas.selection.Contains(output.connection.destination.unit))
                    {
                        sourceOutputs.Add(output);
                        return true;
                    }
                    return false;
                })).ToList();

                var firstDestination = sourceOutputs.FirstOrDefault()?.connection.destination.unit as Unit;

                bool allSourcesConnectToSame = sourceOutputs.All(output =>
                        output.hasValidConnection &&
                        output.connection.destination.unit == firstDestination);

                if (sourceUnits.Count > 1 && !allSourcesConnectToSame)
                {
                    Debug.LogError("Cannot surround different flows!");
                    return;
                }
                if (sourceOutputs.Count == 0)
                {
                    Debug.LogWarning("No source unit to connect to!");
                    return;
                }
                SurroundWithWindow.ShowWindow((surroundCommand) =>
                {
                    ApplySurround(graph, canvas, surroundCommand);
                });
            }
        }

        private void ApplySurround(FlowGraph graph, FlowCanvas canvas, SurroundCommand surroundCommand)
        {
            var selection = canvas.selection.OfType<Unit>().ToList();
            if (selection.Count == 0) return;

            // Downstream (selection → outside)
            var downstreamConnections = graph.controlConnections
                .Where(c => selection.Contains(c.source.unit) && !selection.Contains(c.destination.unit))
                .ToList();

            // Incoming (outside → selection)
            var incomingConnections = graph.controlConnections
                .Where(c => !selection.Contains(c.source.unit) && selection.Contains(c.destination.unit))
                .ToList();

            var firstUnit = incomingConnections.FirstOrDefault()?.destination.unit ?? selection.First();
            var lastUnit = downstreamConnections.LastOrDefault()?.source.unit ?? selection.Last();

            var minX = selection.Min(u => u.position.x);
            var minY = selection.Min(u => u.position.y);
            var maxX = selection.Max(u => u.position.x);
            var maxY = selection.Max(u => u.position.y);

            var selectionCenter = new Vector2((minX + maxX) / 2f, (minY + maxY) / 2f);

            var surroundUnit = surroundCommand.SurroundUnit;
            graph.units.Add(surroundUnit);

            const float spacing = 200f;
            if (surroundCommand.SequenceExit)
            {
                var seq = surroundCommand.sequenceUnit;
                graph.units.Add(seq);
                seq.position = new Vector2(minX, selectionCenter.y);
                surroundUnit.position = new Vector2(minX + spacing, selectionCenter.y);
                maxX += spacing;
            }
            else
                surroundUnit.position = new Vector2(minX, selectionCenter.y);

            var offsetX = canvas.Widget(surroundUnit).position.width + spacing + (surroundCommand.SequenceExit ? canvas.Widget(surroundCommand.sequenceUnit).position.width + spacing : 0f);
            foreach (var u in selection)
            {
                u.position += new Vector2(offsetX, 0f);
            }

            if (surroundCommand.SequenceExit)
            {
                var seq = surroundCommand.sequenceUnit;
                foreach (var conn in incomingConnections)
                {
                    conn.source?.ValidlyConnectTo(seq.controlInputs[0]);
                }

                if (seq.controlOutputs.Count > 0 && surroundCommand.unitEnterPort != null)
                {
                    seq.controlOutputs[0].ValidlyConnectTo(surroundCommand.unitEnterPort);
                }

                if (surroundCommand.surroundSource != null && firstUnit.controlInputs.Count > 0)
                {
                    surroundCommand.surroundSource.ValidlyConnectTo(firstUnit.controlInputs[0]);
                }

                foreach (var conn in downstreamConnections)
                {
                    if (conn.destination != null)
                    {
                        conn.destination.unit.position += new Vector2(0, canvas.Widget(lastUnit).position.height);
                        conn.source.Disconnect();
                        seq.controlOutputs[1].ValidlyConnectTo(conn.destination);
                    }
                }
            }
            else
            {
                foreach (var conn in incomingConnections)
                {
                    if (conn.source != null && surroundCommand.unitEnterPort != null)
                    {
                        conn.source.ValidlyConnectTo(surroundCommand.unitEnterPort);
                    }
                }

                if (surroundCommand.surroundSource != null && firstUnit.controlInputs.Count > 0)
                {
                    surroundCommand.surroundSource.ValidlyConnectTo(firstUnit.controlInputs[0]);
                }

                foreach (var conn in downstreamConnections)
                {
                    if (conn.destination != null && surroundCommand.surroundExit != null)
                    {
                        conn.destination.unit.position -= new Vector2(0, canvas.Widget(lastUnit).position.height);
                        conn.source.Disconnect();
                        surroundCommand.surroundExit.ValidlyConnectTo(conn.destination);
                    }
                }
            }

            if (surroundCommand.autoConnectPort != null)
            {
                canvas.connectionSource = surroundCommand.autoConnectPort;
                GraphUtility.AddNewUnitContextual(graph, canvas, (element) =>
                {
                    var unit = element as Unit;
                    if (unit == null) return;

                    if (surroundCommand.autoConnectPort is ValueInput)
                    {
                        var surroundPos = surroundCommand.autoConnectPort.unit.position;
                        unit.position = new Vector2(surroundPos.x - 150, surroundPos.y + 150);
                    }
                    else if (surroundCommand.autoConnectPort is ValueOutput)
                    {
                        var surroundPos = surroundCommand.autoConnectPort.unit.position;
                        unit.position = new Vector2(surroundPos.x + 150, surroundPos.y + 150);
                    }
                    else if (surroundCommand.autoConnectPort is ControlOutput)
                    {
                        var surroundPos = surroundCommand.autoConnectPort.unit.position;
                        unit.position = new Vector2(surroundPos.x + 250, surroundPos.y + 150);
                    }
                    else if (surroundCommand.autoConnectPort is ControlOutput)
                    {
                        var surroundPos = surroundCommand.autoConnectPort.unit.position;
                        unit.position = new Vector2(surroundPos.x - 250, surroundPos.y + 150);
                    }
                });
            }
        }
    }
}