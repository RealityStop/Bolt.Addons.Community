namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(TriggerChannelEvent))]
    public class TriggerChannelEventDescriptor : UnitDescriptor<TriggerChannelEvent>
    {
        public TriggerChannelEventDescriptor(TriggerChannelEvent unit) : base(unit) { }

        protected override void DefinedPort(IUnitPort port, UnitPortDescription description)
        {
            base.DefinedPort(port, description);

            switch (port.key)
            {
                case "InputTrigger":
                    description.summary = "Executes this node and sends an event to all OnChannelEvent nodes on the same channel.";
                    break;

                case "OutputTrigger":
                    description.summary = "Executed after the channel event has been triggered.";
                    break;

                case "channel":
                    description.summary = "The channel to broadcast the event on.";
                    break;
            }
        }
    }
}