namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(ConvertNode))]
    public sealed class ConvertNodeDescriptor : UnitDescriptor<ConvertNode>
    {
        public ConvertNodeDescriptor(ConvertNode target) : base(target)
        {
        }

        protected override void DefinedPort(IUnitPort port, UnitPortDescription description)
        {
            base.DefinedPort(port, description);

            description.showLabel = false;
        }
    }
} 