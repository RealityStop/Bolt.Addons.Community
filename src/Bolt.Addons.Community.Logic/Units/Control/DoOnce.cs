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
    public sealed class DoOnce : Unit
    {
        public DoOnce() : base() { }

        /// <summary>
        /// The entry point for the node.
        /// </summary>
        [DoNotSerialize]
        public ControlInput enter { get; private set; }

        /// <summary>
        /// The resets the node, allowing the next entry through.
        /// </summary>
        [DoNotSerialize]
        public ControlInput reset { get; private set; }

        /// <summary>
        /// The exit point for the node.
        /// </summary>
        [DoNotSerialize]
        public ControlOutput exit { get; private set; }

        private bool _isOpen = true;

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Enter);
            reset = ControlInput(nameof(reset), Reset);
            exit = ControlOutput(nameof(exit));

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
    }
}