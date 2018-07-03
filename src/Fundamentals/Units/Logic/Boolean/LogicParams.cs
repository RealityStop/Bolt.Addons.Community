using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Bolt.Addons.Community.Fundamentals
{
    [UnitShortTitle("Logic")]
    [UnitTitle("Logic (Params)")]
    [UnitCategory("Community\\Logic")]
    [RenamedFrom("Bolt.Addons.Community.Logic.Units.LogicParams")]
    [RenamedFrom("Bolt.Addons.Community.Logic.Units.OrParam")]
    public sealed class LogicParams : LogicParamNode
    {
        public LogicParams() { }

        [PortLabel("Result")]
        [DoNotSerialize]
        public ValueOutput output { get; private set; }

        protected override void Definition()
        {
            output = ValueOutput<bool>(nameof(output), GetValue);

            base.Definition();
        }

        protected override void BuildRelations(ValueInput arg)
        {
            Relation(arg, output);
        }

        private bool GetValue(Recursion arg1)
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
    }
}