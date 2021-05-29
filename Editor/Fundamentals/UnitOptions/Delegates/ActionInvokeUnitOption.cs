using Bolt.Addons.Community.Fundamentals.Units.logic;
using Unity.VisualScripting;
using System;

namespace Bolt.Addons.Community.Variables.Editor.UnitOptions
{
    [FuzzyOption(typeof(ActionInvokeUnit))]
    public sealed class ActionInvokeUnitOption : UnitOption<ActionInvokeUnit>
    {
        [Obsolete()]
        public ActionInvokeUnitOption() : base() { }

        public ActionInvokeUnitOption(ActionInvokeUnit unit) : base(unit)
        {
        }

        protected override string Label(bool human)
        {
            return $"{LudiqGUIUtility.DimString("Invoke")} { unit._action?.GetType().Name }";
        }

        protected override UnitCategory Category()
        {
            return new UnitCategory(base.Category().fullName + "/Actions");
        }
    }
}