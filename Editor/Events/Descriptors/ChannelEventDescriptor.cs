namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(ChannelEvent))]
    public class ChannelEventDescriptor : UnitDescriptor<ChannelEvent>
    {
        public ChannelEventDescriptor(ChannelEvent unit) : base(unit) { }

        protected override string DefinedSummary()
        {
            return "Triggers when an event occurs on the chosen channel.";
        }
    }
}