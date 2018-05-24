using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Bolt.Addons.Community.Logic.Units
{
    /// <summary>
    /// Restricts control flow by only allowing through one control flow until reset.
    /// </summary>
    [UnitCategory("Control")]
    [TypeIcon(typeof(ISelectUnit))]
    public sealed class ChangeDetect : Unit
    {
        public ChangeDetect() : base() { }

        /// <summary>
        /// The entry point for the node.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter { get; private set; }

        /// <summary>
        /// The resets the node, allowing the next entry through.
        /// </summary>
        [DoNotSerialize]
        public ValueInput input { get; private set; }

        /// <summary>
        /// The resets the node, allowing the next entry through.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueOutput lastValue{ get; private set; }

        /// <summary>
        /// The exit point for the node.
        /// </summary>
        [DoNotSerialize]
        public ControlOutput onChange { get; private set; }

        private object _previous = null;

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Enter);
            input = ValueInput<object>(nameof(input));
            lastValue = ValueOutput<object>(nameof(lastValue), (x) => _previous);
            onChange = ControlOutput(nameof(onChange));

            Relation(enter, onChange);
            Relation(input, onChange);
        }


        public void Enter(Flow flow)
        {
            object currentValue = input.GetValue<object>();

            if (currentValue is float && _previous is float)
            {
                if (!Mathf.Approximately((float)currentValue, (float)_previous))
                {
                    _previous = currentValue;
                    flow.Invoke(onChange);
                }
            }
            else
            {
                if (currentValue != _previous)
                {
                    _previous = currentValue;
                    flow.Invoke(onChange);
                }
            }
        }
    }
}