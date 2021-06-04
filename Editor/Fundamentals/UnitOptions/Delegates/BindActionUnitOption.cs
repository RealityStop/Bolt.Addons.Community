using Bolt.Addons.Community.Fundamentals.Units.logic;
using Bolt.Addons.Community.Utility;
using System;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Variables.Editor.UnitOptions
{
    [FuzzyOption(typeof(BindActionUnit))]
    public sealed class BindActionUnitOption : BindDelegateUnitOption<BindActionUnit, IAction>
    {
        [Obsolete()]
        public BindActionUnitOption() : base() { }

        protected override string Label(bool human)
        {
            if (unit._delegate == null) return "Bind Action";
            return base.Label(human);
        }

        public BindActionUnitOption(BindActionUnit unit) : base(unit)
        {
        }
    }
}