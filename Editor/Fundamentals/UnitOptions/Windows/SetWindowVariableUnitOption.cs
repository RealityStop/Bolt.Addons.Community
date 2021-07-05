using System;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Utility.Editor.UnitOptions
{
    [FuzzyOption(typeof(SetWindowVariableUnit))]
    public class SetWindowVariableUnitOption : UnitOption<SetWindowVariableUnit>
    {
        private string name;
        private EditorWindowAsset asset;

        [Obsolete(Serialization.ConstructorWarning)]
        public SetWindowVariableUnitOption() : base() { }

        public SetWindowVariableUnitOption(SetWindowVariableUnit unit) : base(unit)
        {
        }

        protected override UnitCategory Category()
        {
            if (unit.asset == null) return base.Category();
            return new UnitCategory($"Community/Editor/{unit.asset?.name}");
        }

        protected override string Label(bool human)
        {
            if (unit.asset == null) return base.Label(human);
            return $"{LudiqGUIUtility.DimString("Set")} {unit.defaultName}";
        }
    }
}
