namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(SetWindowVariableNode))]
    public sealed class SetWindowVariableNodeDescriptor : WindowVariableNodeDescriptor
    {
        public SetWindowVariableNodeDescriptor(SetWindowVariableNode unit) : base(unit)
        {

        }
        protected override string Prefix => "Set";
    }
}
