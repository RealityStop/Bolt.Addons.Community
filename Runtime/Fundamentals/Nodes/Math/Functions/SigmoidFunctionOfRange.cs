namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// Takes a given float input (0-1) and scales it across the specified range.
    /// </summary>
    [UnitCategory("Community\\Math\\Functions")]
    [RenamedFrom("Bolt.Addons.Community.Math.Custom_Units.Math.Functions.SigmoidFunctionOfRange")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.SigmoidFunctionOfRange")]
    [UnitTitle("Sigmoid Function Of Range")]
    [UnitOrder(10)]
    public class SigmoidFunctionOfRange : Unit
    {
        public SigmoidFunctionOfRange() : base() { }

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
        /// The minimum valid output.  Returned when the input is 0.
        /// </summary>
        [DoNotSerialize]
        [PortLabel("MinimumRange")]
        public ValueInput minimumRange { get; private set; }


        /// <summary>
        /// The minimum valid output.  Returned when the input is 0.
        /// </summary>
        [DoNotSerialize]
        [PortLabel("MaximumRange")]
        public ValueInput maximumRange { get; private set; }


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
        protected float defaultMinimumRange => 0;

        [DoNotSerialize]
        protected float defaultMaximumRange => 100;

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
            minimumRange = ValueInput<float>(nameof(minimumRange), defaultMinimumRange);
            maximumRange = ValueInput<float>(nameof(maximumRange), defaultMaximumRange);
            inflectionPoint = ValueInput<float>(nameof(inflectionPoint), defaultInflectionPoint);
            decayFactor = ValueInput<float>(nameof(decayFactor), defaultDecayFactor);
            scale = ValueInput<float>(nameof(scale), defaultScale);
            output = ValueOutput<float>(nameof(output), (x) => Operation(x));


            Requirement(minimumRange, output);
            Requirement(maximumRange, output);
            Requirement(input, output);
            Requirement(minimum, output);
            Requirement(inflectionPoint, output);
            Requirement(decayFactor, output);
            Requirement(scale, output);
        }

        private float Operation(Flow flow)
        {
            float normalizedInput = MathLibrary.ReverseLinearFunction(flow.GetValue<float>(input), flow.GetValue<float>(minimumRange), flow.GetValue<float>(maximumRange));
            float normalizedInflection = MathLibrary.ReverseLinearFunction(flow.GetValue<float>(inflectionPoint), flow.GetValue<float>(minimumRange), flow.GetValue<float>(maximumRange));

            return MathLibrary.DecayingSigmoid(normalizedInput, normalizedInflection, flow.GetValue<float>(minimum), flow.GetValue<float>(decayFactor), flow.GetValue<float>(scale));
        }
    }
}