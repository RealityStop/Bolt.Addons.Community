using Bolt.Addons.Libraries.Humility;
using Bolt.Addons.Libraries.CSharp;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Generation
{
    [UnitGenerator(typeof(Unit))]
    public class UnitGenerator : Decorator<UnitGenerator, UnitGeneratorAttribute, Unit>
    {
        public Unit unit;

        public UnitGenerator(Unit unit) { this.unit = unit; }

        public virtual string GenerateValue(ValueInput input) { return $"/* Port '{ input.key }' of '{ input.unit.GetType().Name }' Missing Generator. */"; }

        public virtual string GenerateValue(ValueOutput output) { return $"/* Port '{ output.key }' of '{ output.unit.GetType().Name }' Missing Generator. */"; }

        public virtual string GenerateControl(ControlInput input, ControlGenerationData data, int indent) { return CodeBuilder.Indent(indent) + $"/* Port '{ input.key }' of '{ input.unit.GetType().Name }' Missing Generator. */"; }
    }

    public class UnitGenerator<TUnit> : UnitGenerator where TUnit : Unit
    {
        public TUnit Unit;

        public UnitGenerator(Unit unit) : base (unit) { this.unit = unit; Unit = (TUnit)unit; }
    }
}