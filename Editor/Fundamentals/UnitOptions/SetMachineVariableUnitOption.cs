using Bolt.Addons.Community.Fundamentals;

using System;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Variables.Editor.UnitOptions
{
    [FuzzyOption(typeof(SetMachineVariableUnit))]
    public class SetMachineVariableUnitOption : UnitOption<SetMachineVariableUnit>
    {
        private string name;
        private ScriptGraphAsset asset;

        [Obsolete(Serialization.ConstructorWarning)]
        public SetMachineVariableUnitOption() : base() { }

        public SetMachineVariableUnitOption(SetMachineVariableUnit unit) : base(unit)
        {
        }

        protected override UnitCategory Category()
        {
            if (unit.asset == null) return base.Category();
            return new UnitCategory($"Community/Graphs/{unit.asset?.name}");
        }

        protected override string Label(bool human)
        {
            if (unit.asset == null) return base.Label(human);
            return $"{LudiqGUIUtility.DimString("Set")} {unit.defaultName}";
        }
    }
}