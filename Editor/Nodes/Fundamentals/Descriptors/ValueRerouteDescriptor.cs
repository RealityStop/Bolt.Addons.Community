namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(ValueReroute))]
    public sealed class ValueRerouteDescriptor : UnitDescriptor<ValueReroute>
    {
        public ValueRerouteDescriptor(ValueReroute target) : base(target)
        {
        }

        protected override void DefinedPort(IUnitPort port, UnitPortDescription description)
        {
            base.DefinedPort(port, description);
            
            description.showLabel = false;
        }

        protected override EditorTexture DefaultIcon()
        {
            return PathUtil.Load("value_reroute", CommunityEditorPath.Fundamentals);
        }

        protected override EditorTexture DefinedIcon()
        {
            return PathUtil.Load("value_reroute", CommunityEditorPath.Fundamentals);
        }
    }
} 