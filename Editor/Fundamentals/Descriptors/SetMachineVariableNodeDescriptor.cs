namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(SetMachineVariableNode))]
    public sealed class SetMachineVariableNodeDescriptor : MachineVariableNodeDescriptor
    {
        public SetMachineVariableNodeDescriptor(SetMachineVariableNode unit) : base(unit)
        {

        }
        protected override string Prefix => "Set";
    }
}
