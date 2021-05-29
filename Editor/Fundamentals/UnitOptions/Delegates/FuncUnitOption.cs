using Bolt.Addons.Community.Fundamentals.Units.logic;
using Unity.VisualScripting;
using System;

namespace Bolt.Addons.Community.Variables.Editor.UnitOptions
{
    [FuzzyOption(typeof(FuncUnit))]
    public sealed class FuncUnitOption : UnitOption<FuncUnit>
    {
        [Obsolete()]
        public FuncUnitOption() : base() { }

        public FuncUnitOption(FuncUnit unit) : base(unit)
        {
        }

        protected override string Label(bool human)
        {
            return $"{LudiqGUIUtility.DimString("Func")} { unit._delegate?.GetType().Name }";
        }

        protected override UnitCategory Category()
        {
            return new UnitCategory(base.Category().fullName + "/Funcs");
        }
    }
}