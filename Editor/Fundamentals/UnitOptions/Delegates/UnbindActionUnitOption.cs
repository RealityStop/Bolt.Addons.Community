using Bolt.Addons.Community.Fundamentals.Units.logic;
using Bolt.Addons.Community.Utility;
using System;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Variables.Editor.UnitOptions
{
    [FuzzyOption(typeof(UnbindActionUnit))]
    public sealed class UnbindActionUnitOption : UnbindDelegateUnitOption<UnbindActionUnit, IAction>
    {
        [Obsolete()]
        public UnbindActionUnitOption() : base() { }

        protected override string Label(bool human)
        {
            if (unit._delegate == null) return "Unbind Action";
            return base.Label(human);
        }

        public UnbindActionUnitOption(UnbindActionUnit unit) : base(unit)
        {
        }
    }
}