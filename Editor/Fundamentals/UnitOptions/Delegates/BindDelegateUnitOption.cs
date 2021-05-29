using Bolt.Addons.Community.Fundamentals.Units.logic;
using Bolt.Addons.Community.Utility;
using Unity.VisualScripting;
using System;

namespace Bolt.Addons.Community.Variables.Editor.UnitOptions
{
    public abstract class BindDelegateUnitOption<TBindDelegateUnit, TDelegateInterface> : UnitOption<TBindDelegateUnit>
        where TBindDelegateUnit : BindDelegateUnit<TDelegateInterface>
        where TDelegateInterface : IDelegate
    {
        [Obsolete()]
        public BindDelegateUnitOption() : base() { }

        protected abstract string subCategory { get; }

        public BindDelegateUnitOption(TBindDelegateUnit unit) : base(unit)
        {
        }

        protected override string Label(bool human)
        {
            return $"{LudiqGUIUtility.DimString("Bind")} { unit._delegate?.GetType().Name }";
        }

        protected override UnitCategory Category()
        {
            return new UnitCategory(base.Category().fullName + "/" + subCategory);
        }
    }
}