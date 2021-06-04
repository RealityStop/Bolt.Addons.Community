using Bolt.Addons.Community.Fundamentals.Units.logic;
using Bolt.Addons.Community.Utility;
using System;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Variables.Editor.UnitOptions
{
    [FuzzyOption(typeof(BindFuncUnit))]
    public sealed class BindFuncUnitOption : BindDelegateUnitOption<BindFuncUnit, IFunc>
    {
        [Obsolete()]
        public BindFuncUnitOption() : base() { }

        protected override string Label(bool human)
        {
            if (unit._delegate == null) return "Bind Func";
            return base.Label(human);
        }

        public BindFuncUnitOption(BindFuncUnit unit) : base(unit)
        {
        }
    }
}