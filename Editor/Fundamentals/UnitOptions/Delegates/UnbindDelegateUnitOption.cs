using Bolt.Addons.Community.Fundamentals.Units.logic;
using Bolt.Addons.Community.Utility;
using Bolt.Addons.Integrations.Continuum.CSharp;
using Unity.VisualScripting;
using System;

namespace Bolt.Addons.Community.Variables.Editor.UnitOptions
{
    public abstract class UnbindDelegateUnitOption<TUnbindDelegateUnit, TDelegateInterface> : UnitOption<TUnbindDelegateUnit>
       where TUnbindDelegateUnit : UnbindDelegateUnit<TDelegateInterface>
       where TDelegateInterface : IDelegate
    {
        [Obsolete()]
        public UnbindDelegateUnitOption() : base() { }

        public UnbindDelegateUnitOption(TUnbindDelegateUnit unit) : base(unit)
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