
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ludiq;
using UnityEngine;

namespace Bolt.Addons.Community.Fundamentals
{
    /// <summary>
    /// Branches flow by checking if a condition is true or false.
    /// </summary>
    [UnitCategory("Community\\Control")]
    [RenamedFrom("Bolt.Addons.Community.Logic.Units.BranchEqual")]
    [UnitOrder(0)]
    [Obsolete("Use the Branch (Param) node instead!  It supersede this one!")]
    public sealed class BranchEqual : ComparisonBranch
    {
        public BranchEqual() : base() { }


        /// <summary>
        /// Whether the compared inputs are numbers.
        /// </summary>
        [Serialize]
        [Inspectable]
        [InspectorToggleLeft]
        public bool numeric { get; set; } = true;


        protected override void Definition()
        {
            if (numeric)
            {
                a = ValueInput<float>(nameof(a));
                b = ValueInput<float>(nameof(b), 0);
            }
            else
            {
                a = ValueInput<object>(nameof(a)).AllowsNull();
                b = ValueInput<object>(nameof(b)).AllowsNull();
            }

            base.Definition();
        }

        public override bool Comparison(Flow flow)
        {
            if (numeric)
                return NumericComparison(flow.GetValue<float>(a), flow.GetValue<float>(b));
            else
                return GenericComparison(flow.GetValue<object>(a), flow.GetValue<object>(b));
        }

        private bool NumericComparison(float a, float b)
        {
            return Mathf.Approximately(a, b);
        }

        private bool GenericComparison(object a, object b)
        {
            return OperatorUtility.Equal(a, b);
        }
    }
}