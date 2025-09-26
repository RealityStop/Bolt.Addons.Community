using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class UnitPreservationContext<TSourceUnit> where TSourceUnit : Unit
    {
        protected Unit unitToPreserveFrom;
        public Unit unitToRestoreTo { get; protected set; }
        protected FlowGraph graph;

        protected static HashSet<Unit> processedUnits = new HashSet<Unit>();

        protected List<UnitPreservationContext<TSourceUnit>> connectedPreservation = new List<UnitPreservationContext<TSourceUnit>>();
        protected Dictionary<string, object> defaultValues = new Dictionary<string, object>();

        public IUnitPort sourcePort;
        public string initialPortKey = "";

        public List<Unit> addedUnits = new List<Unit>();

        // Enum for port types
        protected enum PortType { ControlOutput, ValueInput, ControlInput, ValueOutput }

        protected List<((Unit unit, string key, PortType portType) target, (UnitPreservationContext<TSourceUnit> unit, string key) result)> connectedPorts =
        new List<((Unit unit, string key, PortType portType) target, (UnitPreservationContext<TSourceUnit> unit, string key) result)>();

        // Constructor
        public UnitPreservationContext(Unit unitToPreserveFrom, FlowGraph graph)
        {
            this.unitToPreserveFrom = unitToPreserveFrom;
            unitToRestoreTo = unitToPreserveFrom.CloneViaFakeSerialization();
            unitToRestoreTo.Define();
            this.graph = graph;
        }

        protected void Create()
        {
            StoreDefaultValues();
            InitializeInitialPortKey();
            ProcessControlOutputs();
            ProcessValueInputs();
        }

        // Store default values from the unit
        private void StoreDefaultValues()
        {
            foreach (var defaultValue in unitToPreserveFrom.defaultValues)
            {
                defaultValues[defaultValue.Key] = defaultValue.Value;
            }
        }

        protected virtual void ProcessValueInputs() { }
        protected virtual void ProcessControlOutputs() { }
        protected virtual void InitializeInitialPortKey() { }

        /// <summary>
        /// Add the preserved units to the graph
        /// </summary>
        public void AddToGraph(Vector2 offsetPosition)
        {
            if (!graph.units.Any(u => u.guid == unitToRestoreTo.guid))
            {
                Reset();
                graph.units.Add(unitToRestoreTo);
                unitToRestoreTo.graph = graph;
                if (!addedUnits.Contains(unitToRestoreTo))
                {
                    addedUnits.Add(unitToRestoreTo);
                }
            }
            else
            {
                unitToRestoreTo = (Unit)graph.units.First(u => u.guid == unitToRestoreTo.guid);
                if (!addedUnits.Contains(unitToRestoreTo))
                {
                    addedUnits.Add(unitToRestoreTo);
                }
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
            if (sourcePort is ControlOutput controlOutput && unitToRestoreTo.controlInputs.Any(c => c.key == initialPortKey))
            {
                unitToRestoreTo.controlInputs[initialPortKey].ConnectToValid(controlOutput);
            }
            else if (sourcePort is ValueInput valueInput && unitToRestoreTo.valueOutputs.Any(o => o.key == initialPortKey))
            {
                unitToRestoreTo.valueOutputs[initialPortKey].ConnectToValid(valueInput);
            }
            else if (sourcePort is ValueOutput valueOutput && unitToRestoreTo.valueInputs.Any(o => o.key == initialPortKey))
            {
                unitToRestoreTo.valueInputs[initialPortKey].ConnectToValid(valueOutput);
            }
            else if (sourcePort is ControlInput controlInput && unitToRestoreTo.controlOutputs.Any(o => o.key == initialPortKey))
            {
                unitToRestoreTo.controlOutputs[initialPortKey].ConnectToValid(controlInput);
            }
        }

        // Handle connecting ports between units
        private void ConnectPort((Unit unit, string key, PortType portType) target, (UnitPreservationContext<TSourceUnit> unit, string key) result)
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
                case PortType.ValueOutput:
                    targetUnit.valueOutputs[target.key].ValidlyConnectTo(result.unit.unitToRestoreTo.valueInputs[result.key]);
                    break;
                case PortType.ControlInput:
                    targetUnit.controlInputs[target.key].ValidlyConnectTo(result.unit.unitToRestoreTo.controlOutputs[result.key]);
                    break;
            }
        }

        /// <summary>
        /// Reset the GUIDs to allow re-adding to the graph
        /// </summary>
        public void Reset()
        {
            unitToRestoreTo.guid = Guid.NewGuid();

            foreach (var preservation in connectedPreservation)
            {
                preservation.Reset();
            }
        }
    }
}