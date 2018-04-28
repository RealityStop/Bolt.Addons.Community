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
    [TypeIcon(typeof(ISelectUnit))]
    public sealed class EdgeTrigger : Unit
    {
        public EdgeTrigger() : base() { }

        /// <summary>
        /// The entry point for the branch.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter { get; private set; }

        /// <summary>
        /// Boolean indicating to let the next control flow through.
        /// </summary>
        [DoNotSerialize]
        public ValueInput inValue { get; private set; }

        /// <summary>
        /// Boolean indicating to let the next control flow through.
        /// </summary>
        [DoNotSerialize]
        public ValueOutput outValue { get; private set; }

        /// <summary>
        /// The exit point for the node.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput exit { get; private set; }

        private bool _lastEdge = true;

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Enter);
            inValue = ValueInput<bool>(nameof(inValue), false);
            outValue = ValueOutput<bool>(nameof(outValue), (recursion) => _lastEdge);
            exit = ControlOutput(nameof(exit));

            Relation(enter, exit);
            Relation(inValue, exit);
            Relation(inValue, outValue);
        }


        public void Enter(Flow flow)
        {
            bool currentValue = inValue.GetValue<bool>();
            if (_lastEdge != currentValue)
            {
                _lastEdge = currentValue;
                flow.Invoke(exit);
            }
        }
    }
}