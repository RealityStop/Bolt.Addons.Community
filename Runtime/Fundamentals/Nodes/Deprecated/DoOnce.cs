using System;

namespace Unity.VisualScripting.Community.Deprecated
{
    /// <summary>
    /// Restricts control flow by only allowing through one control flow until reset.
    /// </summary>
    [UnitCategory("Community\\Control")]
    [RenamedFrom("Bolt.Addons.Community.Logic.Units.DoOnce")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.DoOnce")]
    [UnitTitle("Do Once")]
    [TypeIcon(typeof(ISelectUnit))]
    [Obsolete]
    public sealed class DoOnce : Unit
    {
        public DoOnce() : base() { }

        /// <summary>
        /// The entry point for the node.
        /// </summary>
        [DoNotSerialize]
        public ControlInput enter { get; private set; }

        /// <summary>
        /// The resets the node, allowing the next entry through.
        /// </summary>
        [DoNotSerialize]
        public ControlInput reset { get; private set; }

        /// <summary>
        /// The exit point for the node.
        /// </summary>
        [DoNotSerialize]
        public ControlOutput exit { get; private set; }

        private bool _isOpen = true;

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Enter);
            reset = ControlInput(nameof(reset), Reset);
            exit = ControlOutput(nameof(exit));

            Succession(enter, exit);
        }

        public ControlOutput Enter(Flow flow)
        {
            if (_isOpen)
            {
                _isOpen = false;
                return exit;
            }
            return null;
        }

        private ControlOutput Reset(Flow obj)
        {
            _isOpen = true;
            return null;
        }
    }
}