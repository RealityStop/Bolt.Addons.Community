using Bolt.Addons.Community.Fundamentals.Units.logic;
using Unity.VisualScripting;
using System;

namespace Bolt.Addons.Community.Variables.Editor.UnitOptions
{
    [FuzzyOption(typeof(ActionUnit))]
    public sealed class ActionUnitOption : UnitOption<ActionUnit>
    {
        [Obsolete()]
        public ActionUnitOption() : base() { }

        public ActionUnitOption(ActionUnit unit) : base(unit)
        {
        }

        protected override string Label(bool human)
        {
            return $"{LudiqGUIUtility.DimString("Action")} { unit._delegate?.DisplayName }";
        }

        protected override UnitCategory Category()
        {
            return new UnitCategory(base.Category().fullName + "/Actions");
        }
    }
}