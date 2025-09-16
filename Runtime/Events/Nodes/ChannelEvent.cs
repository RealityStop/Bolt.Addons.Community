using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("ChannelEvent")]
    [UnitTitle("Channel Event")]
    [UnitCategory("Events\\Community")]
    public class ChannelEvent : EventUnit<Channel>
    {
        [PortLabelHidden]
        public ValueInput Channel { get; set; }

        protected override bool register => true;

        public override EventHook GetHook(GraphReference reference)
        {
            return new EventHook(CommunityEvents.ChannelEvent);
        }

        protected override void Definition()
        {
            base.Definition();

            Channel = ValueInput(nameof(Channel), Community.Channel.Channel1);
        }

        protected override bool ShouldTrigger(Flow flow, Channel args)
        {
            return args == flow.GetValue<Channel>(Channel);
        }
    }

}