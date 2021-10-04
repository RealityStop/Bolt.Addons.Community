using UnityEngine;

namespace Unity.VisualScripting.Community
{    /// <summary>
     /// Returns whether the value is within the given range (inclusive).
     /// </summary>
    [UnitCategory("Community\\Logic")]
    [RenamedFrom("Bolt.Addons.Community.Logic.Units.Polarity")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.Polarity")]
    [TypeIcon(typeof(Unity.VisualScripting.Comparison))]
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
            positive = ValueOutput<bool>(nameof(positive), (flow) => flow.GetValue<float>(input) > 0);
            negative = ValueOutput<bool>(nameof(negative), (flow) => flow.GetValue<float>(input) < 0);
            zero = ValueOutput<bool>(nameof(zero), (flow) => { return Mathf.Approximately(flow.GetValue<float>(input), 0f); });

            Requirement(input, positive);
            Requirement(input, negative);
            Requirement(input, zero);
        }
    }
}