using Unity.VisualScripting.Community.Libraries.CSharp;
using System;

namespace Unity.VisualScripting.Community
{
    [FuzzyOption(typeof(FuncNode))]
    public sealed class FuncNodeOption : UnitOption<FuncNode>
    {
        [Obsolete()]
        public FuncNodeOption() : base() { }

        public FuncNodeOption(FuncNode unit) : base(unit)
        {
        }

        protected override string Label(bool human)
        {
            if (unit._delegate == null) return "Create Func";
            return $"{LudiqGUIUtility.DimString("Func")} { unit._delegate?.DisplayName }";
        }

        protected override UnitCategory Category()
        {
            var @namespace = unit._delegate == null ? string.Empty : unit._delegate.GetDelegateType().Namespace;
            @namespace = (string.IsNullOrEmpty(@namespace) ? string.Empty : @namespace).PeriodsToSlashes();
            return new UnitCategory(base.Category().fullName + (unit._delegate == null ? string.Empty : "/" + @namespace));
        }
    }
}