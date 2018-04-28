using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Bolt.Addons.Community.Logic.Units
{    /// <summary>
     /// Returns whether the value is within the given range (inclusive).
     /// </summary>
    [UnitCategory("Logic")]
    [TypeIcon(typeof(Bolt.Comparison))]
    public class Polarity : Unit
    {
        public Polarity() : base() { }

        /// <summary>
        /// The input value to check
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput input { get; private set; }


        /// <summary>
        /// Whether the input is greater than zero.
        /// </summary>
        [DoNotSerialize]
        public ValueOutput positive { get; private set; }


        /// <summary>
        /// Whether the input is less than zero.
        /// </summary>
        [DoNotSerialize]
        public ValueOutput negative { get; private set; }

        /// <summary>
        /// Whether the input is 0.
        /// </summary>
        [DoNotSerialize]
        public ValueOutput zero { get; private set; }


        protected override void Definition()
        {
            input = ValueInput<float>(nameof(input));
            positive = ValueOutput<bool>(nameof(positive), (recursion) => input.GetValue<float>() > 0);
            negative = ValueOutput<bool>(nameof(negative), (recursion) => input.GetValue<float>() < 0);
            zero = ValueOutput<bool>(nameof(zero), (recursion) => { return Mathf.Approximately(input.GetValue<float>(), 0f); });

            Relation(input, positive);
            Relation(input, negative);
            Relation(input, zero);
        }
    }
}