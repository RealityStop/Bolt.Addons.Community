using System;
using UnityEngine;

namespace Unity.VisualScripting.Community.Deprecated
{
    /// <summary>
    /// Branches flow by checking if a condition is true or false.
    /// </summary>
    [UnitCategory("Community\\Control")]
    [RenamedFrom("Bolt.Addons.Community.Logic.Units.BranchGreater")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.BranchGreater")]
    [UnitOrder(1)]
    [Obsolete("Use the Branch (Param) node instead!  It supersede this one!")]
    public sealed class BranchGreater : ComparisonBranch
    {
        public BranchGreater() : base() { }

        /// <summary>
        /// Whether to consider equals as true.
        /// </summary>
        [DoNotSerialize]
        public ValueInput AllowEquals { get; private set; }

        protected override void Definition()
        {
            a = ValueInput<float>(nameof(a));
            b = ValueInput<float>(nameof(b), 0);
            AllowEquals = ValueInput<bool>(nameof(AllowEquals), false);

            base.Definition();

            Requirement(AllowEquals, enter);
            Requirement(AllowEquals, enter);
        }

        public override bool Comparison(Flow flow)
        {
            return NumericComparison(flow.GetValue<float>(a), flow.GetValue<float>(b), flow.GetValue<bool>(AllowEquals));
        }

        private bool NumericComparison(float a, float b, bool allowEquals)
        {
            return (a > b) || (allowEquals && Mathf.Approximately(a, b));
        }
    }
}