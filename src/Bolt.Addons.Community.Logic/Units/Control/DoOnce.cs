using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Addons.Community.Logic.Units
{
    /// <summary>
    /// Branches flow by checking if a condition is true or false.
    /// </summary>
    [UnitCategory("Control")]
    [UnitTitle("Do Once")]
    [TypeIcon(typeof(ISelectUnit))]
    public sealed class DoOnce : Unit
    {
        public DoOnce() : base() { }

        /// <summary>
        /// The entry point for the branch.
        /// </summary>
        [DoNotSerialize]
        public ControlInput enter { get; private set; }

        /// <summary>
        /// The entry point for the branch.
        /// </summary>
        [DoNotSerialize]
        public ControlInput reset { get; private set; }

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