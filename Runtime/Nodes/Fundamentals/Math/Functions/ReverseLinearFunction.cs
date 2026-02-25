namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// Takes a given float input (minimum-maximum) and returns a 0-1 value that represents the position along the range (percent).
    /// </summary>
    [UnitCategory("Community\\Math\\Functions")]
    [RenamedFrom("Bolt.Addons.Community.Custom_Units.Math.Functions.ReverseLinearFunction")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.ReverseLinearFunction")]
    [UnitTitle("Reverse Linear")]
    [UnitOrder(1)]
    public class ReverseLinearFunction : Unit
    {
        public ReverseLinearFunction() : base() { }

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
            return MathLibrary.ReverseLinearFunction(flow.GetValue<float>(minimum), flow.GetValue<float>(maximum), flow.GetValue<float>(input));
        }
    }
}