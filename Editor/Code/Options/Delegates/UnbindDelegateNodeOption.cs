using Unity.VisualScripting.Community.Libraries.CSharp;
using System;

namespace Unity.VisualScripting.Community
{
    public abstract class UnbindDelegateNodeOption<TUnbindDelegateUnit, TDelegateInterface> : UnitOption<TUnbindDelegateUnit>
       where TUnbindDelegateUnit : UnbindDelegateNode<TDelegateInterface>
       where TDelegateInterface : IDelegate
    {
        [Obsolete()]
        public UnbindDelegateNodeOption() : base() { }

        public UnbindDelegateNodeOption(TUnbindDelegateUnit unit) : base(unit)
        {
        }

        protected override string Label(bool human)
        {
            return $"{LudiqGUIUtility.DimString("Unbind")} { unit._delegate?.DisplayName }";
        }

        protected override UnitCategory Category()
        {
            var @namespace = unit._delegate == null ? string.Empty : unit._delegate.GetDelegateType().Namespace;
            @namespace = (string.IsNullOrEmpty(@namespace) ? string.Empty : @namespace).PeriodsToSlashes();
            return new UnitCategory(base.Category().fullName + (unit._delegate == null ? string.Empty : "/" + @namespace));
        }
    }
}