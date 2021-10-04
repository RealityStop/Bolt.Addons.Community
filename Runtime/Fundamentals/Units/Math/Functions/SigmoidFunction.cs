using UnityEngine;

namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// Takes a given float input (0-1) and scales it across the specified range.
    /// </summary>
    [UnitCategory("Community\\Math\\Functions")]
    [RenamedFrom("Bolt.Addons.Community.Math.Custom_Units.Math.Functions.SigmoidFunction")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.SigmoidFunction")]
    [UnitTitle("Sigmoid")]
    [UnitOrder(9)]
    public class SigmoidFunction : Unit
    {
        public SigmoidFunction() : base() { }

        /// <summary>
        /// The (0-1) input to interpolate onto the range between minimum and maximum
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput input { get; private set; }

        /// <summary>
        /// The minimum valid output.  Returned when the input is 0.
        /// </summary>
        [DoNotSerialize]
        [PortLabel("MinimumValue")]
        public ValueInput minimum { get; private set; }

        /// <summary>
        /// The maximum valid output.  Returned when the input is 1.
        /// </summary>
        [DoNotSerialize]
        [PortLabel("InflectionPoint")]
        public ValueInput inflectionPoint { get; private set; }

        /// <summary>
        /// The maximum valid output.  Returned when the input is 1.
        /// </summary>
        [DoNotSerialize]
        [PortLabel("Scale")]
        public ValueInput scale { get; private set; }

        /// <summary>
        /// The maximum valid output.  Returned when the input is 1.
        /// </summary>
        [DoNotSerialize]
        [PortLabel("DecayFactor")]
        public ValueInput decayFactor { get; private set; }

        /// <summary>
        /// The result of the interpolation (minimum-maximum).
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueOutput output { get; private set; }

        [DoNotSerialize]
        protected float defaultMinimum => 0;

        [DoNotSerialize]
        protected float defaultDecayFactor => 1;

        [DoNotSerialize]
        protected float defaultScale => 1;

        [DoNotSerialize]
        protected float defaultInflectionPoint => 0.5f;

        protected override void Definition()
        {
            input = ValueInput<float>(nameof(input), 0);
            minimum = ValueInput<float>(nameof(minimum), defaultMinimum);
            inflectionPoint = ValueInput<float>(nameof(inflectionPoint), defaultInflectionPoint);
            decayFactor = ValueInput<float>(nameof(decayFactor), defaultDecayFactor);
            scale = ValueInput<float>(nameof(scale), defaultScale);
            output = ValueOutput<float>(nameof(output), (flow) => Operation(flow));

            Requirement(input, output);
            Requirement(minimum, output);
            Requirement(inflectionPoint, output);
            Requirement(decayFactor, output);
            Requirement(scale, output);
        }

        private float Operation(Flow flow)
        {
            float inputValue = Mathf.Clamp01(flow.GetValue<float>(input));
            float inflectionPointValue = Mathf.Clamp01(flow.GetValue<float>(inflectionPoint));

            return MathLibrary.DecayingSigmoid(inputValue, inflectionPointValue, flow.GetValue<float>(minimum), flow.GetValue<float>(decayFactor), flow.GetValue<float>(scale));
        }
    }
}