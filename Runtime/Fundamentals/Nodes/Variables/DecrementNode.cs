namespace Unity.VisualScripting.Community
{
    [UnitCategory("Variables")]
    [UnitShortTitle("Decrement Variable")]
    [RenamedFrom("Bolt.Addons.Community.Variables.Units.DecrementUnit")]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.DecrementUnit")]
    [UnitTitle("Decrement")]
    public sealed class DecrementNode : IncrementNode
    {
        public DecrementNode() : base() { }

        protected override float GetAmount(Flow flow)
        {
            return -1;
        }
    }
}