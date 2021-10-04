namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// Takes a given float input (0-1) and scales it across the specified range.
    /// </summary>
    [UnitCategory("Community\\Math\\Functions")]
    [RenamedFrom("Bolt.Addons.Community.Custom_Units.Math.Functions.LinearFunction")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.LinearFunction")]
    [UnitTitle("Linear")]
    [UnitOrder(0)]
    public class LinearFunction : Unit
    {
        public LinearFunction() : base() { }

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
        [PortLabel("Maximum")]
        public ValueInput maximum { get; private set; }

        
        /// <summary>
        /// The result of the interpolation (minimum-maximum).
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueOutput output { get; private set; }

        [DoNotSerialize]
        protected float defaultMinimum => 0;

        [DoNotSerialize]
        protected float defaultMaximum => 1;

        protected override void Definition()
        {
            input = ValueInput<float>(nameof(input), 0);
            minimum = ValueInput<float>(nameof(minimum), defaultMinimum);
            maximum = ValueInput<float>(nameof(maximum), defaultMaximum);
            output = ValueOutput<float>(nameof(output), Operation);

            Requirement(input, output);
            Requirement(minimum, output);
            Requirement(maximum, output);
        }

        private float Operation(Flow flow)
        {
            return MathLibrary.LinearFunction(flow.GetValue<float>(minimum), flow.GetValue<float>(maximum), flow.GetValue<float>(input));
        }
    }
}