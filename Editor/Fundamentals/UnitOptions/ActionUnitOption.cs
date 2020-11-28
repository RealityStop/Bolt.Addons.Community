﻿using Bolt.Addons.Community.Fundamentals.Units.logic;
using Bolt.Addons.Community.Utility;
using Ludiq;
using System;

namespace Bolt.Addons.Community.Variables.Editor.UnitOptions
{
    [FuzzyOption(typeof(ActionUnit))]
    public class ActionUnitOption : UnitOption<ActionUnit>
    {
        [Obsolete()]
        public ActionUnitOption() : base() { }

        public ActionUnitOption(ActionUnit unit) : base(unit)
        {
        }

        protected override string Label(bool human)
        {
            return $"{LudiqGUIUtility.DimString("Action")} { unit._action?.GetType().Name }";
        }

        protected override UnitCategory Category()
        {
            return new UnitCategory(base.Category().fullName + "/Actions");
        }
    }
}