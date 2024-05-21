using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Unit))]
    public class NodeGenerator : Decorator<NodeGenerator, NodeGeneratorAttribute, Unit>
    {
        public Unit unit;

        public bool hasNamespace;

        public string NameSpace = "";

        public string UniqueID = "";

        #region Subgraphs
        public List<ControlOutput> connectedGraphOutputs = new List<ControlOutput>();
        public List<ValueInput> connectedValueInputs = new List<ValueInput>();
        #endregion

        public NodeGenerator(Unit unit) { this.unit = unit; }

        public virtual string GenerateValue(ValueInput input) { return $"/* Port '{input.key}' of '{input.unit.GetType().Name}' Missing Generator. */"; }

        public virtual string GenerateValue(ValueOutput output) { return $"/* Port '{output.key}' of '{output.unit.GetType().Name}' Missing Generator. */"; }

        public virtual string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;
            return CodeBuilder.Indent(indent) + $"/* Port '{input.key}' of '{input.unit.GetType().Name}' Missing Generator. */\n" + output;
        }

        public bool ShouldCast(ValueInput input)
        {
            if (input.hasValidConnection)
            {
                return input.connection.source.type != input.type && input.connection.source.type == typeof(object) && input.type != typeof(object);
            }

            return false;
        }
    }

    public class NodeGenerator<TUnit> : NodeGenerator where TUnit : Unit
    {
        public TUnit Unit;

        public NodeGenerator(Unit unit) : base(unit) { this.unit = unit; Unit = (TUnit)unit; }
    }
}