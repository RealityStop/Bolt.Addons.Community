namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// Takes a given float input (minimum-maximum) and returns a 0-1 value that represents the position along the range (percent).
    /// </summary>
    [UnitCategory("Community\\Math\\Functions")]
    [RenamedFrom("Bolt.Addons.Community.Custom_Units.Math.Functions.LinearFunctionOfRange")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.LinearFunctionOfRange")]
    [UnitTitle("Linear Function of Range")]
    [UnitOrder(2)]
    public class LinearFunctionOfRange : Unit
    {
        public LinearFunctionOfRange() : base() { }

        /// <summary>
        /// The (minimum-maximum) input to interpolate from the range between minimum and maximum
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput input { get; private set; }


        /// <summary>
        /// The minimum valid input. 
        /// </summary>
        [DoNotSerialize]
        [PortLabel("MinInputRange")]
        public ValueInput minInputRange{ get; private set; }

        /// <summary>
        /// The maximum valid input.
        /// </summary>
        [DoNotSerialize]
        [PortLabel("MaxInputRange")]
        public ValueInput maxInputRange{ get; private set; }


        /// <summary>
        /// The minimum valid input. 
        /// </summary>
        [DoNotSerialize]
        [PortLabel("Minimum")]
        public ValueInput minimum { get; private set; }

        /// <summary>
        /// The maximum valid input.
        /// </summary>
        [DoNotSerialize]
        [PortLabel("Maximum")]
        public ValueInput maximum { get; private set; }


        /// <summary>
        /// The result of the reverse interpolation (0-1).
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueOutput output { get; private set; }


        [DoNotSerialize]
        protected float defaultMinRange => 0;

        [DoNotSerialize]
        protected float defaultMaxRange => 1;

        [DoNotSerialize]
        protected float defaultMinimum => 0;

        [DoNotSerialize]
        protected float defaultMaximum => 1;

        protected override void Definition()
        {
            input = ValueInput<float>(nameof(input), 0);
            minInputRange = ValueInput<float>(nameof(minInputRange), defaultMinRange);
            maxInputRange = ValueInput<float>(nameof(maxInputRange), defaultMaxRange);
            minimum = ValueInput<float>(nameof(minimum), defaultMinimum);
            maximum = ValueInput<float>(nameof(maximum), defaultMaximum);
            output = ValueOutput<float>(nameof(output), Operation);

            Requirement(input, output);
            Requirement(minInputRange, output);
            Requirement(maxInputRange, output);
            Requirement(minimum, output);
            Requirement(maximum, output);
        }

        private float Operation(Flow flow)
        {
            return MathLibrary.LinearFunctionOfRange(
                flow.GetValue<float>(input),
                flow.GetValue<float>(minInputRange),
                flow.GetValue<float>(maxInputRange), flow.GetValue<float>(minimum), flow.GetValue<float>(maximum));
        }
    }
}