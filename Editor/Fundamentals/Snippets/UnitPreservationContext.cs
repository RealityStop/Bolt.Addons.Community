using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class UnitPreservationContext
    {
        private Unit unitToPreserveFrom;
        public Unit unitToRestoreTo { get; private set; }
        private FlowGraph graph;

        private static HashSet<Unit> processedUnits = new HashSet<Unit>();

        private List<UnitPreservationContext> connectedPreservation = new();
        private Dictionary<string, object> defaultValues = new();

        public IUnitPort sourcePort;
        public string initialPortKey = "";

        // Enum for port types
        private enum PortType { ControlOutput, ValueInput }

        List<((Unit unit, string key, PortType portType) target, (UnitPreservationContext unit, string key) result)> connectedPorts = new();

        // Constructor
        public UnitPreservationContext(Unit unitToPreserveFrom, FlowGraph graph, SnippetType snippetType)
        {
            this.unitToPreserveFrom = unitToPreserveFrom;
            unitToRestoreTo = unitToPreserveFrom.CloneViaFakeSerialization();
            unitToRestoreTo.Define();
            this.graph = graph;

            InitializeInitialPortKey(snippetType);
            StoreDefaultValues();
            ProcessValueInputs(snippetType);
            ProcessControlOutputs(snippetType);
        }

        // Initialize the port key based on snippet type
        private void InitializeInitialPortKey(SnippetType snippetType)
        {
            if (snippetType == SnippetType.ControlInput)
            {
                initialPortKey = unitToPreserveFrom.controlInputs
                    .FirstOrDefault(c => c.connections.Any(conn => conn.source.unit is SnippetSourceUnit))
                    ?.key ?? "";
            }
            else if (snippetType == SnippetType.ValueInput)
            {
                initialPortKey = unitToPreserveFrom.valueOutputs
                    .FirstOrDefault(c => c.connections.Any(conn => conn.destination.unit is SnippetSourceUnit))
                    ?.key ?? "";
            }
        }

        // Store default values from the unit
        private void StoreDefaultValues()
        {
            foreach (var defaultValue in unitToPreserveFrom.defaultValues)
            {
                defaultValues[defaultValue.Key] = defaultValue.Value;
            }
        }

        // Process value inputs and connect them to preserved units
        private void ProcessValueInputs(SnippetType snippetType)
        {
            foreach (var input in unitToPreserveFrom.valueInputs.Where(i => i.hasValidConnection))
            {
                var connectedUnit = input.connection.source.unit as Unit;
                var preservation = new UnitPreservationContext(connectedUnit, graph, snippetType);

                connectedPreservation.Add(preservation);
                connectedPorts.Add(((unitToRestoreTo, input.key, PortType.ValueInput), (preservation, input.connection.source.key)));
            }
        }

        // Process control outputs and connect them to preserved units
        private void ProcessControlOutputs(SnippetType snippetType)
        {
            foreach (var controlOutput in unitToPreserveFrom.controlOutputs.Where(o => o.hasValidConnection))
            {
                var connectedUnit = controlOutput.connection.destination.unit as Unit;
                if (processedUnits.Contains(connectedUnit)) return;

                processedUnits.Add(connectedUnit);
                var preservation = new UnitPreservationContext(connectedUnit, graph, snippetType);

                connectedPreservation.Add(preservation);
                connectedPorts.Add(((unitToRestoreTo, controlOutput.key, PortType.ControlOutput), (preservation, controlOutput.connection.destination.key)));
            }
        }

        /// <summary>
        /// Add the preserved units to the graph
        /// </summary>
        public void AddToGraph(Vector2 offsetPosition)
        {
            if (!graph.units.Any(u => u.guid == unitToRestoreTo.guid))
            {
                graph.units.Add(unitToRestoreTo);
                unitToRestoreTo.graph = graph;
            }
            else
            {
                unitToRestoreTo = (Unit)graph.units.First(u => u.guid == unitToRestoreTo.guid);
            }

            if (unitToRestoreTo.position != null)
            {
                unitToRestoreTo.position += offsetPosition;
            }

            foreach (var kvp in defaultValues)
            {
                unitToRestoreTo.defaultValues[kvp.Key] = kvp.Value;
            }

            foreach (var preservation in connectedPreservation)
            {
                preservation.AddToGraph(offsetPosition);
            }
        }

        /// <summary>
        /// Connect units and their ports
        /// </summary>
        public void Connect()
        {
            ConnectInitialPort();
            foreach (var (target, result) in connectedPorts)
            {
                ConnectPort(target, result);
            }
            foreach (var preservation in connectedPreservation)
            {
                preservation.Connect();
            }
        }

        // Connect the initial port based on source type
        private void ConnectInitialPort()
        {
            if (sourcePort is ControlOutput controlOutput &&
                unitToRestoreTo.controlInputs.Any(c => c.key == initialPortKey))
            {
                unitToRestoreTo.controlInputs[initialPortKey].ConnectToValid(controlOutput);
            }
            else if (sourcePort is ValueInput valueInput &&
                     unitToRestoreTo.valueOutputs.Any(o => o.key == initialPortKey))
            {
                unitToRestoreTo.valueOutputs[initialPortKey].ConnectToValid(valueInput);
            }
        }

        // Handle connecting ports between units
        private void ConnectPort((Unit unit, string key, PortType portType) target, (UnitPreservationContext unit, string key) result)
        {
            var targetUnit = graph.units.FirstOrDefault(u => u.guid == target.unit.guid) ?? target.unit;

            switch (target.portType)
            {
                case PortType.ValueInput:
                    targetUnit.valueInputs[target.key].ValidlyConnectTo(result.unit.unitToRestoreTo.valueOutputs[result.key]);
                    break;
                case PortType.ControlOutput:
                    targetUnit.controlOutputs[target.key].ValidlyConnectTo(result.unit.unitToRestoreTo.controlInputs[result.key]);
                    break;
            }
        }

        /// <summary>
        /// Reset the GUIDs to allow re-adding to the graph
        /// </summary>
        public void Reset()
        {
            unitToPreserveFrom.guid = Guid.NewGuid();
            unitToRestoreTo.guid = Guid.NewGuid();

            foreach (var preservation in connectedPreservation)
            {
                preservation.Reset();
            }
        }
    }
}