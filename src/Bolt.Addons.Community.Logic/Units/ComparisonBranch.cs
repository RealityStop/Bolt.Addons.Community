
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
        public ValueInput a { get; private set; }

        /// <summary>
        /// The second input.
        /// </summary>
        [DoNotSerialize]
        public ValueInput b { get; private set; }

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


        /// <summary>
        /// Whether the compared inputs are numbers.
        /// </summary>
        [Serialize]
        [Inspectable]
        [InspectorToggleLeft]
        public bool numeric { get; set; } = true;

        protected override void Definition()
        {

            enter = ControlInput("enter", Enter);

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

            ifTrue = ControlOutput("ifTrue");
            ifFalse = ControlOutput("ifFalse");

            Relation(enter, ifTrue);
            Relation(enter, ifFalse);
            Relation(a, ifTrue);
            Relation(b, ifTrue);
            Relation(a, ifFalse);
            Relation(b, ifFalse);
        }

        public void Enter(Flow flow)
        {
            bool istrue = false;
            if (numeric)
                istrue = NumericComparison(a.GetValue<float>(), b.GetValue<float>());
            else
                istrue = GenericComparison(a.GetValue<object>(), b.GetValue<object>());


            if (istrue)
                flow.Invoke(ifTrue);
            else
                flow.Invoke(ifFalse);
        }


        protected abstract bool NumericComparison(float a, float b);

        protected abstract bool GenericComparison(object a, object b);
    }
}