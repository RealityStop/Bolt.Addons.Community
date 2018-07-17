
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bolt.Addons.Community.Fundamentals;
using Ludiq;
using UnityEngine;

namespace Bolt.Addons.Community.Fundamentals
{
    [UnitShortTitle("Branch")]
    [UnitTitle("Branch (Params)")]
    [UnitCategory("Community\\Control")]
    [TypeIcon(typeof(Branch))]
    [RenamedFrom("Bolt.Addons.Community.Logic.Units.BranchAnd")]
    [RenamedFrom("Bolt.Addons.Community.Logic.Units.BranchParams")]
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

            Succession(enter, exitTrue);
            Succession(enter, exitFalse);
        }

        protected override void BuildRelations(ValueInput arg)
        {
            Requirement(arg, enter);
        }

        private bool GetValue(Flow flow)
        {
            foreach (var item in arguments)
            {
                switch (BranchingType)
                {
                    case BranchType.And:
                        if (!flow.GetValue<bool>(item))
                            return false;
                        break;
                    case BranchType.Or:
                        if (flow.GetValue<bool>(item))
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

        private ControlOutput Branch(Flow flow)
        {
            if (GetValue(flow))
                return exitTrue;
            else
                return exitFalse;
        }
    }
}