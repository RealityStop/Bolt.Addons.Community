using Bolt.Addons.Community.Fundamentals;

using System;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Variables.Editor.UnitOptions
{
    [FuzzyOption(typeof(GetMachineVariableUnit))]
    public class GetMachineVariableUnitOption : UnitOption<GetMachineVariableUnit>
    {
        [Obsolete(Serialization.ConstructorWarning)]
        public GetMachineVariableUnitOption() : base() { }

        public GetMachineVariableUnitOption(GetMachineVariableUnit unit) : base(unit)
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
            return $"{LudiqGUIUtility.DimString("Get")} {unit.defaultName}";
        }
    }
}