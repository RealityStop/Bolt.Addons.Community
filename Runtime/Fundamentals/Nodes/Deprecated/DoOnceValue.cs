using System;

namespace Unity.VisualScripting.Community.Deprecated
{
    /// <summary>
    /// Restricts control flow by only allowing through one control flow until reset.
    /// </summary>
    [UnitCategory("Community\\Control")]
    [RenamedFrom("Bolt.Addons.Community.Logic.Units.DoOnceValue")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.DoOnceValue")]
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

            Succession(enter, exit);
            Requirement(reset, enter);
            Requirement(reset, testReset);
        }


        public ControlOutput Enter(Flow flow)
        {
            if (_isOpen || flow.GetValue<bool>(reset))
            {
                _isOpen = false;
                return exit;
            }
            return null;
        }

        public ControlOutput TestReset(Flow flow)
        {
            if (flow.GetValue<bool>(reset))
            {
                _isOpen = true;
            }
            return null;
        }
    }
}