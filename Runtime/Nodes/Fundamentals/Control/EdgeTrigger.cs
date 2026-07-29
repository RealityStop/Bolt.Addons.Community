namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// Restricts control flow by only allowing through one control flow until reset.
    /// </summary>
    [UnitCategory("Community\\Control")]
    [RenamedFrom("Bolt.Addons.Community.Logic.Units.EdgeTrigger")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.EdgeTrigger")]
    [TypeIcon(typeof(ISelectUnit))]
    public sealed class EdgeTrigger : Unit, IGraphElementWithData
    {
        public EdgeTrigger() : base() { }

        private class Data : IGraphElementData
        {
            public bool? lastEdge;
        }

        /// <summary>
        /// The entry point for the branch.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter { get; private set; }

        /// <summary>
        /// Boolean indicating to let the next control flow through.
        /// </summary>
        [DoNotSerialize]
        public ValueInput inValue { get; private set; }

        /// <summary>
        /// Boolean indicating to let the next control flow through.
        /// </summary>
        [DoNotSerialize]
        public ValueOutput outValue { get; private set; }

        /// <summary>
        /// The exit point for the node.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput exit { get; private set; }

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Enter);
            inValue = ValueInput<bool>(nameof(inValue), false);
            outValue = ValueOutput<bool>(nameof(outValue), (flow) =>
            {
                var data = flow.stack.GetElementData<Data>(this);

                if (data.lastEdge.HasValue)
                    return data.lastEdge.Value;
                return false;
            });
            exit = ControlOutput(nameof(exit));

            Succession(enter, exit);
            Requirement(inValue, enter);
            Requirement(inValue, outValue);
        }


        public ControlOutput Enter(Flow flow)
        {
            var data = flow.stack.GetElementData<Data>(this);
            bool currentValue = flow.GetValue<bool>(inValue);
            if (!data.lastEdge.HasValue || data.lastEdge != currentValue)
            {
                data.lastEdge = currentValue;
                return exit;
            }

            return null;
        }

        public IGraphElementData CreateData()
        {
            return new Data();
        }
    }
}