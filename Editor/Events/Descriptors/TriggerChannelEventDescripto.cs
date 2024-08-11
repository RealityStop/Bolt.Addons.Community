using Unity.VisualScripting;

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
                    description.summary = "Triggers all OnChannelEvent nodes that are on the channel of this node";
                    break;
                case "OutputTrigger":
                    description.summary = "Called after the node has been triggered";
                    break;
                case "channel":
                    description.summary = "The channel to trigger the OnChannelEvent node";
                    break;
            }
        }
    }
}