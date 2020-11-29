using Bolt.Addons.Community.Fundamentals.Units.logic;
using Bolt.Addons.Community.Utility;
using Ludiq;
using System;

namespace Bolt.Addons.Community.Variables.Editor.UnitOptions
{
    [FuzzyOption(typeof(ActionUnit))]
    public class ActionUnitOption : UnitOption<ActionUnit>
    {
        [Obsolete()]
        public ActionUnitOption() : base() { }

        public ActionUnitOption(ActionUnit unit) : base(unit)
        {
        }

        protected override string Label(bool human)
        {
            return $"{LudiqGUIUtility.DimString("Action")} { unit._delegate?.GetType().Name }";
        }

        protected override UnitCategory Category()
        {
            return new UnitCategory(base.Category().fullName + "/Actions");
        }
    }

    [FuzzyOption(typeof(BindDelegateUnit))]
    public class BindActionDelegateUnitOption : UnitOption<BindDelegateUnit>
    {
        [Obsolete()]
        public BindActionDelegateUnitOption() : base() { }

        public BindActionDelegateUnitOption(BindDelegateUnit unit) : base(unit)
        {
        }

        protected override string Label(bool human)
        {
            return $"{LudiqGUIUtility.DimString("Bind")} { unit._delegate?.GetType().Name }";
        }

        protected override UnitCategory Category()
        {
            return new UnitCategory(base.Category().fullName + "/Actions");
        }
    }

    [FuzzyOption(typeof(BindDelegateUnit))]
    public class BindFuncDelegateUnitOption : UnitOption<BindDelegateUnit>
    {
        [Obsolete()]
        public BindFuncDelegateUnitOption() : base() { }

        public BindFuncDelegateUnitOption(BindDelegateUnit unit) : base(unit)
        {
        }

        protected override string Label(bool human)
        {
            return $"{LudiqGUIUtility.DimString("Bind")} { unit._delegate?.GetType().Name }";
        }

        protected override UnitCategory Category()
        {
            return new UnitCategory(base.Category().fullName + "/Funcs");
        }
    }
}