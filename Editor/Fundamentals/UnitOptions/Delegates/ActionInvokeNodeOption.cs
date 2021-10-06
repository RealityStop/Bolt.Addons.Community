using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting;
using System;

namespace Unity.VisualScripting.Community
{
    [FuzzyOption(typeof(ActionInvokeNode))]
    public sealed class ActionInvokeNodeOption : UnitOption<ActionInvokeNode>
    {
        [Obsolete()]
        public ActionInvokeNodeOption() : base() { }

        public ActionInvokeNodeOption(ActionInvokeNode unit) : base(unit)
        {

        }

        protected override string Label(bool human)
        {
            if (unit._delegate == null) return "Invoke Action";
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