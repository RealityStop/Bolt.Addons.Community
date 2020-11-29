using Bolt.Addons.Community.Fundamentals.Units.logic;
using Ludiq;
using System;

namespace Bolt.Addons.Community.Variables.Editor.UnitOptions
{
    [FuzzyOption(typeof(FuncInvokeUnit))]
    public sealed class FuncInvokeUnitOption : UnitOption<FuncInvokeUnit>
    {
        [Obsolete()]
        public FuncInvokeUnitOption() : base() { }

        public FuncInvokeUnitOption(FuncInvokeUnit unit) : base(unit)
        {
        }

        protected override string Label(bool human)
        {
            return $"{LudiqGUIUtility.DimString("Invoke")} { unit._func?.GetType().Name }";
        }

        protected override UnitCategory Category()
        {
            return new UnitCategory(base.Category().fullName + "/Funcs");
        }
    }
}