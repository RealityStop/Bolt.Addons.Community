using Unity.VisualScripting.Community.Libraries.CSharp;
using System;

namespace Unity.VisualScripting.Community
{
    public abstract class BindDelegateNodeOption<TBindDelegateUnit, TDelegateInterface> : UnitOption<TBindDelegateUnit>
        where TBindDelegateUnit : BindDelegateNode<TDelegateInterface>
        where TDelegateInterface : IDelegate
    {
        [Obsolete()]
        public BindDelegateNodeOption() : base() { }

        public BindDelegateNodeOption(TBindDelegateUnit unit) : base(unit)
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