using Bolt.Addons.Community.Fundamentals.Units.logic;
using Bolt.Addons.Community.Utility;
using System;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Variables.Editor.UnitOptions
{
    [FuzzyOption(typeof(UnbindFuncUnit))]
    public sealed class UnbindFuncUnitOption : UnbindDelegateUnitOption<UnbindFuncUnit, IFunc>
    {
        [Obsolete()]
        public UnbindFuncUnitOption() : base() { }

        protected override string Label(bool human)
        {
            if (unit._delegate == null) return "Unbind Func";
            return base.Label(human);
        }

        public UnbindFuncUnitOption(UnbindFuncUnit unit) : base(unit)
        {
        }
    }
}