namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// Takes a given float input (0-1) and scales it across the specified range.
    /// </summary>
    [UnitCategory("Community\\Math\\Functions")]
    [RenamedFrom("Bolt.Addons.Community.Custom_Units.Math.Functions.LogarithmicFunction")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.LogarithmicFunction")]
    [UnitTitle("Logarithmic")]
    [UnitOrder(5)]
    public class LogarithmicFunction : Unit
    {
        public LogarithmicFunction() : base() { }

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
        [PortLabel("Minimum")]
        public ValueInput minimum { get; private set; }

        /// <summary>
        /// The maximum valid output.  Returned when the input is 1.
        /// </summary>
        [DoNotSerialize]
        [PortLabel("Exponent")]
        public ValueInput exponent { get; private set; }

        /// <summary>
        /// The maximum valid output.  Returned when the input is 1.
        /// </summary>
        [DoNotSerialize]
        [PortLabel("Scale")]
        public ValueInput scale { get; private set; }


        /// <summary>
        /// The result of the interpolation (minimum-maximum).
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueOutput output { get; private set; }

        [DoNotSerialize]
        protected float defaultMinimum => 0;

        [DoNotSerialize]
        protected float defaultExponent => 1;

        [DoNotSerialize]
        protected float defaultScale => 1;

        protected override void Definition()
        {
            input = ValueInput<float>(nameof(input), 0);
            minimum = ValueInput<float>(nameof(minimum), defaultMinimum);
            exponent = ValueInput<float>(nameof(exponent), defaultExponent);
            scale = ValueInput<float>(nameof(scale), defaultScale);
            output = ValueOutput<float>(nameof(output), Operation);

            Requirement(input, output);
            Requirement(minimum, output);
            Requirement(exponent, output);
            Requirement(scale, output);
        }

        private float Operation(Flow flow)
        {
            return MathLibrary.LogarithmicFunction(flow.GetValue<float>(input), flow.GetValue<float>(minimum), flow.GetValue<float>(exponent), flow.GetValue<float>(scale));
        }
    }
}