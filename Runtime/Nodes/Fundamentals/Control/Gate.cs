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
    public sealed class Gate : Unit, IGraphElementWithData
    {
        public Gate() : base() { }

        private class Data : IGraphElementData
        {
            public bool isInitial = true;
            public bool isOpen = false;
        }

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
            var data = flow.stack.GetElementData<Data>(this);

            PrepInitialState(flow, data);

            if (data.isOpen)
                return exit;

            return null;
        }

        private ControlOutput Open(Flow flow)
        {
            var data = flow.stack.GetElementData<Data>(this);

            data.isInitial = false;
            data.isOpen = true;
            return null;
        }

        private ControlOutput Close(Flow flow)
        {
            var data = flow.stack.GetElementData<Data>(this);

            data.isInitial = false;
            data.isOpen = false;
            return null;
        }

        private ControlOutput Toggle(Flow flow)
        {
            var data = flow.stack.GetElementData<Data>(this);

            data.isInitial = false;
            data.isOpen = !data.isOpen;
            return null;
        }

        private void PrepInitialState(Flow flow, Data data)
        {
            if (data.isInitial)
                data.isOpen = flow.GetValue<bool>(initialState);
            data.isInitial = false;
        }

        public IGraphElementData CreateData()
        {
            return new Data();
        }
    }
}