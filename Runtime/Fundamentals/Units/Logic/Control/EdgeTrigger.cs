namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// Restricts control flow by only allowing through one control flow until reset.
    /// </summary>
    [UnitCategory("Community\\Control")]
    [RenamedFrom("Bolt.Addons.Community.Logic.Units.EdgeTrigger")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.EdgeTrigger")]
    [TypeIcon(typeof(ISelectUnit))]
    public sealed class EdgeTrigger : Unit
    {
        public EdgeTrigger() : base() { }

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

        private bool? _lastEdge;

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Enter);
            inValue = ValueInput<bool>(nameof(inValue), false);
            outValue = ValueOutput<bool>(nameof(outValue), (recursion) => { if (_lastEdge.HasValue) return _lastEdge.Value; return false; });
            exit = ControlOutput(nameof(exit));

            Succession(enter, exit);
            Requirement(inValue, enter);
            Requirement(inValue, outValue);
        }


        public ControlOutput Enter(Flow flow)
        {
            bool currentValue = flow.GetValue<bool>(inValue);
            if (!_lastEdge.HasValue || _lastEdge != currentValue)
            {
                _lastEdge = currentValue;
                return exit;
            }

            return null;
        }
    }
}