namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// Branches flow by checking if a condition is true or false.
    /// </summary>
    [UnitCategory("Community\\Control")]
    [RenamedFrom("Bolt.Addons.Community.Logic.Units.Gate")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.Gate")]
    [TypeIcon(typeof(ISelectUnit))]
    [UnitOrder(0)]
    public sealed class Gate : Unit
    {
        public Gate() : base() { }

        /// <summary>
        /// The entry point for the branch.
        /// </summary>
        [DoNotSerialize]
        public ControlInput enter { get; private set; }


        /// <summary>
        /// The entry point for the branch.
        /// </summary>
        [DoNotSerialize]
        public ControlInput open { get; private set; }


        /// <summary>
        /// The entry point for the branch.
        /// </summary>
        [DoNotSerialize]
        public ControlInput close { get; private set; }



        /// <summary>
        /// The entry point for the branch.
        /// </summary>
        [DoNotSerialize]
        public ControlInput toggle { get; private set; }


        /// <summary>
        /// The condition to check.
        /// </summary>
        [DoNotSerialize]
        [PortLabel("Initially Open")]
        public ValueInput initialState { get; private set; }


        /// </summary>
        [DoNotSerialize]
        [PortLabel("Exit")]
        public ControlOutput exit { get; private set; }

        private bool _isInitial = true;
        private bool _isOpen = false;

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Enter);
            open = ControlInput(nameof(open), Open);
            close = ControlInput(nameof(close), Close);
            toggle = ControlInput(nameof(toggle), Toggle);
            initialState = ValueInput<bool>(nameof(initialState), true);
            exit = ControlOutput(nameof(exit));

            Succession(enter, exit);
            Requirement(initialState, enter);
        }


        public ControlOutput Enter(Flow flow)
        {
            PrepInitialState(flow);

            if (_isOpen)
                return exit;

            return null;
        }

        private ControlOutput Open(Flow obj)
        {
            _isInitial = false;
            _isOpen = true;
            return null;
        }

        private ControlOutput Close(Flow obj)
        {
            _isInitial = false;
            _isOpen = false;
            return null;
        }

        private ControlOutput Toggle(Flow obj)
        {
            _isInitial = false;
            _isOpen = !_isOpen;
            return null;
        }



        private void PrepInitialState(Flow flow)
        {
            if (_isInitial)
                _isOpen = flow.GetValue<bool>(initialState);
            _isInitial = false;
        }
    }
}