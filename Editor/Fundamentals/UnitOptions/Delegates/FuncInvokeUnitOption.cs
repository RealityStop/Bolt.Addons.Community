using Bolt.Addons.Community.Fundamentals.Units.logic;
using Bolt.Addons.Integrations.Continuum.CSharp;
using Unity.VisualScripting;
using System;

namespace Bolt.Addons.Community.Variables.Editor.UnitOptions
{
    [FuzzyOption(typeof(FuncInvokeUnit))]
    public sealed class FuncInvokeUnitOption : UnitOption<FuncInvokeUnit>
    {
        [Obsolete()]
        public FuncInvokeUnitOption() : base() { }

        public FuncInvokeUnitOption(FuncInvokeUnit unit) : base(unit)
        {
        }

        protected override string Label(bool human)
        {
            if (unit._delegate == null) return "Invoke Func";
            return $"{LudiqGUIUtility.DimString("Invoke")} { unit._delegate?.DisplayName }";
        }

        protected override UnitCategory Category()
        {
            var @namespace = unit._delegate == null ? string.Empty : unit._delegate.GetDelegateType().Namespace;
            @namespace = (string.IsNullOrEmpty(@namespace) ? string.Empty : @namespace).PeriodsToSlashes();
            return new UnitCategory(base.Category().fullName + (unit._delegate == null ? string.Empty : "/" + @namespace));
        }
    }
}