using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Bolt.Addons.Community.Fundamentals
{
    public abstract class LogicParamNode : VariadicNode<bool>
    {
        public enum BranchType { And, Or }


        [SerializeAs(nameof(BranchingType))]
        private BranchType _branchingType;

        [DoNotSerialize]
        [Inspectable, UnitHeaderInspectable("Condition")]
        public BranchType BranchingType { get { return _branchingType; } set { _branchingType = value; } }


        protected override void Definition()
        {
            base.Definition();
        }
    }
}