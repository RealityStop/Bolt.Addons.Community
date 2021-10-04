using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Unit))]
    public class NodeGenerator : Decorator<NodeGenerator, NodeGeneratorAttribute, Unit>
    {
        public Unit unit;

        public NodeGenerator(Unit unit) { this.unit = unit; }

        public virtual string GenerateValue(ValueInput input) { return $"/* Port '{ input.key }' of '{ input.unit.GetType().Name }' Missing Generator. */"; }

        public virtual string GenerateValue(ValueOutput output) { return $"/* Port '{ output.key }' of '{ output.unit.GetType().Name }' Missing Generator. */"; }

        public virtual string GenerateControl(ControlInput input, ControlGenerationData data, int indent) { return CodeBuilder.Indent(indent) + $"/* Port '{ input.key }' of '{ input.unit.GetType().Name }' Missing Generator. */"; }
    }

    public class NodeGenerator<TUnit> : NodeGenerator where TUnit : Unit
    {
        public TUnit Unit;

        public NodeGenerator(Unit unit) : base (unit) { this.unit = unit; Unit = (TUnit)unit; }
    }
}