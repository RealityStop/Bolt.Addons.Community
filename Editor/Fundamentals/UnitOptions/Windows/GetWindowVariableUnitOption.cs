using Bolt.Addons.Community.Fundamentals.Units.Documenting;
using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Bolt.Addons.Community.Utility.Editor.UnitOptions
{
    [FuzzyOption(typeof(GetWindowVariableUnit))]
    public class GetWindowVariableUnitOption : UnitOption<GetWindowVariableUnit>
    {
        private string name;
        private EditorWindowAsset asset;

        [Obsolete(Serialization.ConstructorWarning)]
        public GetWindowVariableUnitOption() : base() { }

        public GetWindowVariableUnitOption(GetWindowVariableUnit unit) : base(unit)
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
            return $"{LudiqGUIUtility.DimString("Get")} {unit.defaultName}";
        }
    }
}
