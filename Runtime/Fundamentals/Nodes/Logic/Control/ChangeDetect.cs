namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// Restricts control flow by only allowing through one control flow until reset.
    /// </summary>
    [UnitCategory("Community\\Control")]
    [RenamedFrom("Bolt.Addons.Community.Logic.Units.ChangeDetect")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.ChangeDetect")]
    [TypeIcon(typeof(ISelectUnit))]
    public sealed class ChangeDetect : Unit, IGraphElementWithData
    {
        public sealed class Data : IGraphElementData
        {
            public object lastValue;
        }
        public IGraphElementData CreateData()
        {
            return new Data();
        }


        public ChangeDetect() : base() { }

        /// <summary>
        /// The entry point for the node.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter { get; private set; }

        /// <summary>
        /// The resets the node, allowing the next entry through.
        /// </summary>
        [DoNotSerialize]
        public ValueInput input { get; private set; }

        /// <summary>
        /// The resets the node, allowing the next entry through.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueOutput lastValue{ get; private set; }

        /// <summary>
        /// The exit point for the node.
        /// </summary>
        [DoNotSerialize]
        public ControlOutput onChange { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Enter);
            input = ValueInput<object>(nameof(input));
            lastValue = ValueOutput<object>(nameof(lastValue));
            onChange = ControlOutput(nameof(onChange));

            Succession(enter, onChange);
            Requirement(input, enter);
        }


        public ControlOutput Enter(Flow flow)
        {
            var data = flow.stack.GetElementData<Data>(this);
            object currentValue = flow.GetValue<object>(input);


            if (!OperatorUtility.Equal(data.lastValue, currentValue))
            {
                data.lastValue = currentValue;
                flow.SetValue(lastValue, currentValue);
                return onChange;
            }

            return null;
        }
    }
}