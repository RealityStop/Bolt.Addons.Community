using Unity.VisualScripting.Community.Libraries.CSharp;
using System;

namespace Unity.VisualScripting.Community
{
    [FuzzyOption(typeof(ActionNode))]
    public sealed class ActionNodeOption : UnitOption<ActionNode>
    {
        [Obsolete()]
        public ActionNodeOption() : base() { }

        public ActionNodeOption(ActionNode unit) : base(unit)
        {
        }

        protected override string Label(bool human)
        {
            if (unit._delegate == null) return "Create Action";
            return $"{LudiqGUIUtility.DimString("Action")} { unit._delegate?.DisplayName }";
        }

        protected override UnitCategory Category()
        {
            var @namespace = unit._delegate == null ? string.Empty : unit._delegate.GetDelegateType().Namespace;
            @namespace = (string.IsNullOrEmpty(@namespace) ? string.Empty : @namespace).PeriodsToSlashes();
            return new UnitCategory(base.Category().fullName + (unit._delegate == null ? string.Empty : "/" + @namespace));
        }
    }
}