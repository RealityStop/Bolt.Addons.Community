using Bolt.Addons.Community.Utility;
using Bolt.Addons.Libraries.CSharp;
using Unity.VisualScripting;
using System;

namespace Bolt.Addons.Community.Fundamentals.Editor.UnitOptions
{
    public abstract class BindDelegateUnitOption<TBindDelegateUnit, TDelegateInterface> : UnitOption<TBindDelegateUnit>
        where TBindDelegateUnit : BindDelegateUnit<TDelegateInterface>
        where TDelegateInterface : IDelegate
    {
        [Obsolete()]
        public BindDelegateUnitOption() : base() { }

        public BindDelegateUnitOption(TBindDelegateUnit unit) : base(unit)
        {
        }

        protected override string Label(bool human)
        {
            return $"{LudiqGUIUtility.DimString("Bind")} { unit._delegate?.DisplayName }";
        }

        protected override UnitCategory Category()
        {
            var @namespace = unit._delegate == null ? string.Empty : unit._delegate.GetDelegateType().Namespace;
            @namespace = (string.IsNullOrEmpty(@namespace) ? string.Empty : @namespace).PeriodsToSlashes();
            return new UnitCategory(base.Category().fullName + (unit._delegate == null ? string.Empty : "/" + @namespace));
        }
    }
}