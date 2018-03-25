
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
    [UnitCategory("Control\\Conditional")]
    [UnitOrder(0)]
    public sealed class BranchEqual : ComparisonBranch
    {
        public BranchEqual() : base() { }

        protected override bool NumericComparison(float a, float b)
        {
            return Mathf.Approximately(a, b);
        }

        protected override bool GenericComparison(object a, object b)
        {
            return OperatorUtility.Equal(a, b);
        }
    }
}