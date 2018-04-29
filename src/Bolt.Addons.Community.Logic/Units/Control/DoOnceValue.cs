using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Addons.Community.Logic.Units
{
    /// <summary>
    /// Restricts control flow by only allowing through one control flow until reset.
    /// </summary>
    [UnitCategory("Control")]
    [UnitTitle("Do Once")]
    [TypeIcon(typeof(ISelectUnit))]
    [Obsolete]
    public sealed class DoOnceValue : Unit
    {
        public DoOnceValue() : base() { }

        /// <summary>
        /// The entry point for the branch.
        /// </summary>
        [DoNotSerialize]
        public ControlInput enter { get; private set; }


        /// <summary>
        /// Allows requering the reset without passing through control flow.  Does NOT trigger exit.
        /// </summary>
        [DoNotSerialize]
        public ControlInput testReset { get; private set; }

        /// <summary>
        /// Boolean indicating to let the next control flow through.
        /// </summary>
        [DoNotSerialize]
        public ValueInput reset { get; private set; }

        /// <summary>
        /// The exit point for the node.
        /// </summary>
        [DoNotSerialize]
        public ControlOutput exit { get; private set; }

        private bool _isOpen = true;

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Enter);
            testReset = ControlInput(nameof(testReset), TestReset);
            reset = ValueInput<bool>(nameof(reset), false);
            exit = ControlOutput(nameof(exit));

            Relation(enter, exit);
            Relation(reset, enter);
            Relation(reset, testReset);
        }


        public void Enter(Flow flow)
        {
            if (_isOpen || reset.GetValue<bool>())
            {
                _isOpen = false;
                flow.Invoke(exit);
            }
        }

        public void TestReset(Flow flow)
        {
            if (reset.GetValue<bool>())
            {
                _isOpen = true;
            }
        }
    }
}