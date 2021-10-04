namespace Unity.VisualScripting.Community
{   
    /// <summary>
     /// Returns whether the value is within the given range (inclusive).
     /// </summary>
    [UnitCategory("Community\\Logic")]
    [RenamedFrom("Bolt.Addons.Community.Logic.Units.Between")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.Between")]
    [TypeIcon(typeof(Unity.VisualScripting.And))]
    public class Between : Unit
    {
        public Between() : base() { }

        /// <summary>
        /// The input value to check
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput input { get; private set; }


        /// <summary>
        /// The minimum value (inclusive)
        /// </summary>
        [DoNotSerialize]
        public ValueInput min { get; private set; }


        /// <summary>
        /// The maximum value (inclusive)
        /// </summary>
        [DoNotSerialize]
        public ValueInput max { get; private set; }



        /// <summary>
        /// Whether the input is within the specified range.
        /// </summary>
        [DoNotSerialize]
        public ValueOutput within { get; private set; }


        protected override void Definition()
        {
            input = ValueInput<float>(nameof(input));
            min = ValueInput<float>(nameof(min), 0);
            max = ValueInput<float>(nameof(max), 0);
            within = ValueOutput<bool>(nameof(within), (flow)=>GetWithin(flow));

            Requirement(input, within);
            Requirement(min, within);
            Requirement(max, within);
        }

        private bool GetWithin(Flow flow)
        {
            float inputValue = flow.GetValue<float>(input);

            return (inputValue >= flow.GetValue<float>(min)) &&
                    (inputValue <= flow.GetValue<float>(max));
        }
    }
}