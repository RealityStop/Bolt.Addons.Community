namespace Unity.VisualScripting.Community
{
    [UnitCategory("Variables")]
    [UnitShortTitle("Plus Equals")]
    [RenamedFrom("Bolt.Addons.Community.Variables.Units.PlusEquals")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.PlusEquals")]
    [UnitTitle("Plus Equals")]
    public sealed class PlusEquals : VariableAdder
    {
        public PlusEquals() : base() { }

        [DoNotSerialize]
        [PortLabel("amount")]
        public ValueInput amount { get; private set; }


        /// <summary>
        /// The value assigned to the variable after incrementing.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueOutput postIncrement { get; private set; }


        protected override void Definition()
        {
            base.Definition();

            amount = ValueInput<float>(nameof(amount), 1);
            postIncrement = ValueOutput<float>(nameof(postIncrement), (x) => _postIncrementValue);
            
            Requirement(amount, assign);
            Requirement(amount, postIncrement);
        }

        protected override float GetAmount(Flow flow)
        {
            return flow.GetValue<float>(amount);
        }
    }
}