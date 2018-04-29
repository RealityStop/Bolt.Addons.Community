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
    public sealed class DoOnceV2 : Unit
    {
        public DoOnceV2() : base() { }

        /// <summary>
        /// The entry point for the node.
        /// </summary>
        [DoNotSerialize]
        public ControlInput enter { get; private set; }

        /// <summary>
        /// The resets the node, allowing the next entry through.
        /// </summary>
        [DoNotSerialize]
        [PortLabel("Reset")]
        public ControlInput resetFlow { get; private set; }


        #region Only valid if boolean is true
        /// <summary>
        /// Allows requering the reset without passing through control flow.  Does NOT trigger exit.
        /// </summary>
        [DoNotSerialize]
        [PortLabel("Test Reset")]
        public ControlInput testReset { get; private set; }

        /// <summary>
        /// Boolean indicating to let the next control flow through.
        /// </summary>
        [DoNotSerialize]
        [PortLabel("Reset")]
        public ValueInput resetBool { get; private set; }
        #endregion

        /// <summary>
        /// Whether the compared inputs are numbers.
        /// </summary>
        [Serialize]
        [Inspectable]
        [InspectorToggleLeft]
        public bool boolean { get; set; } = false;


        /// <summary>
        /// The exit point for the node.
        /// </summary>
        [DoNotSerialize]
        public ControlOutput exit { get; private set; }

        private bool _isOpen = true;

        protected override void Definition()
        {
            if (!boolean)
            {
                enter = ControlInput(nameof(enter), Enter);
                resetFlow = ControlInput(nameof(resetFlow), Reset);
                exit = ControlOutput(nameof(exit));
            }
            else
            {
                enter = ControlInput(nameof(enter), Enter);
                testReset = ControlInput(nameof(testReset), TestReset);
                resetBool = ValueInput<bool>(nameof(resetBool), false);
                exit = ControlOutput(nameof(exit));

                Relation(resetBool, enter);
                Relation(resetBool, testReset);
            }
            Relation(enter, exit);
        }


        public void Enter(Flow flow)
        {
            if (_isOpen)
            {
                _isOpen = false;
                flow.Invoke(exit);
            }
        }

        private void Reset(Flow obj)
        {
            _isOpen = true;
        }

        public void TestReset(Flow flow)
        {
            if (resetBool.GetValue<bool>())
            {
                _isOpen = true;
            }
        }
    }
}