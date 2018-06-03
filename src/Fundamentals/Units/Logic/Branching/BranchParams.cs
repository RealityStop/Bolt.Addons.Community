
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bolt.Addons.Community.Fundamentals;
using Bolt.Addons.Community.Fundamentals.Units;
using Ludiq;
using UnityEngine;

namespace Bolt.Addons.Community.Logic.Units
{
    [UnitShortTitle("Branch")]
    [UnitTitle("Branch (Params)")]
    [UnitCategory("Community\\Control")]
    [TypeIcon(typeof(Branch))]
    [RenamedFrom("Bolt.Addons.Community.Logic.Units.BranchAnd")]
    public sealed class BranchParams : LogicParamNode
    {
        public BranchParams() { }

        [PortLabelHidden]
        [DoNotSerialize]
        public ControlInput enter { get; private set; }

        [PortLabel("True")]
        [DoNotSerialize]
        public ControlOutput exitTrue { get; private set; }

        [PortLabel("False")]
        [DoNotSerialize]
        public ControlOutput exitFalse { get; private set; }


        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), (x) => Branch(x));
            exitTrue = ControlOutput(nameof(exitTrue));
            exitFalse = ControlOutput(nameof(exitFalse));

            base.Definition();

            Relation(enter, exitTrue);
            Relation(enter, exitFalse);
        }

        protected override void BuildRelations(ValueInput arg)
        {
            Relation(arg, exitTrue);
            Relation(arg, exitFalse);
        }

        private bool GetValue()
        {
            foreach (var item in arguments)
            {
                switch (BranchingType)
                {
                    case BranchType.And:
                        if (!item.GetValue<bool>())
                            return false;
                        break;
                    case BranchType.Or:
                        if (item.GetValue<bool>())
                            return true;
                        break;
                    default:
                        return false;
                }
            }

            if (BranchingType == BranchType.And)
                return true;

            if (BranchingType == BranchType.Or)
                return false;

            return false;
        }

        private void Branch(Flow flow)
        {
            if (GetValue())
                flow.Invoke(exitTrue);
            else
                flow.Invoke(exitFalse);
        }
    }
}