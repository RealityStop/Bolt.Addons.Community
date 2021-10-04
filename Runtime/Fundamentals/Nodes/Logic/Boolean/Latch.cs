namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// Latches a boolean value until it is reset.
    /// </summary>
    [UnitCategory("Community\\Logic")]
    [RenamedFrom("Bolt.Addons.Community.Logic.Units.Latch")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.Latch")]
    public sealed class Latch : Unit
    {
        public Latch() : base() { }

        /// <summary>
        /// The entry point for the Latch.
        /// </summary>
        [DoNotSerialize]
        public ControlInput enter { get; private set; }


        /// <summary>
        /// the set input of the branch.  Once set, the output will be true unless
        /// reset is set.
        /// </summary>
        [DoNotSerialize]
        public ValueInput set { get; private set; }


        /// <summary>
        /// the reset input of the branch.  Will release the latch, causing the output
        /// to be false until a set is received.
        /// </summary>
        [DoNotSerialize]
        public ValueInput reset { get; private set; }


        /// <summary>
        /// Determines what should happen if both set and reset are set.  
        /// if set, the reset is dominant and the output is false.  If unset,
        /// the set value is considered dominant and the output will be true.
        /// </summary>
        [DoNotSerialize]
        [PortLabel("Reset Dominant")]
        public ValueInput resetDominant { get; private set; }

        /// <summary>
        /// The exit for the unit
        /// </summary>
        [DoNotSerialize]
        [PortLabel("Exit")]
        public ControlOutput exit { get; private set; }

        /// <summary>
        /// The value output of the latch
        /// </summary>
        [DoNotSerialize]
        public ValueOutput value { get; private set; }

        private bool _isSet = false;

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Enter);
            set = ValueInput<bool>(nameof(set), false);
            reset = ValueInput<bool>(nameof(reset), false);
            resetDominant = ValueInput<bool>(nameof(resetDominant), false);
            exit = ControlOutput(nameof(exit));
            value = ValueOutput<bool>(nameof(value), (x) => _isSet);

            Succession(enter, exit);


            Requirement(set, enter);
            Requirement(reset, enter);
            Requirement(resetDominant, enter);

            Requirement(set, value);
            Requirement(reset, value);
            Requirement(resetDominant, value);
        }


        public ControlOutput Enter(Flow flow)
        {
            if (flow.GetValue<bool>(set))
            {
                if (flow.GetValue<bool>(reset))
                {
                    if (flow.GetValue<bool>(resetDominant))
                        _isSet = false;
                    else
                        _isSet = true;
                }
                else
                {
                    _isSet = true;
                }
            }
            else
            {
                if (flow.GetValue<bool>(reset))
                    _isSet = false;
            }

            return exit;
        }
    }
}