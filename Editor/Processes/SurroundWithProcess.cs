using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class SurroundWithProcess : GraphProcess<FlowGraph, FlowCanvas>
    {
        public override void Process(FlowGraph graph, FlowCanvas canvas)
        {
            if (@event != null && @event.keyCode == KeyCode.Tab && @event.CtrlOrCmd() && canvas.selection.Count > 0 && !canvas.isCreatingConnection)
            {
                var sourceOutputs = new List<ControlOutput>();
                var sourceUnits = graph.units.Where(unit => !canvas.selection.Contains(unit) && unit.controlOutputs.Any(output =>
                {
                    if (output.hasValidConnection && canvas.selection.Contains(output.connection.destination.unit))
                    {
                        sourceOutputs.Add(output);
                        return true;
                    }
                    return false;
                })).ToList();

                // Get the destination unit that all sources connect to
                var firstDestination = sourceOutputs.FirstOrDefault()?.connection.destination.unit as Unit;

                // Check if all sources connect to the same destination unit
                bool allSourcesConnectToSame = sourceOutputs.All(output =>
                        output.hasValidConnection &&
                        output.connection.destination.unit == firstDestination);

                if (sourceUnits.Count > 1 && !allSourcesConnectToSame)
                {
                    Debug.LogError("Cannot surround different flows!");
                    return;
                }
                if (sourceOutputs.Count > 0)
                {
                    var firstUnit = firstDestination;
                    var lastUnit = (Unit)graph.units.First(unit => canvas.selection.Contains(unit) && (!unit.controlOutputs.Any(output => output.hasValidConnection) || unit.controlOutputs.Any(output => output.hasValidConnection && !canvas.selection.Contains(output.connection.destination.unit))));
                    List<Unit> exitUnits = new List<Unit>();
                    List<Unit> surroundUnits = new List<Unit>();

                    SurroundWithWindow.ShowWindow((surroundCommand) =>
                    {
                        if (lastUnit.controlOutputs.Any(output => output.hasValidConnection))
                        {
                            foreach (var controlOutput in lastUnit.controlOutputs)
                            {
                                AddUnitsRecursive(controlOutput, exitUnits);
                            }
                        }
                        graph.units.Add(surroundCommand.SurroundUnit);

                        if (surroundCommand.SequenceExit)
                        {
                            graph.units.Add(surroundCommand.sequenceUnit);
                        }
                        var firstInput = firstUnit?.controlInputs?.FirstOrDefault(input => input.hasValidConnection);
                        firstInput?.Disconnect();
                        surroundCommand.surroundSource.ConnectToValid(firstInput);
                        foreach (var output in lastUnit.controlOutputs)
                        {
                            if (output.hasValidConnection)
                                output.Disconnect();
                        }
                        AddUnitsRecursive(surroundCommand.surroundSource, surroundUnits);

                        if (surroundCommand.SequenceExit)
                        {
                            foreach (var sourceOutput in sourceOutputs)
                            {
                                sourceOutput.ConnectToValid(surroundCommand.sequenceUnit.enter);
                            }
                            surroundCommand.sequenceUnit.multiOutputs[0].ConnectToValid(surroundCommand.unitEnterPort);
                            surroundCommand.SurroundUnit.position = new Vector2(firstUnit.position.x + 150, firstUnit.position.y);
                            surroundCommand.sequenceUnit.position = new Vector2(surroundCommand.SurroundUnit.position.x - 150, surroundCommand.SurroundUnit.position.y);
                        }
                        else
                        {
                            foreach (var sourceOutput in sourceOutputs)
                            {
                                sourceOutput.ConnectToValid(surroundCommand.unitEnterPort);
                            }
                            surroundCommand.SurroundUnit.position = new Vector2(firstUnit.position.x + 150, firstUnit.position.y);
                        }
                        Vector2 exitStartPosition = new Vector2();
                        if (surroundUnits.Count > 0)
                        {
                            var referencePostion = surroundUnits[0].position;
                            foreach (var unit in surroundUnits)
                            {
                                var offset = new Vector2(surroundCommand.SurroundUnit.position.x + 200, surroundCommand.SurroundUnit.position.y) - referencePostion;
                                if (unit != surroundCommand.SurroundUnit)
                                    unit.position += offset;
                                exitStartPosition = unit.position;
                            }
                        }
                        if (exitUnits.Count > 0)
                        {
                            exitUnits[0].controlInputs.First().ConnectToValid(surroundCommand.surroundExit);
                            var referencePostion = exitUnits[0].position;
                            foreach (var unit in exitUnits)
                            {
                                bool sourceIsAboveExit = surroundCommand.SurroundUnit.controlOutputs.ToList().IndexOf(surroundCommand.surroundSource) < surroundCommand.SurroundUnit.controlOutputs.ToList().IndexOf(surroundCommand.surroundExit) || surroundCommand.SequenceExit;
                                var offset = new Vector2(exitStartPosition.x, sourceIsAboveExit ? exitStartPosition.y + 170 : exitStartPosition.y - 170) - referencePostion;
                                unit.position += offset;
                                exitStartPosition.x += 150;
                            }
                        }
                        if (surroundCommand.autoConnectPort != null)
                        {
                            canvas.connectionSource = surroundCommand.autoConnectPort;
                            LudiqGraphsEditorUtility.editedContext.BeginOverride(GraphWindow.active.context);
                            canvas.NewUnitContextual();
                            LudiqGraphsEditorUtility.editedContext.EndOverride();
                        }
                    });
                }
                else
                {
                    Debug.LogWarning("No source unit to connect to!");
                }
            }
        }

        private void AddUnitsRecursive(ControlOutput output, List<Unit> units)
        {
            if (output.hasValidConnection)
            {
                var destination = output.connection.destination.unit as Unit;
                if (!units.Contains(destination))
                {
                    units.Add(destination);
                    foreach (var controlOutput in destination.controlOutputs)
                    {
                        AddUnitsRecursive(controlOutput, units);
                    }

                    foreach (var valueInput in destination.valueInputs)
                    {
                        AddValueUnitsRecursive(valueInput, units);
                    }
                }
            }
        }

        private void AddValueUnitsRecursive(ValueInput valueInput, List<Unit> units)
        {
            if (valueInput.hasValidConnection)
            {
                var source = valueInput.connection.source.unit as Unit;
                if (!units.Contains(source))
                {
                    units.Add(source);
                    foreach (var _valueInput in source.valueInputs)
                    {
                        AddValueUnitsRecursive(_valueInput, units);
                    }
                }
            }
        }
    }
}