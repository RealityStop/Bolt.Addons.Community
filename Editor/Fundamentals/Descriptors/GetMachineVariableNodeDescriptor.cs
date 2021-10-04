namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(GetMachineVariableNode))]
    public sealed class GetMachineVariableNodeDescriptor : MachineVariableNodeDescriptor
    {
        public GetMachineVariableNodeDescriptor(GetMachineVariableNode unit) : base(unit)
        {

        }

        protected override string Prefix => "Get";
    }
}
