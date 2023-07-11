using Unity.VisualScripting;
using UnityEngine;

[UnitTitle("ChannelEvent")]
[UnitCategory("Events")]
public class ChannelEvent : EventUnit<EmptyEventArgs>
{
    [UnitHeaderInspectable("Channel")]
    public Channel Channel { get; set; }

    protected override bool register => true;

    public override EventHook GetHook(GraphReference reference)
    {
        return new EventHook(GetChannelEventName(Channel));
    }

    private string GetChannelEventName(Channel channel)
    {
        switch (channel)
        {
            case Channel.Channel1:
                return CommunityEvents.ChannelEvent1;
            case Channel.Channel2:
                return CommunityEvents.ChannelEvent2;
            case Channel.Channel3:
                return CommunityEvents.ChannelEvent3;
            case Channel.Channel4:
                return CommunityEvents.ChannelEvent4;
            case Channel.Channel5:
                return CommunityEvents.ChannelEvent5;
            case Channel.Channel6:
                return CommunityEvents.ChannelEvent6;
            case Channel.Channel7:
                return CommunityEvents.ChannelEvent7;
            case Channel.Channel8:
                return CommunityEvents.ChannelEvent8;
            case Channel.Channel9:
                return CommunityEvents.ChannelEvent9;
            case Channel.Channel10:
                return CommunityEvents.ChannelEvent10;
            default:
                Debug.LogWarning("Unknown channel value: " + channel);
                return null;
        }
    }

    protected override void Definition()
    {
        base.Definition();
    }
}
