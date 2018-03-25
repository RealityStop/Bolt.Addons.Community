
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ludiq;
using UnityEngine;

namespace Bolt.Addons.Community.Logic.Units
{
    public abstract class ComparisonBranch : Unit, IBranchUnit
    {
        public ComparisonBranch() : base() { }

        /// <summary>
        /// The entry point for the branch.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter { get; private set; }

        /// <summary>
        /// The first input.
        /// </summary>
        [DoNotSerialize]
        public ValueInput a { get; protected set; }

        /// <summary>
        /// The second input.
        /// </summary>
        [DoNotSerialize]
        public ValueInput b { get; protected set; }


        /// <summary>
        /// The action to execute if the condition is true.
        /// </summary>
        [DoNotSerialize]
        [PortLabel("True")]
        public ControlOutput ifTrue { get; private set; }

        /// <summary>
        /// The action to execute if the condition is false.
        /// </summary>
        [DoNotSerialize]
        [PortLabel("False")]
        public ControlOutput ifFalse { get; private set; }


        protected override void Definition()
        {
            enter = ControlInput("enter", Enter);
            
            ifTrue = ControlOutput("ifTrue");
            ifFalse = ControlOutput("ifFalse");

            Relation(enter, ifTrue);
            Relation(enter, ifFalse);
            Relation(a, ifTrue);
            Relation(b, ifTrue);
            Relation(a, ifFalse);
            Relation(b, ifFalse);
        }

        public abstract bool Comparison();

        public virtual void Enter(Flow flow)
        {
            if (Comparison())
                flow.Invoke(ifTrue);
            else
                flow.Invoke(ifFalse);
        }
    }
}