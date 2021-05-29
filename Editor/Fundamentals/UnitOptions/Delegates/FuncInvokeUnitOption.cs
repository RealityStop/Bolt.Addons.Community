using Bolt.Addons.Community.Fundamentals.Units.logic;
using Unity.VisualScripting;
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
            return $"{LudiqGUIUtility.DimString("Invoke")} { unit._func?.DisplayName }";
        }

        protected override UnitCategory Category()
        {
            return new UnitCategory(base.Category().fullName + "/Funcs");
        }
    }
}