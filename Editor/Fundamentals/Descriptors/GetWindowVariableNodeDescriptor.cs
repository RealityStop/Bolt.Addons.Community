namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(GetWindowVariableNode))]
    public sealed class GetWindowVariableNodeDescriptor : WindowVariableNodeDescriptor
    {
        public GetWindowVariableNodeDescriptor(GetWindowVariableNode unit) : base(unit)
        {

        }
        protected override string Prefix => "Get";
    }
}
