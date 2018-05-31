
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ludiq;
using UnityEngine;

namespace Bolt.Addons.Community.Logic.Units
{
    /// <summary>
    /// Branches flow by checking if a condition is true or false.
    /// </summary>
    [UnitCategory("Community\\Control")]
    [UnitOrder(0)]
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

        public override bool Comparison()
        {
            if (numeric)
                return NumericComparison(a.GetValue<float>(), b.GetValue<float>());
            else
                return GenericComparison(a.GetValue<object>(), b.GetValue<object>());
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