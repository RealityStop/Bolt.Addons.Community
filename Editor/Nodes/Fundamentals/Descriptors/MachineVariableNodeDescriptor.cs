namespace Unity.VisualScripting.Community
{
    public abstract class MachineVariableNodeDescriptor : UnitDescriptor<MachineVariableNode>
    {
        public MachineVariableNodeDescriptor(MachineVariableNode unit) : base(unit)
        {

        }

        protected abstract string Prefix { get; }

        protected override string DefinedTitle()
        {
            return $"{Prefix} Machine Variable";
        }

        protected override string DefinedShortTitle()
        {
            return $"{Prefix} Machine Variable";
        }

        protected override string DefaultTitle()
        {
            return $"{Prefix} Machine Variable";
        }

        protected override string DefaultShortTitle()
        {
            return $"{Prefix} Machine Variable";
        }
    }
}
